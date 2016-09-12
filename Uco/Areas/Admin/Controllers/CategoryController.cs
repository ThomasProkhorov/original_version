using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Kendo.Mvc.UI;
using Kendo.Mvc.Extensions;
using Uco.Infrastructure;
using Uco.Models;
using System.Data.Entity;
using System.IO;
using System.Text;

using Uco.Infrastructure.Livecycle;
using OfficeOpenXml;
using ExportToExcel;
using Uco.Infrastructure.Repositories;

namespace Uco.Areas.Admin.Controllers
{
    [AuthorizeAdmin]
    public class CategoryController : BaseAdminController
    {
        public ActionResult Index()
        {
            ViewBag.MessageRed = TempData["MessageRed"];
            ViewBag.MessageYellow = TempData["MessageYellow"];
            ViewBag.MessageGreen = TempData["MessageGreen"];
            return View();
        }

        public ActionResult _AjaxIndex([DataSourceRequest]DataSourceRequest request)
        {
            IQueryable<Category> items = _db.Categories.AsQueryable();
            DataSourceResult result = items.ToDataSourceResult(request);

            var allCategories = LS.Get<Category>();
            foreach (var c in (IEnumerable<Category>)result.Data)
            {
                if (c.ParentCategoryID > 0)
                {
                    var pc = allCategories.FirstOrDefault(x => x.ID == c.ParentCategoryID);
                    if (pc != null)
                    {
                        c.ParentCategory = pc;
                    }
                }
                //cached product count
                c.CachedProductCount = LS.GetCachedFunc(() =>
                {
                    var cid = c.ID;
                    return _db.Products.Where(x => x.Deleted == false && x.CategoryID == cid).Count();

                }, string.Format("productsInCategory-{0}.", c.ID), 60);
            }
            return Json(result);
        }

        public ActionResult MoveProducts(int from, int to)
        {
            if (from != to)
            {
                var products = _db.Products.Where(x => x.CategoryID == from).ToList();
                foreach (var p in products)
                {
                    p.CategoryID = to;
                }
                _db.SaveChanges();
                return Json(new { message = RP.S("Admin.Category.ProductsMoved") });
            }

            return Json(new { message = RP.S("Admin.Category.ItsSameCategory") });
        }

        [AcceptVerbs(HttpVerbs.Post)]
        public ActionResult _AjaxDelete(Category item, [DataSourceRequest]DataSourceRequest request)
        {
            if (ModelState.IsValid)
            {
                _db.Categories.Remove(_db.Categories.First(r => r.ID == item.ID));
                _db.SaveChanges();
                CleanCache.CleanOutputCache();
            }
            return Json(new[] { item }.ToDataSourceResult(request, ModelState));
        }

        public ActionResult Create()
        {
            return View(new Category());
        }

        [HttpPost]
        public ActionResult Create(Category item)
        {
            if (ModelState.IsValid)
            {

                _db.Categories.Add(item);
                _db.SaveChanges();


                return RedirectToAction("Index");
            }
            return View(item);
        }

        public ActionResult Edit(int ID)
        {
            return View(_db.Categories.First(r => r.ID == ID));
        }

        [HttpPost]
        public ActionResult Edit(int ID, Category item)
        {
            if (ModelState.IsValid)
            {
                _db.Entry(item).State = EntityState.Modified;
                _db.SaveChanges();


                return RedirectToAction("Index");
            }
            return View(item);
        }

        #region ImportExport

        //settings for csv
        private string csvDlm = ",";
        private string csvQuote = "\"";


        [ValidateInput(false)]
        [FormValueRequired("import")]
        [HttpPost]
        public ActionResult Import(HttpPostedFileBase attachment)
        {
            if (attachment == null)
            {
                // TempData["MessageRed"] = "File Error";
                return Json(new { success = "error", message = "File Error" });
            }

            string FolderPath = Server.MapPath("~/App_Data/Temp/");
            //1
            string FileName = "";
            string FilePath = "";

            FileName = Path.GetFileName(attachment.FileName);
            FilePath = Path.Combine(FolderPath, FileName);
            attachment.SaveAs(FilePath);
            if (!FileName.EndsWith(".xls") && !FileName.EndsWith(".xlsx") && !FileName.EndsWith(".csv"))
            {
                return Json(new { success = "error", message = "File must have extension : xls, xlsx, csv" });

            }

            var existingFile = new FileInfo(FilePath);
            var all = new List<Dictionary<int, string>>();
            // Open and read the XlSX file.
            using (var package = new ExcelPackage(existingFile))
            {
                // Get the work book in the file
                ExcelWorkbook workBook = package.Workbook;
                if (workBook != null)
                {
                    if (workBook.Worksheets.Count > 0)
                    {
                        // Get the first worksheet
                        ExcelWorksheet currentWorksheet = workBook.Worksheets.First();

                        var begin = currentWorksheet.Dimension.Start;
                        var end = currentWorksheet.Dimension.End;
                        for (var i = begin.Row + 1; i <= end.Row; i++)
                        {

                            // read some data
                            Dictionary<int, string> row = new Dictionary<int, string>();
                            for (var c = begin.Column; c <= end.Column; c++)
                            {
                                var val = currentWorksheet.Cells[i, c].Value;
                                row[c - 1] = val != null ? val.ToString() : "";

                            }
                            all.Add(row);
                        }

                    }

                }

            }

            var AllCategories = LS.Get<Category>(); //get category cache
            List<Category> treeLevelParent = new List<Category>();
            foreach (var s in all)
            {
                if (s.Count != 2)
                {
                    continue;
                }
                if (s[0] != null && !string.IsNullOrEmpty(s[0].ToString()))
                {
                    var categoryParent = new Category();
                    categoryParent.DisplayOrder = 500;
                    categoryParent.Name = s[0].ToString().Trim();
                    categoryParent.Published = true;
                    categoryParent.ParentCategoryID = 0;
                    treeLevelParent.Add(categoryParent);
                }

            }
            foreach (var c in treeLevelParent)
            {
                if (!AllCategories.Any(x => x.Name == c.Name))
                {
                    _db.Categories.Add(c);
                    AllCategories.Add(c);
                }
            }
            _db.SaveChanges(); //save parent categories
            List<Category> treeLevelChild = new List<Category>();
            AllCategories = LS.Get<Category>();
            foreach (var s in all)
            {
                if (s[1] != null && !string.IsNullOrEmpty(s[1].ToString()))
                {
                    var category = new Category();
                    category.DisplayOrder = 1000;
                    category.Name = s[1].ToString().Trim();
                    category.Published = true;
                    category.ParentCategoryID = 0;
                    if (s[0] != null && !string.IsNullOrEmpty(s[0].ToString()))
                    {
                        var parentName = s[0].ToString().Trim();
                        var parentCat = AllCategories.FirstOrDefault(x => x.Name == parentName);
                        if (parentCat != null)
                        {
                            category.ParentCategoryID = parentCat.ID;
                        }
                    }
                    //  category.ParentCategory = categoryParent; //save parent for ID
                    treeLevelChild.Add(category);
                }
            }
            foreach (var c in treeLevelChild)
            {
                if (!AllCategories.Any(x => x.Name == c.Name))
                {
                    _db.Categories.Add(c);
                    AllCategories.Add(c);
                }
            }
            _db.SaveChanges(); //save categories
            return Json(new { success = "ok", message = "Import Success" });
            //TempData["MessageGreen"] = "Import Success";
            // return Content("");
        }


        [ValidateInput(false)]
        [FormValueRequired("export")]
        [HttpPost, ActionName("Export")]
        public ActionResult Export()
        {

            byte[] bytes = null;

            //string LangCode = "en-US";
            using (var stream = new MemoryStream())
            {
                //  _exportManager.ExportProductsToXlsx(stream, products);
                StreamWriter sw = new StreamWriter(stream, Encoding.UTF8);
                var list = _db.Products.AsNoTracking().ToList();
                var cats = LS.Get<Category>();

                var neededData = cats.Where(x => x.ParentCategoryID > 0).Select
                    (
                    x => new
                    {
                        Category = (cats.FirstOrDefault(y => y.ID == x.ParentCategoryID) != null
                      ? cats.FirstOrDefault(y => y.ID == x.ParentCategoryID).Name : ""),
                        SubCategory = x.Name,

                    }
                    ).ToList();
                neededData.AddRange(cats.Where(x => x.ParentCategoryID == 0).Select
                    (
                    x => new
                    {
                        Category = x.Name,
                        SubCategory = "",

                    }
                    ));

                bytes = CreateExcelFile.CreateExcelDocument(
                   neededData, "Category_" + DateTime.Now.ToString() + ".xlsx"
                   , HttpContext.Response);

                return File(bytes
                    , "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet"
                    , "Category_" + DateTime.Now.ToString() + ".xlsx");
            }

        }

        #endregion
    }
}

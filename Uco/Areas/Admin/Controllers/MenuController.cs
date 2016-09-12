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
using Uco.Infrastructure.Repositories;
using System.IO;
using System.Text;


namespace Uco.Areas.Admin.Controllers
{
    [AuthorizeAdmin]
    public class MenuController : BaseAdminController
    {
        private List<MenuModel> GetChildrens(int ID, List<MenuModel> menuItems)
        {
            var model = menuItems.Where(x => x.ParentID == ID).ToList();
            model.ForEach(x =>
            {
                x.Childrens = GetChildrens(x.ID, menuItems);//.Where(m => m.ParentID == x.ID).ToList();
            });
            return model;
        }
        public ActionResult Index(string lang, string group)
        {
            ViewBag.MessageRed = TempData["MessageRed"];
            ViewBag.MessageYellow = TempData["MessageYellow"];
            ViewBag.MessageGreen = TempData["MessageGreen"];

            if (string.IsNullOrEmpty(lang)) lang = SF.GetLangCodeThreading();
            ViewBag.Lang = lang;

            if (string.IsNullOrEmpty(group)) group = SF.GetFirstMenuGroup();
            ViewBag.Group = group;

            var menuItems = _db.Menus.Where(x => x.Group == group && x.LangCode == lang).OrderBy(x => x.DisplayOrder).AsNoTracking().ToList();
            var model = menuItems.Where(x => x.ParentID == 0).ToList();
            model.ForEach(x =>
            {
                x.Childrens = GetChildrens(x.ID, menuItems);//.Where(m => m.ParentID == x.ID).ToList();
            });
            return View(model);
        }

        [HttpPost]
        public ActionResult AjaxInsert(MenuModel item)
        {

            _db.Menus.Add(item);
            _db.SaveChanges();
            return Json(new { message = "save success", newItem = item });
        }
        [HttpPost]
        public ActionResult AjaxDelete(MenuModel model)
        {
            var item = _db.Menus.FirstOrDefault(x => x.ID == model.ID);
            if (item != null)
            {

                _db.Menus.Remove(item);
                _db.SaveChanges();
            }
            return Json(new { message = "save success" });
        }
        [HttpPost]
        public ActionResult AjaxSaveOrder(MenuModel model)
        {
            var item = _db.Menus.FirstOrDefault(x => x.ID == model.ID);
            if (item != null)
            {

                item.DisplayOrder = model.DisplayOrder;
                _db.SaveChanges();
            }
            return Json(new { message = "save success" });
        }
        [HttpPost]
        public ActionResult AjaxSaveParent(MenuModel model)
        {
            var item = _db.Menus.FirstOrDefault(x => x.ID == model.ID);
            if (item != null)
            {
                item.ParentID = model.ParentID;
                item.DisplayOrder = model.DisplayOrder;
                _db.SaveChanges();
            }
            return Json(new { message = "save success" });
        }
        [HttpPost]
        public ActionResult AjaxSave(MenuModel item)
        {

            _db.Entry(item).State = EntityState.Modified;
            _db.SaveChanges();
            return Json(new { message = "save success" });
        }
        //settings for csv
        private string csvDlm = ",";
        private string csvQuote = "\"";

        [ValidateInput(false)]
        [FormValueRequired("export")]
        [HttpPost, ActionName("Export")]
        public ActionResult Export(string Languages = "en-US")
        {

            byte[] bytes = null;

            //string LangCode = "en-US";
            using (var stream = new MemoryStream())
            {
                //  _exportManager.ExportProductsToXlsx(stream, products);
                StreamWriter sw = new StreamWriter(stream, Encoding.UTF8);
                var list = _db.Translations.Where(x => x.LangCode == Languages).AsNoTracking().ToList();
                foreach (var t in list)
                {
                    sw.Write(csvQuote + t.SystemName + csvQuote + csvDlm);
                    sw.Write(csvQuote + t.Text
                        .Replace("\n", "&sp;")
                        .Replace(csvQuote, "&quot;") + csvQuote + csvDlm);
                    sw.WriteLine();

                }

                sw.Flush();
                stream.Flush();
                stream.Position = 0;
                //Encoding heb = Encoding.GetEncoding("windows-1255");

                bytes = Encoding.UTF8.GetBytes(new StreamReader(stream).ReadToEnd());

            }
            return File(bytes, "text/csv", "contactforms.csv");
        }
        [ValidateInput(false)]
        [FormValueRequired("import")]
        [HttpPost, ActionName("Export")]
        public ActionResult Import(HttpPostedFileBase attachment, string Languages = "en-US")
        {
            if (attachment == null)
            {
                return Json(new { success = "error", message = "File Error" });
                //TempData["MessageRed"] = "File Error";
                //return RedirectToAction("Index");
            }

            string FolderPath = Server.MapPath("~/App_Data/Temp/");
            string FileName = "";
            string FilePath = "";

            FileName = Path.GetFileName(attachment.FileName);
            FilePath = Path.Combine(FolderPath, FileName);
            attachment.SaveAs(FilePath);
            if (!FileName.EndsWith(".csv"))
            {
                return Json(new { success = "error", message = "File must have extension : csv" });
                // TempData["MessageRed"] = "File must have extension : xls, xlsx, csv";
                //return RedirectToAction("Index");
            }
            // var excel = new ExcelQueryFactory(FilePath);
            var content = System.IO.File.ReadAllText(FilePath);
            string newLine = Environment.NewLine;
            string[] lines = content.Split(new string[] { newLine }, StringSplitOptions.RemoveEmptyEntries);
            var list = _db.Translations.Where(x => x.LangCode == Languages).ToList();

            foreach (var row in lines)
            {
                if (!string.IsNullOrEmpty(row))
                {
                    var rowObj = row.TrimStart(csvQuote.ToCharArray()).TrimEnd((csvQuote + csvDlm).ToCharArray());
                    string[] values = rowObj.Split(new string[] { csvQuote + csvDlm + csvQuote }, StringSplitOptions.None);
                    if (values.Length > 1)
                    {
                        var key = values[0];
                        var value = values[1]
                            .Replace("&sp;", "\n")
                       .Replace("&quot;", csvQuote);
                        var upd = list.FirstOrDefault(x => x.SystemName == key);
                        if (upd != null)
                        {
                            //update
                            upd.Text = value;
                            // _db.SaveChanges();
                        }
                        else
                        {
                            //insert new
                            var resString = new Translation()
                            {
                                SystemName = key,
                                Text = value
                            };
                            resString.LangCode = Languages;
                            _db.Translations.Add(resString);

                        }
                    }
                }
            }
            _db.SaveChanges();
            return Json(new { success = "ok", message = "Import Success" });

            return Content("");
        }
        public ActionResult _AjaxIndex([DataSourceRequest]DataSourceRequest request, string Lang)
        {
            IQueryable<Translation> items = _db.Translations.Where(r => r.LangCode == Lang);
            DataSourceResult result = items.ToDataSourceResult(request);
            return Json(result);
        }

        [AcceptVerbs(HttpVerbs.Post)]
        public ActionResult _AjaxDelete(Translation item, [DataSourceRequest]DataSourceRequest request, string Lang)
        {
            if (ModelState.IsValid)
            {
                _db.Translations.Remove(_db.Translations.First(r => r.ID == item.ID && r.LangCode == Lang));
                _db.SaveChanges();
                RP.CleanTranslationsRepository();
            }
            return Json(new[] { item }.ToDataSourceResult(request, ModelState));
        }

        [AcceptVerbs(HttpVerbs.Post)]
        public ActionResult _AjaxInsert([DataSourceRequest] DataSourceRequest request, Translation item, string Lang)
        {
            if (ModelState.IsValid)
            {
                item.LangCode = Lang;
                _db.Translations.Add(item);
                _db.SaveChanges();

                RP.CleanTranslationsRepository();
            }
            return Json(new[] { item }.ToDataSourceResult(request, ModelState));
        }

        [AcceptVerbs(HttpVerbs.Post)]
        public ActionResult _AjaxSave([DataSourceRequest] DataSourceRequest request, Translation item, string Lang)
        {
            if (ModelState.IsValid)
            {
                item.LangCode = Lang;
                _db.Entry(item).State = EntityState.Modified;
                _db.SaveChanges();

                RP.CleanTranslationsRepository();
            }

            return Json(new[] { item }.ToDataSourceResult(request, ModelState));
        }
    }
}

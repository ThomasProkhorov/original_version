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

using Uco.Infrastructure.Livecycle;
using System.Threading.Tasks;
using System.Globalization;
using Uco.Infrastructure.EntityExtensions;
using System.Text.RegularExpressions;
using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Spreadsheet;
using ExportToExcel;
using OfficeOpenXml;
using NPOI.HSSF.UserModel;
using Uco.Infrastructure.Tasks;
using System.Security.Cryptography;
using System.Linq.Expressions;


namespace Uco.Areas.Admin.Controllers
{
    [AuthorizeAdmin]
    public class ProductController : BaseAdminController
    {
        public ActionResult Index()
        {

            return View(new Product());
        }

        public ActionResult MergeSku()
        {
            return View();
        }

        [HttpPost]
        [ValidateInput(false)]
        public ActionResult MergeSkuPost(HttpPostedFileBase attachment)
        {
            if (attachment == null)
            {
                // TempData["MessageRed"] = "File Error";
                return Json(new { success = "error", message = RP.S("Admin.Product.Import.Error.FileMissing") });
            }

            string FolderPath = Server.MapPath("~/App_Data/Import/");
            if (!Directory.Exists(FolderPath))
            {
                Directory.CreateDirectory(FolderPath);
            }
            //1
            string FileName = "";
            string FilePath = "";

            FileName = Path.GetFileName(attachment.FileName);
            FilePath = Path.Combine(FolderPath, FileName);


            if (!FileName.EndsWith(".xls") && !FileName.EndsWith(".xlsx") && !FileName.EndsWith(".csv"))
            {
                return Json(new { success = "error", message = RP.S("Admin.Product.Import.Error.FileExtensionMustBe.xls.xlsx.csv") });

            }
            attachment.SaveAs(FilePath);
            var existingFile = new FileInfo(FilePath);
            var all = new List<Dictionary<int, string>>();
            string message = RP.S("Admin.Product.Import.SeeError");
            StringBuilder errors = new StringBuilder();

            #region read file
            if (FileName.EndsWith(".xls"))
            {
                using (var fs = new FileStream(FilePath, FileMode.Open, FileAccess.Read))
                {
                    var wb = new HSSFWorkbook(fs);
                    // get sheet
                    var sh = (HSSFSheet)wb.GetSheetAt(0);
                    var head = sh.GetRow(0);
                    int colCount = 0;
                    if (head != null)
                    {
                        colCount = head.Cells.Count;
                    }
                    int i = 1;
                    while (sh.GetRow(i) != null)
                    {
                        var xlsRow = sh.GetRow(i);
                        // read some data
                        Dictionary<int, string> row = new Dictionary<int, string>();
                        // write row value
                        for (int j = 0; j < colCount; j++)
                        {
                            var cell = xlsRow.GetCell(j);

                            if (cell != null)
                            {

                                // TODO: you can add more cell types capability, e. g. formula
                                switch (cell.CellType)
                                {
                                    case NPOI.SS.UserModel.CellType.Numeric:
                                        var valN = cell.NumericCellValue;
                                        row[j] = valN.ToString();
                                        break;
                                    case NPOI.SS.UserModel.CellType.String:
                                        var val = cell.StringCellValue;
                                        row[j] = val != null ? val : "";
                                        break;
                                    default:
                                        row[j] = "";
                                        break;
                                }
                            }
                            else
                            {
                                row[j] = "";
                            }
                        }
                        all.Add(row);
                        i++;
                    }
                }
            }
            else
            {
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
            }
            #endregion

            foreach (var row in all)
            {
                if (row[6] != null && row[6] == "1")
                {
                    //merge
                    int oldProductID = 0;
                    int newProductID = 0;
                    int.TryParse(row[0], out newProductID);
                    int.TryParse(row[3], out oldProductID);
                    if (newProductID > 0 && oldProductID > 0)
                    {
                        var shopProducts = LS.CurrentEntityContext.ProductShopMap.Where(x => x.ProductID == oldProductID).ToList();
                        foreach (var ps in shopProducts)
                        {
                            ps.ProductID = newProductID;
                        }
                        LS.CurrentEntityContext.SaveChanges();
                        var product = LS.CurrentEntityContext.Products.FirstOrDefault(x => x.ID == oldProductID);
                        if (product != null)
                        {
                            LS.CurrentEntityContext.Products.Remove(product);
                            LS.CurrentEntityContext.SaveChanges();
                        }
                    }
                }
            }

            message = RP.S("Admin.Product.Import.Success");
            return Json(new { success = "ok", message = message, errors = errors.ToString() });

        }
        public ActionResult CopyUnusedImages()
        {
            StringBuilder StatusInfoSB = new StringBuilder();
            StatusInfoSB.AppendLine("Processing unused images...");

            //get all Images from Products           
            var ProductImages = _db.Products.Where(p => p.HasImage == true && !String.IsNullOrEmpty(p.Image))
                                            .Select(p => p.Image).ToList()
                                            .Select(p => HttpContext.Server.MapPath("~" + p));

            //get all Images from image folder
            string ImagePath = "/Content/ImportedProductImages/";
            ImagePath = HttpContext.Server.MapPath("~" + ImagePath);
            if (Directory.Exists(ImagePath))
            {
                var ImageEntries = Directory.GetFiles(ImagePath);

                //get all images that are not listed in db
                var UnusedImages = ImageEntries.Except(ProductImages);

                //move unused images to another folder
                string MoveToPath = "/Content/UnusedImages/";
                MoveToPath = HttpContext.Server.MapPath("~" + MoveToPath);
                if (!Directory.Exists(MoveToPath))
                    Directory.CreateDirectory(MoveToPath);

                if (UnusedImages.Count() > 0)
                    StatusInfoSB.AppendLine("Unused Images found: " + UnusedImages.Count());

                foreach (var file in UnusedImages)
                {
                    string TargetFile = Path.Combine(MoveToPath, Path.GetFileName(file));
                    if (System.IO.File.Exists(TargetFile))
                        System.IO.File.Delete(TargetFile);

                    System.IO.File.Copy(file, TargetFile);
                    StatusInfoSB.AppendLine(file);
                }

                if (UnusedImages.Count() > 0)
                    StatusInfoSB.AppendLine("All Images were moved to: " + MoveToPath);

            }
            StatusInfoSB.AppendLine("").AppendLine("Done!");

            //return Content(StatusInfo);
            ViewBag.StatusInfo = StatusInfoSB.ToString();
            return View();
        }
        #region import status

        public ActionResult Status(string type)
        {
            var task = _db.PlanedTasks.AsNoTracking().FirstOrDefault(x => x.SystemName == type);
            if (task != null)
            {
                if (task.Active)
                {
                    return Json(new { message = task.Message, next = true, progress = task.PercentProgress }, JsonRequestBehavior.AllowGet);
                }
                return Json(new { message = task.Message, next = false, progress = task.PercentProgress }, JsonRequestBehavior.AllowGet);
            }
            return Json(new { message = "Not found", next = false, progress = 100 }, JsonRequestBehavior.AllowGet);
        }
        #endregion

        #region autocomplite
        public ActionResult _AjaxDetailAutoComplete([DataSourceRequest]DataSourceRequest request, string text)
        {
            var items = LS.Get<ProductSmall>(
                "ProductSmall", false, () =>
                {
                    return _db.Products.AsNoTracking()
                        .Select(x => new ProductSmall()
                        {
                            ID = x.ID,
                            Image = x.Image,
                            Name = x.Name,
                            SKU = x.SKU
                        })
                        .ToList();
                },
                "Product"
                ).AsQueryable();


            if (!string.IsNullOrEmpty(text))
            {
                items = items.Where(x => (x.SKU != null && x.SKU.Contains(text))
                     || (x.Name != null && x.Name.Contains(text))
                    );

            }
            request.PageSize = 100;
            DataSourceResult result = items.ToDataSourceResult(request);
            return Json(result);
        }
        #endregion

        #region ImportExport

        private static bool IsFixRunned = false;
        public ActionResult ImageFixImport()
        {
            if (!IsFixRunned)
            {
                Task.Factory.StartNew(() =>
                {
                    try
                    {
                        IsFixRunned = true;
                        var _db = new Db();
                        int lastID = 0;
                        bool ifProductLeft = true;
                        int porcionLimit = 1000;
                        Dictionary<int, string> idImageMap = new Dictionary<int, string>();

                        while (ifProductLeft)
                        {

                            if (idImageMap.Count > 0)
                            {
                                //run sql command, optimized
                                StringBuilder sql = new StringBuilder();
                                foreach (var im in idImageMap)
                                {
                                    sql.AppendLine(string.Format("UPDATE [{0}] SET [Image] = N'{2}'  WHERE [Id] = {1} ",
                                        "Products",
                                        im.Key,
                                        im.Value.Replace("'", @"''") //fix string insert
                                        ));
                                }
                                _db.Database.ExecuteSqlCommand(sql.ToString());
                                idImageMap.Clear();
                            }

                            var productsList = _db.Products
                  .Where(x => x.ID > lastID)
                 .Take(porcionLimit)
                  .OrderBy(x => x.ID)
                  .Select(x => new { x.ID, x.SKU, x.Name, x.Image }) // optimization
                  .ToList();


                            var productsToUpdateImage = productsList.Where(x => x.Image != null
                                && x.Image.Contains("/Content/ImportedProductImages/")
                                && Regex.Replace(Path.GetFileNameWithoutExtension(x.Image), "[^0-9]", "").Length < 5
                              ).ToList();
                            foreach (var p in productsToUpdateImage)
                            {
                                idImageMap[p.ID] = "";
                            }
                            if (productsList.Count >= porcionLimit)
                            {
                                ifProductLeft = true; // products left
                                lastID = productsList.LastOrDefault().ID;
                            }
                            else
                            {
                                ifProductLeft = false;
                            }
                        }
                        if (idImageMap.Count > 0)
                        {
                            //run sql command, optimized
                            StringBuilder sql = new StringBuilder();
                            foreach (var im in idImageMap)
                            {
                                sql.AppendLine(string.Format("UPDATE [{0}] SET [Image] = N'{2}'  WHERE [Id] = {1} ",
                                    "Products",
                                    im.Key,
                                    im.Value.Replace("'", @"''") //fix string insert
                                    ));
                            }
                            _db.Database.ExecuteSqlCommand(sql.ToString());
                            idImageMap.Clear();
                        }
                    }
                    finally
                    {
                        IsFixRunned = false;
                    }
                    return;
                });
                return Json(new { success = "ok", message = RP.S("Admin.ProductImage.Import.TaskRunned") }, JsonRequestBehavior.AllowGet);
            }
            return Json(new { success = "ok", message = RP.S("Admin.ProductImage.Import.TaskAlreadyInProcess") }, JsonRequestBehavior.AllowGet);

        }

        //settings for image import
        private static bool IsRunned = false;
        private object _imageImport_lock = new object();
        private string _newProducImageFolder = "/Content/NewProductImages/";
        private string _importedProducImageFolder = "/Content/ImportedProductImages/";
        [ValidateInput(false)]
        [FormValueRequired("import")]
        [HttpPost]
        public ActionResult ImageImport()
        {
            if (!IsRunned)
            {
                Task.Factory.StartNew(() =>
                {
                    try
                    {
                        IsRunned = true;
                        var _db = new Db();
                        string newProductImageFolderPath = Server.MapPath("~" + _newProducImageFolder);
                        var newImages = Directory.GetFiles(newProductImageFolderPath, "*.*")
                           .Where(s => s.ToLower().EndsWith(".jpg") || s.ToLower().EndsWith(".jpeg")
                               || s.ToLower().EndsWith(".png") || s.ToLower().EndsWith(".gif"))
                                                  .Select(path => Path.GetFileName(path))
                                                  .ToArray();

                        Dictionary<string, string> skuImageMap = new Dictionary<string, string>();
                        Dictionary<int, string> idImageMap = new Dictionary<int, string>();
                        foreach (var s in newImages)
                        {
                            var name = Regex.Replace(Path.GetFileNameWithoutExtension(s), "[^0-9]", "");
                            if (!string.IsNullOrEmpty(name) && name.Length > 4)
                            {
                                skuImageMap[name] = s;
                            }
                        }
                        int porcionLimit = 1000; // get 1000 products from DB
                        int lastID = 0;
                        bool ifProductLeft = skuImageMap.Count > 0;

                        while (ifProductLeft)
                        {
                            if (idImageMap.Count > 0)
                            {
                                //run sql command, optimized
                                StringBuilder sql = new StringBuilder();
                                foreach (var im in idImageMap)
                                {
                                    sql.AppendLine(string.Format("UPDATE [{0}] SET [Image] = N'{2}'  WHERE [Id] = {1} ",
                                        "Products",
                                        im.Key,
                                        im.Value.Replace("'", @"''") //fix string insert
                                        ));
                                }
                                _db.Database.ExecuteSqlCommand(sql.ToString());
                                idImageMap.Clear();
                            }

                            ifProductLeft = false;//cycle prevent
                            var productsList = _db.Products
                                .Where(x => x.ID > lastID)
                               .Take(porcionLimit)
                                .OrderBy(x => x.ID)
                                .Select(x => new { x.ID, x.SKU, x.Name, x.Image }) // optimization
                                .ToList();
                            //process import image
                            //parallel optimization
                            var removeList = new List<string>();
                            object _lock = new object();
                            Parallel.ForEach(skuImageMap, s =>
                        {
                            var productsToUpdateImage = productsList.Where(x => x.SKU == s.Key
                                //|| (x.SKU!=null && x.SKU.Length > 5 && x.SKU.StartsWith(s.Key))
                                 || (x.SKU != null && x.SKU.Length > 5 && x.SKU.EndsWith(s.Key))
                                 || x.Name == s.Key
                                ).ToList();
                            var newphysicalPath = Path.Combine(newProductImageFolderPath, s.Value);
                            string ProductImageFolder = "~" + _importedProducImageFolder;
                            string ProductImageFolderPath = Server.MapPath(ProductImageFolder);
                            var physicalPath = Path.Combine(ProductImageFolderPath, s.Value);

                            foreach (var p in productsToUpdateImage)

                            //if (p != null) // founded
                            {

                                if (p.Image != null && Regex.Replace(Path.GetFileNameWithoutExtension(p.Image), "[^0-9]", "").Length > s.Key.Length)
                                {
                                    continue;
                                }


                                if (p.Image != _importedProducImageFolder + s.Value)
                                {


                                    lock (_lock)
                                    {
                                        if (System.IO.File.Exists(newphysicalPath))
                                        {
                                            System.IO.File.Copy(newphysicalPath, physicalPath, true);
                                            //update image
                                            idImageMap[p.ID] = _importedProducImageFolder + s.Value;
                                            //deleting new file
                                            //System.IO.File.Delete(newphysicalPath);
                                        }
                                    }
                                }
                                else
                                {
                                    if (System.IO.File.Exists(newphysicalPath))
                                    {
                                        var sourceMd5 = "";
                                        var destMd5 = "";
                                        using (var md5 = MD5.Create())
                                        {
                                            using (var stream = System.IO.File.OpenRead(newphysicalPath))
                                            {
                                                sourceMd5 = Encoding.Default.GetString(md5.ComputeHash(stream));
                                            }
                                            if (System.IO.File.Exists(physicalPath))
                                            {
                                                using (var stream = System.IO.File.OpenRead(physicalPath))
                                                {
                                                    destMd5 = Encoding.Default.GetString(md5.ComputeHash(stream));
                                                }
                                            }
                                        }
                                        if (sourceMd5 != destMd5)
                                        {
                                            lock (_lock)
                                            {

                                                System.IO.File.Copy(newphysicalPath, physicalPath, true);
                                                //update image
                                                idImageMap[p.ID] = _importedProducImageFolder + s.Value;
                                                //deleting new file
                                                //System.IO.File.Delete(newphysicalPath);

                                            }
                                        }
                                    }
                                    //deleting new file
                                    //System.IO.File.Delete(newphysicalPath); //already in
                                }

                                lock (_lock)
                                {
                                    removeList.Add(s.Key);
                                }

                            }

                            //IF YOU NEED DELETE all wrong mapped image, run this
                            //productsToUpdateImage = productsList.Where(x => x.Image!=null && Regex.Replace(Path.GetFileNameWithoutExtension(x.Image), "[^0-9]", "").Length < 2
                            //  ).ToList();
                            // foreach (var p in productsToUpdateImage)
                            // {
                            //     idImageMap[p.ID] = "";
                            // }


                            //_imageImport_lock
                        });
                            foreach (var s in removeList)
                            {
                                // skuImageMap.Remove(s);//remove for counting progrees and optimization
                            }

                            //do each if need
                            if (productsList.Count >= porcionLimit)
                            {
                                ifProductLeft = true; // products left
                                lastID = productsList.LastOrDefault().ID;
                            }

                        }
                        //run last update
                        if (idImageMap.Count > 0)
                        {
                            //run sql command, optimized
                            StringBuilder sql = new StringBuilder();
                            foreach (var im in idImageMap)
                            {
                                sql.AppendLine(string.Format("UPDATE [{0}] SET [Image] = N'{2}'  WHERE [Id] = {1} ",
                                    "Products",
                                    im.Key,
                                    im.Value.Replace("'", @"''") //fix string insert
                                    ));
                            }
                            _db.Database.ExecuteSqlCommand(sql.ToString());
                            idImageMap.Clear();
                        }
                    }
                    finally
                    {
                        IsRunned = false;
                    }
                    return;
                });
                return Json(new { success = "ok", message = RP.S("Admin.ProductImage.Import.TaskRunned") });
            }
            return Json(new { success = "ok", message = RP.S("Admin.ProductImage.Import.TaskAlreadyInProcess") });

        }


        private class PIsheme
        {
            /// <summary>
            /// 0
            /// </summary>
            public int SKU = 0;
            /// <summary>
            /// 1
            /// </summary>
            public int Name = 1;
            /// <summary>
            /// 2
            /// </summary>
            public int IsKosher = 2;
            /// <summary>
            /// 3
            /// </summary>
            public int KosherType = 3;
            /// <summary>
            /// 4
            /// </summary>
            public int Capacity = 4;
            /// <summary>
            /// 5
            /// </summary>
            public int MeasureUnit = 5;
            /// <summary>
            /// 6
            /// </summary>
            public int Components = 6;
            /// <summary>
            /// 7
            /// </summary>
            public int UnitPerPackage = 7;
            ///// <summary>
            ///// 7
            ///// </summary>
            //public int Category = 7;
            /// <summary>
            /// 8
            /// </summary>
            public int SubCategory = 8;
            /// <summary>
            /// 9
            /// </summary>
            public int Manufacturer = 9;
            /// <summary>
            /// 10
            /// </summary>
            public int Tax = 10;
            /// <summary>
            /// 11
            /// </summary>
            public int Price = 11;
            /// <summary>
            /// 12
            /// </summary>
            public int SoldByWeight = 12;
            /// <summary>
            /// 13 (0, 1, 2, 3)
            /// </summary>
            public int FlagOperation = 13;
            /// <summary>
            /// 14
            /// </summary>
            public int ShortDescription = 14;
            /// <summary>
            /// 15
            /// </summary>
            public int FullDescription = 15;
            /// <summary>
            /// 16
            /// </summary>
            public int ProductOptions = 16;

        }
        [ValidateInput(false)]
        [HttpPost]
        public ActionResult ProductImportWithFlag(HttpPostedFileBase attachment)
        {
            if (attachment == null)
            {
                // TempData["MessageRed"] = "File Error";
                return Json(new { success = "error", message = RP.S("Admin.Product.Import.Error.FileMissing") });
            }

            string FolderPath = Server.MapPath("~/App_Data/Import/");
            if (!Directory.Exists(FolderPath))
            {
                Directory.CreateDirectory(FolderPath);
            }
            //1
            string FileName = "";
            string FilePath = "";

            FileName = Path.GetFileName(attachment.FileName);
            FilePath = Path.Combine(FolderPath, FileName);


            if (!FileName.EndsWith(".xls") && !FileName.EndsWith(".xlsx") && !FileName.EndsWith(".csv"))
            {
                return Json(new { success = "error", message = RP.S("Admin.Product.Import.Error.FileExtensionMustBe.xls.xlsx.csv") });

            }

            var task = _db.PlanedTasks.FirstOrDefault(x => x.SystemName == "ProductAdminImport");
            if (task == null)
            {
                task = new PlanedTask()
                {
                    SystemName = "ProductAdminImport",

                };
                _db.PlanedTasks.Add(task);
                _db.SaveChanges();
            }
            var responseMessage = RP.S("Admin.Product.Import.TaskStarted");
            if (!task.Active)
            {
                task.Active = true;
                task.ProcessData = FileName;
                task.PercentProgress = 5;
                task.Message = RP.S("Admin.PlanedTask.Queued");
                _db.SaveChanges();
                attachment.SaveAs(FilePath);

            }
            else
            {
                responseMessage = RP.S("Admin.Product.Import.TaskAlreadyInProcess");
            }
            if (!SF.UseTasks())
            {
                new PlanedProcessTask().StartNewInThread("ProductAdminImport");
            }
            return Json(new { success = "ok", message = responseMessage });


        }
        public ActionResult SkuReport()
        {
            var count = _db.Products.Count();
            return View(count);
        }

        public ActionResult GetSkuReport(int pos, int limit)
        {
            if (limit > 5000)
            {
                limit = 5000;
            }
            var products = _db.Products.AsNoTracking()
                .OrderBy(x => x.ID)
                .Skip(pos)
                .Take(limit)
                .Select(x => new ProductSmall()
                {
                    ID = x.ID,
                    Name = x.Name,
                    SKU = x.SKU
                }).ToList();
            var report = new List<ProductReport>();

            foreach (var p in products)
            {
                bool validRow = true;
                bool InvalidCheckSum = false;
                string suggestsku = "";
                if (string.IsNullOrEmpty(p.SKU))
                {
                    validRow = false;
                }
                var sku = p.SKU;
                if (validRow && (sku.Length == 13 || sku.Length == 12))
                {
                    // 036000291454x
                    #region checksum
                    int codelength = 12;
                    if (sku.Length == 12)
                    {
                        // 03600029145x
                        codelength = 11;
                    }
                    int odd = 0;
                    int even = 0;
                    int check = 0;
                    int.TryParse(sku[codelength].ToString(), out check);

                    int tmp = 0;
                    for (int i = 0; i < codelength; i = i + 2)
                    {
                        int.TryParse(sku[i].ToString(), out tmp);
                        odd += tmp;
                    }
                    odd = odd * 3;
                    for (int i = 1; i < codelength; i = i + 2)
                    {
                        int.TryParse(sku[i].ToString(), out tmp);
                        even += tmp;
                    }
                    int sum = odd + even;
                    int module = sum % 10;
                    int mustBe = 10 - module;
                    if (mustBe != check)
                    {
                        validRow = false;
                        InvalidCheckSum = true;
                    }

                    #endregion
                }
                if (!string.IsNullOrEmpty(p.SKU) && sku.Length > 5 && sku.Length < 12)
                {
                    validRow = false;
                    var any = products.FirstOrDefault(x => x.SKU.EndsWith(sku) && x.ID != p.ID);
                    if (any != null)
                    {
                        suggestsku = any.SKU;

                    }
                }
                if (!string.IsNullOrEmpty(p.SKU) && sku.Length < 12)
                {
                    validRow = false;
                }
                if (!validRow)
                {
                    var reportItem = new ProductReport();
                    reportItem.ID = p.ID;
                    reportItem.InvalidCheckSum = InvalidCheckSum;
                    reportItem.Name = p.Name;
                    reportItem.SKU = p.SKU;
                    reportItem.SuggestedSKU = suggestsku;
                    report.Add(reportItem);
                }
            }

            byte[] bytes = null;

            //string LangCode = "en-US";
            using (var stream = new MemoryStream())
            {
                //  _exportManager.ExportProductsToXlsx(stream, products);
                StreamWriter sw = new StreamWriter(stream, Encoding.UTF8);


                bytes = CreateExcelFile.CreateExcelDocument(
                   report, "ProductReport_" + DateTime.Now.ToString() + ".xlsx"
                   , HttpContext.Response);

                return File(bytes
                    , "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet"
                    , "ProductReport_" + DateTime.Now.ToString() + ".xlsx");
            }

        }

        [ValidateInput(false)]
        [HttpPost]
        public ActionResult ProductImport(HttpPostedFileBase attachment)
        {
            if (attachment == null)
            {
                // TempData["MessageRed"] = "File Error";
                return Json(new { success = "error", message = RP.S("Admin.Product.Import.Error.FileMissing") });
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
                return Json(new { success = "error", message = RP.S("Admin.Product.Import.Error.FileExtensionMustBe.xls.xlsx.csv") });

            }

            // var excel = new ExcelQueryFactory(FilePath);
            //var all = from c in excel.Worksheet(0)
            //        select c;
            // Get the file we are going to process
            var existingFile = new FileInfo(FilePath);
            var all = new List<Dictionary<int, string>>();
            if (FileName.EndsWith(".xls"))
            {
                using (var fs = new FileStream(FilePath, FileMode.Open, FileAccess.Read))
                {
                    var wb = new HSSFWorkbook(fs);
                    // get sheet
                    var sh = (HSSFSheet)wb.GetSheetAt(0);
                    var head = sh.GetRow(0);
                    int colCount = 0;
                    if (head != null)
                    {
                        colCount = head.Cells.Count;
                    }
                    int i = 1;
                    while (sh.GetRow(i) != null)
                    {
                        var xlsRow = sh.GetRow(i);
                        // read some data
                        Dictionary<int, string> row = new Dictionary<int, string>();
                        // write row value
                        for (int j = 0; j < colCount; j++)
                        {
                            var cell = xlsRow.GetCell(j);

                            if (cell != null)
                            {

                                // TODO: you can add more cell types capability, e. g. formula
                                switch (cell.CellType)
                                {
                                    case NPOI.SS.UserModel.CellType.Numeric:
                                        var valN = cell.NumericCellValue;
                                        row[j] = valN.ToString();
                                        break;
                                    case NPOI.SS.UserModel.CellType.String:
                                        var val = cell.StringCellValue;
                                        row[j] = val != null ? val : "";
                                        break;
                                    default:
                                        row[j] = "";
                                        break;
                                }
                            }
                            else
                            {
                                row[j] = "";
                            }
                        }
                        all.Add(row);
                        i++;
                    }
                }
            }
            else
            {
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
            }
            //Dictionary<string, string> skuImageMap = new Dictionary<string, string>();
            //Dictionary<int, string> idImageMap = new Dictionary<int, string>();
            var productforInsert = new List<Product>();
            var productForUpdate = new List<Product>();
            var productForDelete = new List<Product>();
            var productsList = _db.Products
                    .OrderBy(x => x.ID)
                    .Select(x => new { x.ID, x.SKU, x.IgnoreOnImport }) // optimization
                    .ToList().ToDictionary(x => x.SKU);
            // var replaceSkuMap = _db.ProductSkuMaps.AsNoTracking().ToList();
            var decimalSeparator = CultureInfo.CurrentCulture.NumberFormat.NumberDecimalSeparator;
            var AllCategories = LS.Get<Category>(); //get category cache
            var AllManufacturers = LS.Get<Manufacturer>();
            PIsheme sheme = new PIsheme();
            int lineNum = 1;
            StringBuilder errors = new StringBuilder();
            StringBuilder lastEmpty = new StringBuilder();
            List<ProcessError> processErrors = new List<ProcessError>();
            bool needCreateCategoryIfNotExist = false;
            Dictionary<string, bool> processed = new Dictionary<string, bool>();
            foreach (var s in all)
            {
                lineNum++;
                if (lineNum == 2) { continue; }//skip second row (excell numeration)
                if (s.Count != 17)
                {
                    processErrors.Add(new ProcessError() { LineNum = lineNum, Message = string.Format(RP.S("Admin.Product.Import.Error.WrongColumnCount-LineNume{0}"), lineNum) });
                    errors.AppendLine(string.Format(RP.S("Admin.Product.Import.Error.WrongColumnCount-LineNume{0}"), lineNum));
                    continue;
                }
                string actionFlag = s[sheme.FlagOperation];
                bool isDelete = actionFlag == "0";
                bool isInsert = actionFlag == "1";
                bool isInsertOrUpdate = actionFlag == "2";
                if (!isDelete && !isInsertOrUpdate)
                {
                    isInsert = true;
                }
                long test = 0;
                bool validRow = true;
                bool SKUorNameEmpty = false;
                if (s[sheme.SKU] == null)
                {
                    SKUorNameEmpty = true;
                }
                else if (s[sheme.SKU].ToString().Trim() == "")
                {
                    SKUorNameEmpty = true;
                }
                else if (s[sheme.Name] == null)
                {
                    SKUorNameEmpty = true;
                }
                else if (s[sheme.Name] == "")
                {
                    SKUorNameEmpty = true;
                }
                else if (!long.TryParse(s[sheme.SKU].ToString(), out test))
                {
                    // SKUorNameEmpty = true;
                }

                if (SKUorNameEmpty)
                {
                    processErrors.Add(new ProcessError()
                    {
                        LineNum = lineNum
                   ,
                        Message = string.Format(RP.S("Admin.Product.Import.Error.SKUorNameEmpty-LineNume{0}"), lineNum)
                    });

                    lastEmpty.AppendLine(string.Format(RP.S("Admin.Product.Import.Error.SKUorNameEmpty-LineNume{0}"), lineNum));
                    continue;
                }
                //check SKU code
                var sku = s[sheme.SKU].ToString().Trim();
                sku = Regex.Replace(sku, "[^.0-9]", "");
                //check map table
                //var replaceTo = replaceSkuMap.FirstOrDefault(x => x.ShortSKU == sku);
                // if(replaceTo!=null)
                //{
                //     sku = replaceTo.ProductSKU;
                //}
                //check if not repeated row
                if (processed.ContainsKey(sku))
                {
                    validRow = false;
                    processErrors.Add(new ProcessError()
                    {
                        LineNum = lineNum
                        ,
                        SKU = sku
                        ,
                        Message = string.Format(RP.S("Admin.Product.Import.Error.SKUAlreadyProcessed-LineNume{0}"), lineNum)
                    });
                    errors.AppendLine(string.Format(RP.S("Admin.Product.Import.Error.SKUAlreadyProcessed-LineNume{0}"), lineNum));

                    continue;
                }
                processed[sku] = true;
                if (sku.Length == 13 || sku.Length == 12)
                {
                    // 036000291454x

                    int codelength = 12;
                    if (sku.Length == 12)
                    {
                        // 03600029145x
                        codelength = 11;
                    }
                    int odd = 0;
                    int even = 0;
                    int check = 0;
                    int.TryParse(sku[codelength].ToString(), out check);

                    int tmp = 0;
                    for (int i = 0; i < codelength; i = i + 2)
                    {
                        int.TryParse(sku[i].ToString(), out tmp);
                        odd += tmp;
                    }
                    odd = odd * 3;
                    for (int i = 1; i < codelength; i = i + 2)
                    {
                        int.TryParse(sku[i].ToString(), out tmp);
                        even += tmp;
                    }
                    int sum = odd + even;
                    int module = sum % 10;
                    int mustBe = 10 - module;
                    if (mustBe != check)
                    {
                        //validRow = false;
                        processErrors.Add(new ProcessError()
                        {
                            LineNum = lineNum
                            ,
                            SKU = sku
                            ,
                            Message = string.Format(RP.S("Admin.Product.Import.Error.SKUnotValid-LineNume{0}"), lineNum)
                        });
                        errors.AppendLine(string.Format(RP.S("Admin.Product.Import.Error.SKUnotValid-LineNume{0}"), lineNum));

                    }
                }


                if (lastEmpty.Length > 0)
                {

                    errors.AppendLine(lastEmpty.ToString());
                    lastEmpty.Clear();
                }
                var priceStr = s[sheme.Price].ToString().Trim();
                if (priceStr == "")
                {
                    priceStr = "0";
                }


                var product = new Product()
                {
                    SKU = sku,
                    Name = s[sheme.Name].ToString().Trim(),
                    IsKosher = s[sheme.IsKosher].ToString().Trim() == "1"
                || s[sheme.IsKosher].ToString().Trim() == "true"
                || s[sheme.IsKosher].ToString().Trim() == "True"
                || !string.IsNullOrEmpty(s[sheme.KosherType].ToString().Trim()),
                    KosherType = s[sheme.KosherType].ToString().Trim(),

                    Capacity = s[sheme.Capacity].ToString().Trim(),
                    MeasureUnit = s[sheme.MeasureUnit].ToString().Trim() == "" ? null : s[sheme.MeasureUnit].ToString().Trim(),
                    Components = s[sheme.Components].ToString().Trim(),
                    // Manufacturer = s[sheme.Manufacturer].ToString().Trim(),
                    NoTax = s[sheme.Tax].ToString().Trim() == "1" || s[sheme.Tax].ToString().Trim() == "true" || s[sheme.Tax].ToString().Trim() == "True",
                    SoldByWeight = s[sheme.SoldByWeight].ToString().Trim() == "1" || s[sheme.SoldByWeight].ToString().Trim() == "true" || s[sheme.SoldByWeight].ToString().Trim() == "True",
                    ShortDescription = s[sheme.ShortDescription].ToString().Trim(),
                    FullDescription = s[sheme.FullDescription].ToString().Trim(),
                    ProductShopOptions = s[sheme.ProductOptions].ToString().Trim(),
                };
                //if (!string.IsNullOrEmpty(priceStr))
                //{
                //    decimal price = 0;
                //    bool valid = decimal.TryParse(priceStr.Replace(".", decimalSeparator).Replace(",", decimalSeparator)
                //         , out price);

                //    product.RecomendedPrice = price;
                //    if (!valid)
                //    {
                //        validRow = false;
                //        processErrors.Add(new ProcessError()
                //        {
                //            LineNum = lineNum
                //            ,
                //            SKU = sku,
                //            Message = string.Format(RP.S("Admin.Product.Import.Error.PriceNotValid-LineNume{0}"), lineNum)
                //        });
                //        errors.AppendLine(string.Format(RP.S("Admin.Product.Import.Error.PriceNotValid-LineNume{0}"), lineNum));
                //    }
                //}

                //step for quantity change
                if (!string.IsNullOrEmpty(s[sheme.UnitPerPackage]))
                {
                    int step = 0;
                    bool valid = int.TryParse(s[sheme.UnitPerPackage].ToString().Trim(), out step);
                    if (step > 0)
                        product.MeasureUnitStep = step;
                    product.UnitsPerPackage = step;

                    if (!valid)
                    {
                        validRow = false;
                        processErrors.Add(new ProcessError()
                        {
                            LineNum = lineNum
                            ,
                            SKU = sku,
                            Message = string.Format(RP.S("Admin.Product.Import.Error.UnitPerPackageNotValidNumber-LineNume{0}"), lineNum)
                        });
                        errors.AppendLine(string.Format(RP.S("Admin.Product.Import.Error.UnitPerPackageNotValidNumber-LineNume{0}"), lineNum));
                    }

                }
                var manuf = s[sheme.Manufacturer].ToString().Trim();
                if (!string.IsNullOrEmpty(manuf))
                {
                    var man = AllManufacturers.FirstOrDefault(x => x.Name != null && x.Name.ToLower() == manuf.ToLower());
                    if (man == null)
                    {
                        man = new Manufacturer()
                        {
                            DisplayOrder = 0,
                            Published = true,
                            Name = manuf
                        };
                        _db.Manufacturers.Add(man);
                        _db.SaveChanges();
                        AllManufacturers.Add(man);
                    }
                    if (man != null)
                    {
                        product.ProductManufacturerID = man.ID;
                        product.DisplayOrder = man.DisplayOrder;
                    }
                }
                //category
                //var parentcat = AllCategories.FirstOrDefault(x => x.Name == s[sheme.Category].ToString() && x.ParentCategoryID == 0);
                //if (parentcat == null)
                //{
                //    parentcat = new Category()
                //    {
                //        DisplayOrder = 500,
                //        Name = s[sheme.Category],
                //        ParentCategoryID=0,
                //        Published=true,
                //    };
                //    _db.Categories.Add(parentcat);
                //    _db.SaveChanges();
                //    AllCategories.Add(parentcat);
                //}
                if (validRow && !string.IsNullOrEmpty(s[sheme.SubCategory].ToString().Trim()))
                {
                    var cat = AllCategories.FirstOrDefault(x => x.Name == s[sheme.SubCategory].ToString().Trim());
                    if (cat == null
                        && needCreateCategoryIfNotExist)
                    {
                        cat = new Category()
                        {
                            DisplayOrder = 500,
                            Name = s[sheme.SubCategory].ToString().Trim(),
                            ParentCategoryID = 0,
                            Published = true,
                        };
                        _db.Categories.Add(cat);
                        _db.SaveChanges();
                        AllCategories.Add(cat);
                    }
                    if (cat != null)
                    {
                        product.CategoryID = cat.ID;
                    }
                }
                if (validRow)
                {
                    if (productsList.ContainsKey(sku))
                    {
                        if (!productsList[sku].IgnoreOnImport)
                        {
                            var pID = productsList[sku].ID;
                            product.ID = pID;
                            //update
                            if (isInsertOrUpdate)
                                productForUpdate.Add(product);

                            //delete
                            if (isDelete)
                                productForDelete.Add(product);
                        }
                    }
                    else
                    {
                        //insert
                        if (isInsert || isInsertOrUpdate)
                        {
                            if (!productforInsert.Any(x => x.SKU == product.SKU))
                                productforInsert.Add(product);
                        }
                    }
                }
            }
            string message = RP.S("Admin.Product.Import.SeeError");
            if (true)// || errors.Length < 2) //no errors
            {
                var insertRes = productforInsert.SqlInsert(returnLog: true);
                var updateRes = productForUpdate.SqlUpdateById(false, returnLog: true);
                var deleteRes = productForDelete.SqlDeleteById(true);
                message = RP.S("Admin.Product.Import.Success");
                //add errors to table
                try
                {
                    foreach (var procError in processErrors)
                    {
                        procError.CreateOn = DateTime.Now;
                        procError.FileServiceName = FileName;
                        procError.IP = LS.GetUser_IP(Request);
                        procError.PageUrl = Request.RawUrl;
                        procError.RefererUrl = Request.UrlReferrer != null ? Request.UrlReferrer.OriginalString : null;
                        procError.ShopID = 0;
                        procError.UserID = LS.CurrentUser.ID;
                    }
                    processErrors.SqlInsert();
                }
                catch
                {

                }

                // ActivityLogFiles
                if (false)
                {
                    var filename = Guid.NewGuid().ToString().ToLower()
                        .Replace("-", "")
                        + Path.GetExtension(FilePath);
                    var filePath = Server.MapPath("~/Content/ActivityLogFiles/") + filename;
                    System.IO.File.Copy(FilePath, filePath, true);
                    ActivityLog activity = new ActivityLog()
                    {
                        ActivityType = ActivityType.Bulk,
                        CreateOn = DateTime.Now,
                        DirectSQL = insertRes.SqlLog.ToString() + @"
                    
--UPDATE SQL
" + updateRes.SqlLog.ToString()

      + @"
                    
--DELETE SQL
" + deleteRes.SqlLog.ToString()
      ,
                        EntityType = EntityType.Product,
                        FullText = "",
                        RequestUrl = Request.RawUrl,
                        ShortDescription = "",
                        UploadedFileName = attachment.FileName,
                        CopiedFileName = filename,
                        UserID = LS.CurrentUser.ID
                    };
                }
            }
            return Json(new { success = "ok", message = message, errors = errors.ToString() });

        }

        [HttpPost]
        public ActionResult ProductSpecificationImport(HttpPostedFileBase attachment)
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
            var Columns = new List<string>();
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
                        for (var c = begin.Column; c <= end.Column; c++)
                        {
                            var val = currentWorksheet.Cells[begin.Row, c].Value;
                            Columns.Add(val != null ? val.ToString() : "");

                        }
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


            var productsList = _db.Products
                   .OrderBy(x => x.ID)
                   .Select(x => new { x.ID, x.SKU, x.IgnoreOnImport }) // optimization
                   .ToList().ToDictionary(x => x.SKU);
            var decimalSeparator = CultureInfo.CurrentCulture.NumberFormat.NumberDecimalSeparator;
            var headers = Columns;
            var curSpecAttr = LS.Get<SpecificationAttribute>();
            var specAttrToInsert = new List<SpecificationAttribute>();
            for (var i = 1; i < headers.Count; i++)
            {
                var name = headers[i].ToString().Trim();
                if (!curSpecAttr.Any(x => x.Name == name))
                {
                    SpecificationAttribute attr = new SpecificationAttribute()
                    {
                        SpecificationAttributeTypeID = 2,
                        Name = headers[i]
                    };
                    specAttrToInsert.Add(attr);
                }
            }
            specAttrToInsert.SqlInsert();//insert
            specAttrToInsert.Clear();
            curSpecAttr = LS.CleanGet<SpecificationAttribute>();//refresh list

            var curSpecAttrOption = LS.Get<SpecificationAttributeOption>();
            var specAttrOptiontoInsert = new List<SpecificationAttributeOption>();
            Dictionary<int, List<SpecificationAttributeOption>> mapoptions = new Dictionary<int, List<SpecificationAttributeOption>>();

            int porcion = 50;
            int counter = 0;
            foreach (var s in all)
            {
                if (s[0] == null || s[0].ToString().Trim() == "") { continue; }
                var sku = s[0].ToString().Trim();
                if (!productsList.ContainsKey(sku))
                {
                    continue; // no product
                }

                var product = productsList[sku];
                if (product.IgnoreOnImport)
                {
                    continue;
                }
                counter++; //next counter
                for (var i = 1; i < headers.Count; i++)
                {
                    var attrname = headers[i].ToString().Trim();
                    var optionname = s[i].ToString().Trim();
                    var attr = curSpecAttr.FirstOrDefault(x => x.Name == attrname);
                    if (!string.IsNullOrEmpty(optionname) && attr != null)
                    {
                        //Attr option
                        var attrOption = curSpecAttrOption.FirstOrDefault(x => x.Name == optionname && x.SpecificationAttributeID == attr.ID);
                        if (attrOption == null)
                        {
                            attrOption = new SpecificationAttributeOption()
                            {
                                Name = optionname,

                            };


                            attrOption.SpecificationAttributeID = attr.ID;

                            specAttrOptiontoInsert.Add(attrOption);
                        }


                        //product attr option map
                        if (!mapoptions.ContainsKey(product.ID))
                        {
                            mapoptions[product.ID] = new List<SpecificationAttributeOption>();
                        }
                        mapoptions[product.ID].Add(attrOption);
                    }
                }
                //check porcion and process map
                if (counter > porcion)
                {

                    //run map insert

                    ProcessInsertSpecPorcion(specAttrOptiontoInsert, mapoptions);
                    curSpecAttrOption = LS.Get<SpecificationAttributeOption>();
                    counter = 0;
                    //end map insert
                }
            }
            //run last porcion
            ProcessInsertSpecPorcion(specAttrOptiontoInsert, mapoptions);


            return Json(new { success = "ok", message = "Import Success" });
        }
        private void ProcessInsertSpecPorcion(List<SpecificationAttributeOption> specAttrOptiontoInsert
            , Dictionary<int, List<SpecificationAttributeOption>> mapoptions)
        {
            specAttrOptiontoInsert.SqlInsert();
            specAttrOptiontoInsert.Clear();
            var curSpecAttrOption = LS.CleanGet<SpecificationAttributeOption>();
            var listProdIDs = mapoptions.Select(x => x.Key);
            var curProductOptionMap = _db.ProductSpecificationAttributeOptions
                .Where(x => listProdIDs.Contains(x.ProductID)).ToList();
            var mapOptionToInsert = new List<ProductSpecificationAttributeOption>();
            foreach (var psa in mapoptions)
            {
                foreach (var sao in psa.Value)
                {
                    var attrOption = sao;
                    if (sao.ID == 0)
                    {
                        attrOption = curSpecAttrOption.FirstOrDefault(x => x.Name == sao.Name
                          && x.SpecificationAttributeID == sao.SpecificationAttributeID);
                    }
                    if (attrOption != null)
                    {
                        if (!curProductOptionMap.Any(x => x.ProductID == psa.Key && x.SpecificationAttributeOptionID == attrOption.ID))
                        {
                            var prodSpecOption = new ProductSpecificationAttributeOption()
                            {
                                ProductID = psa.Key,
                                SpecificationAttributeOptionID = attrOption.ID
                            };
                            mapOptionToInsert.Add(prodSpecOption);
                        }

                    }
                }

            }
            mapOptionToInsert.SqlInsert();

        }
        #endregion


        //settings for csv
        private string csvDlm = ",";
        private string csvQuote = "\"";

        private string WriteCsvLine(string line)
        {
            if (line != null && line.Length > 0 && line.EndsWith(csvDlm))
            {
                return line.Substring(0, line.LastIndexOf(csvDlm));
            }
            return line;
        }
        private string WriteCsvCell(string text)
        {
            if (text == null) { text = ""; }
            string res = text;
            if (text.IndexOf(csvDlm) > -1)
            {
                res = csvQuote + res.Replace(csvQuote, csvQuote + csvQuote) + csvQuote;
            }
            return res + csvDlm;
        }

        [ValidateInput(false)]
        [FormValueRequired("export")]
        [HttpPost, ActionName("Export")]
        public ActionResult Export(ProductFilter model)
        {
            bool? withCategory = model.WithCategory;
            bool? withPictures = model.WithPictures;
            bool? activeShop = model.ActiveShop;
            byte[] bytes = null;
            //string LangCode = "en-US";
            using (var stream = new MemoryStream())
            {
                //  _exportManager.ExportProductsToXlsx(stream, products);
                StreamWriter sw = new StreamWriter(stream, Encoding.UTF8);
                // IQueryable list = null;
                var source = _db.Products.AsNoTracking().Where(x => x.Deleted == false).AsQueryable();
                if (activeShop.HasValue)
                {
                    if (activeShop.Value)
                    {
                        source = from p in source
                                 join ps in _db.ProductShopMap
                                 on p.ID equals ps.ProductID
                                 join s in _db.Shops
                                 on ps.ShopID equals s.ID
                                 where s.Active == true
                                 select p;
                    }
                    else
                    {
                        source = from p in source
                                 join ps in _db.ProductShopMap
                                 on p.ID equals ps.ProductID
                                 join s in _db.Shops
                                 on ps.ShopID equals s.ID
                                 where s.Active == false
                                 select p;
                    }
                }
                if (model.ShopName > 0)
                {
                    var shopID = model.ShopName;
                    source = from p in source
                             join ps in _db.ProductShopMap
                             on p.ID equals ps.ProductID
                             where ps.ShopID == shopID
                             select p;

                    if (activeShop.HasValue)
                    {
                        if (activeShop.Value)
                        {
                            source = from p in source
                                     join ps in _db.ProductShopMap
                                     on p.ID equals ps.ProductID
                                     join s in _db.Shops
                                     on ps.ShopID equals s.ID
                                     where ps.ShopID == shopID
                                     && s.Active == true
                                     select p;
                        }
                        else
                        {
                            source = from p in source
                                     join ps in _db.ProductShopMap
                                     on p.ID equals ps.ProductID
                                     join s in _db.Shops
                                     on ps.ShopID equals s.ID
                                     where ps.ShopID == shopID
                                     && s.Active == false
                                     select p;
                        }
                    }

                }
                if (withCategory.HasValue)
                {
                    if (withCategory.Value)
                    {
                        source = source.Where(x => x.CategoryID != 0);
                    }
                    else
                    {
                        source = source.Where(x => x.CategoryID == 0);
                    }
                }
                //if (withPictures.HasValue)
                //{
                //    if (withPictures.Value)
                //    {
                //        source = source.Where(x => x.Image != null);
                //    }
                //    else
                //    {
                //        source = source.Where(x => x.Image == null);
                //    }
                //}

                var list = source.AsEnumerable();

                if (withPictures.HasValue)
                {
                    if (withPictures.Value)
                    {
                        list = list.Where(x => System.IO.File.Exists(HttpContext.Server.MapPath("~" + x.Image)));
                    }
                    else
                    {
                        list = list.Where(x => System.IO.File.Exists(HttpContext.Server.MapPath("~" + x.Image)) == false);                        
                    }
                }

                list = list.ToList();

                var cats = LS.Get<Category>();
                var manufacturers = LS.Get<Manufacturer>();
                var neededData = list.Select
                    (
                    x => new
                    {
                        x.SKU,
                        x.Name,
                        Category = (cats.FirstOrDefault(y => y.ID == x.CategoryID) != null
                       ? cats.FirstOrDefault(y => y.ID == x.CategoryID).Name : ""),

                        SoldByWeight = x.SoldByWeight ? "1" : "0",
                        x.Capacity,
                        x.MeasureUnit,
                        x.UnitsPerPackage,
                        Manufacturer = (manufacturers.FirstOrDefault(y => y.ID == x.ProductManufacturerID) != null
                      ? manufacturers.FirstOrDefault(y => y.ID == x.ProductManufacturerID).Name : ""),
                        NoTax = x.NoTax ? "1" : "0",
                        x.MadeCoutry,
                        IsKosher = (x.IsKosher ? "1" : "0"),
                        x.KosherType,


                        x.Components,



                        //x.Manufacturer,

                        // x.RecomendedPrice,


                        x.ShortDescription,
                        x.FullDescription,                       
                        x.ProductShopOptions,
                        Flag = 1,
                        x.Image
                    }
                    ).ToList();


                bytes = CreateExcelFile.CreateExcelDocument(
                   neededData, "Products_" + DateTime.Now.ToString() + ".xlsx"
                   , HttpContext.Response);

                return File(bytes
                    , "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet"
                    , "Products_" + DateTime.Now.ToString() + ".xlsx");

            }

        }

        //public ActionResult CSVExport()
        //{
        //    var items = _db.AbstractPages.Where(r => r.Visible == true).OrderBy(r => r.Title).ToList();
        //    MemoryStream output = new MemoryStream();
        //    StreamWriter writer = new StreamWriter(output, Encoding.UTF8);
        //    writer.Write("Comunt1,");
        //    writer.Write("Comunt2,");
        //    writer.Write("Comunt3");
        //    writer.WriteLine();
        //    foreach (Job item in items)
        //    {
        //        writer.Write(item.ID); writer.Write(",\"");
        //        writer.Write(item.Title); writer.Write("\",\"");
        //        writer.Write(item.ShortDescription); writer.Write("\""); writer.WriteLine();
        //    }
        //    writer.Flush();
        //    output.Position = 0;
        //    Encoding heb = Encoding.GetEncoding("windows-1255");
        //    return File(heb.GetBytes(new StreamReader(output).ReadToEnd()), "text/csv", "AbstractPages.csv");
        //}



    }
}

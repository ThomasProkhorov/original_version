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

namespace Uco.Areas.Admin.Controllers
{
    [AuthorizeAdmin]
    public class TextComponentController : BaseAdminController
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
            IQueryable<TextComponent> items = _db.TextComponents.Where(r => r.DomainID == CurrentSettings.ID);
            DataSourceResult result = items.ToDataSourceResult(request);
            return Json(result);
        }

        [AcceptVerbs(HttpVerbs.Post)]
        public ActionResult _AjaxDelete(TextComponent item, [DataSourceRequest]DataSourceRequest request)
        {
            if (ModelState.IsValid)
            {
                _db.TextComponents.Remove(_db.TextComponents.First(r => r.ID == item.ID && r.DomainID == CurrentSettings.ID));
                _db.SaveChanges();
                CleanCache.CleanOutputCache();
            }
            return Json(new[] { item }.ToDataSourceResult(request, ModelState));
        }

        public ActionResult Create()
        {
            return View(new TextComponent());
        }

        [HttpPost]
        public ActionResult Create(TextComponent item)
        {
            if (ModelState.IsValid)
            {
                item.DomainID = CurrentSettings.ID;
                _db.TextComponents.Add(item);
                _db.SaveChanges();

                CleanCache.CleanOutputCache();

                return RedirectToAction("Index");
            }
            return View(item);
        }

        public ActionResult Edit(int ID)
        {
            return View(_db.TextComponents.First(r => r.ID == ID));
        }

        [HttpPost]
        public ActionResult Edit(int ID, TextComponent item)
        {
            if (ModelState.IsValid)
            {
                item.DomainID = CurrentSettings.ID;
                _db.Entry(item).State = EntityState.Modified;
                _db.SaveChanges();

                CleanCache.CleanOutputCache();

                return RedirectToAction("Index");
            }
            return View(item);
        }
        #region ImportExport

        //settings for csv
        private string csvDlm = ",";
        private string csvQuote = "\"";

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
                var list = _db.TextComponents.Where(x => x.DomainID == CurrentSettings.ID).AsNoTracking().ToList();
                foreach (var t in list)
                {
                    sw.Write(csvQuote + t.SystemName + csvQuote + csvDlm);
                    sw.Write(csvQuote + t.LangCode + csvQuote + csvDlm);
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
            return File(bytes, "text/csv", "TextComponents_" + CurrentSettings.ID.ToString() + ".csv");
        }
        [ValidateInput(false)]
        [FormValueRequired("import")]
        [HttpPost, ActionName("Export")]
        public ActionResult Import(HttpPostedFileBase attachment)
        {
            if (attachment == null)
            {
                // TempData["MessageRed"] = "File Error";
                return Json(new { success = "error", message = "File Error" });
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
                //  TempData["MessageRed"] = "File must have extension : csv";
                // return RedirectToAction("Index");
            }
            // var excel = new ExcelQueryFactory(FilePath);
            var content = System.IO.File.ReadAllText(FilePath);
            string newLine = Environment.NewLine;
            string[] lines = content.Split(new string[] { newLine }, StringSplitOptions.RemoveEmptyEntries);
            var list = _db.TextComponents.Where(x => x.DomainID == CurrentSettings.ID).ToList();

            foreach (var row in lines)
            {
                if (!string.IsNullOrEmpty(row))
                {
                    var rowObj = row.TrimStart(csvQuote.ToCharArray()).TrimEnd((csvQuote + csvDlm).ToCharArray());
                    string[] values = rowObj.Split(new string[] { csvQuote + csvDlm + csvQuote }, StringSplitOptions.None);
                    if (values.Length > 2)
                    {
                        var key = values[0];
                        var lang = values[1];
                        var value = values[2]
                            .Replace("&sp;", "\n")
                       .Replace("&quot;", csvQuote);
                        var upd = list.FirstOrDefault(x => x.SystemName == key && x.LangCode == lang);
                        if (upd != null)
                        {
                            //update
                            upd.Text = value;
                            // _db.SaveChanges();
                        }
                        else
                        {
                            //insert new
                            var resString = new TextComponent()
                            {
                                SystemName = key,
                                Text = value
                            };
                            resString.LangCode = lang;
                            _db.TextComponents.Add(resString);

                        }
                    }
                }
            }
            _db.SaveChanges();
            return Json(new { success = "ok", message = "Import Success" });
            TempData["MessageGreen"] = "Import Success";
            return Content("");
        }

        #endregion
    }
}

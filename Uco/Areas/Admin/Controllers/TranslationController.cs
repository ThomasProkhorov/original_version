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
    public class TranslationController : BaseAdminController
    {
        public ActionResult Index(string lang, bool untranslated = false)
        {
            ViewBag.MessageRed = TempData["MessageRed"];
            ViewBag.MessageYellow = TempData["MessageYellow"];
            ViewBag.MessageGreen = TempData["MessageGreen"];

            if (string.IsNullOrEmpty(lang)) ViewBag.Lang = SF.GetLangCodeThreading();
            else ViewBag.Lang = lang;
            ViewBag.Untranslated = untranslated;
            return View();
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
                    sw.Write(csvQuote + (t.SystemName != null ? t.SystemName : "") + csvQuote + csvDlm);
                    sw.Write(csvQuote + (t.Text != null ? t.Text : "")
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
            return File(bytes, "text/csv", "LocalizedStrings_" + Languages + ".csv");
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
        public ActionResult _AjaxIndex([DataSourceRequest]DataSourceRequest request, string Lang, bool untranslated = false)
        {
            IQueryable<Translation> items = _db.Translations.Where(r => r.LangCode == Lang);
            if (untranslated)
            {
                items = items.Where(x => x.SystemName == x.Text);
            }
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

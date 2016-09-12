using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Uco.Models;
using Kendo.Mvc.UI;
using Kendo.Mvc.Extensions;
using System.Text;
using System.Reflection;
using System.IO;
using Uco.Infrastructure;

namespace Uco.Areas.Admin.Controllers
{
    [AuthorizeAdmin]
    public class ErrorsController : BaseAdminController
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
            IQueryable<Error> items = _db.Errors.Where(r => r.DomainID == AdminCurrentSettingsRepository.ID);
            DataSourceResult result = items.ToDataSourceResult(request);
            return Json(result);
        }

        [AcceptVerbs(HttpVerbs.Post)]
        public ActionResult _AjaxDelete(Error item, [DataSourceRequest]DataSourceRequest request)
        {
            if (ModelState.IsValid)
            {
                _db.Errors.Remove(_db.Errors.First(r => r.DomainID == AdminCurrentSettingsRepository.ID && r.ID == item.ID));
                _db.SaveChanges();
            }
            return Json(new[] { item }.ToDataSourceResult(request, ModelState));
        }

        public ActionResult DeleteAll()
        {
            foreach (var item in _db.Errors.Where(r => r.DomainID == AdminCurrentSettingsRepository.ID))
            {
                _db.Errors.Remove(item);
            }
            _db.SaveChanges();

            return RedirectToAction("Index");
        }

        public ActionResult CSVExport()
        {
            var items = _db.Errors.Where(r => r.DomainID == AdminCurrentSettingsRepository.ID).ToList();
            MemoryStream output = new MemoryStream();
            StreamWriter writer = new StreamWriter(output, Encoding.UTF8);
            writer.Write("ID,");
            writer.Write("Date,");
            writer.Write("Message,");
            writer.Write("Host,");
            writer.Write("UserName,");
            writer.Write("PhysicalPath,");
            writer.Write("UserAgent,");
            writer.Write("UserHostAddress,");
            writer.Write("Url,");
            writer.Write("UrlReferrer,");
            writer.Write("InnerException,");
            writer.Write("Source,");
            writer.Write("StackTrace,");
            writer.Write("TargetSite");
            writer.WriteLine();
            foreach (Error item in items)
            {
                writer.Write(item.ID); writer.Write(","); writer.Write("\"");
                writer.Write(item.Date); writer.Write("\""); writer.Write(","); writer.Write("\"");
                writer.Write(item.Message); writer.Write("\""); writer.Write(","); writer.Write("\"");
                writer.Write(item.Host); writer.Write("\""); writer.Write(","); writer.Write("\"");
                writer.Write(item.UserName); writer.Write("\""); writer.Write(","); writer.Write("\"");
                writer.Write(item.PhysicalPath); writer.Write("\""); writer.Write(","); writer.Write("\"");
                writer.Write(item.UserAgent); writer.Write("\""); writer.Write(","); writer.Write("\"");
                writer.Write(item.UserHostAddress); writer.Write("\""); writer.Write(","); writer.Write("\"");
                writer.Write(item.Url); writer.Write("\""); writer.Write(","); writer.Write("\"");
                writer.Write(item.UrlReferrer); writer.Write("\""); writer.Write(","); writer.Write("\"");
                writer.Write(item.InnerException); writer.Write("\""); writer.Write(","); writer.Write("\"");
                writer.Write(item.Source); writer.Write("\""); writer.Write(","); writer.Write("\"");
                writer.Write(item.StackTrace); writer.Write("\""); writer.Write(","); writer.Write("\"");
                writer.Write(item.TargetSite); writer.Write("\""); writer.WriteLine();
            }
            writer.Flush();
            output.Position = 0;
            return File(output, "text/comma-separated-values", "Errors.csv");
        }

        public ActionResult Details(int ID)
        {
            return View(_db.Errors.First(r => r.DomainID == AdminCurrentSettingsRepository.ID && r.ID == ID));
        }
    }
}

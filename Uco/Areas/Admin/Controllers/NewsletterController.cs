using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Uco.Models;
using Kendo.Mvc.UI;
using Kendo.Mvc.Extensions;
using System.Data;
using System.IO;
using System.Text;
using Uco.Infrastructure;

namespace Uco.Areas.Admin.Controllers
{
    [AuthorizeAdmin]
    public class NewsletterController : BaseAdminController
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
            IQueryable<Newsletter> items = _db.Newsletters.Where(r => (CurrentUser.RoleDefault == "Admin" ? true : (r.RoleDefault == CurrentUser.RoleDefault)));
            DataSourceResult result = items.ToDataSourceResult(request);
            return Json(result);
        }

        [AcceptVerbs(HttpVerbs.Post)]
        public ActionResult _AjaxDelete(Newsletter item, [DataSourceRequest]DataSourceRequest request)
        {
            if (ModelState.IsValid)
            {
                _db.Newsletters.Remove(_db.Newsletters.First(r => (CurrentUser.RoleDefault == "Admin" ? true : (r.RoleDefault == CurrentUser.RoleDefault)) && r.ID == item.ID));
                _db.SaveChanges();
                CleanCache.CleanOutputCache();
            }
            return Json(new[] { item }.ToDataSourceResult(request, ModelState));
        }

        public ActionResult Details(int ID)
        {
            return View(_db.Newsletters.First(r => (CurrentUser.RoleDefault == "Admin" ? true : (r.RoleDefault == CurrentUser.RoleDefault)) && r.ID == ID));
        }

        public ActionResult CSVExport()
        {
            var items = _db.Newsletters.Where(r => (CurrentUser.RoleDefault == "Admin" ? true : (r.RoleDefault == CurrentUser.RoleDefault))).ToList();
            MemoryStream output = new MemoryStream();
            StreamWriter writer = new StreamWriter(output, Encoding.UTF8);
            writer.Write("ID,");
            writer.Write("Date,");
            writer.Write("Name,");
            writer.Write("Email,");
            writer.Write("Role,");
            writer.Write("Data");
            writer.WriteLine();
            foreach (Newsletter item in items)
            {
                writer.Write(item.ID); writer.Write(","); writer.Write("\"");
                writer.Write(item.NewsletterDate); writer.Write("\""); writer.Write(","); writer.Write("\"");
                writer.Write(item.NewsletterName); writer.Write("\""); writer.Write(","); writer.Write("\"");
                writer.Write(item.NewsletterEmail); writer.Write("\""); writer.Write(","); writer.Write("\"");
                writer.Write(item.RoleDefault); writer.Write("\""); writer.Write(","); writer.Write("\"");
                writer.Write(item.NewsletterData); writer.Write("\""); writer.WriteLine();
            }
            writer.Flush();
            output.Position = 0;
            return File(output, "application/csv", "Newsletters.csv");
        }
    }
}

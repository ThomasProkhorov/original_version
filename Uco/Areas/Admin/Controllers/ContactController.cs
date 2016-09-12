using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Kendo.Mvc.UI;
using Kendo.Mvc.Extensions;
using Uco.Models;
using System.Data;
using System.IO;
using System.Text;
using Uco.Infrastructure.Livecycle;
using Uco.Infrastructure;

namespace Uco.Areas.Admin.Controllers
{
    [AuthorizeAdmin]
    public class ContactController : BaseAdminController
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
            IQueryable<Contact> items = _db.Contacts.Where(r => (CurrentUser.RoleDefault == "Admin" ? true : (r.RoleDefault == CurrentUser.RoleDefault)));
            DataSourceResult result = items.ToDataSourceResult(request);
            return Json(result);
        }

        [AcceptVerbs(HttpVerbs.Post)]
        public ActionResult _AjaxDelete(Contact item, [DataSourceRequest]DataSourceRequest request)
        {
            if (ModelState.IsValid)
            {
                _db.Contacts.Remove(_db.Contacts.First(r => (CurrentUser.RoleDefault == "Admin" ? true : (r.RoleDefault == CurrentUser.RoleDefault)) && r.ID == item.ID));
                _db.SaveChanges();
            }
            return Json(new[] { item }.ToDataSourceResult(request, ModelState));
        }

        public ActionResult Details(int ID)
        {
            return View(_db.Contacts.First(r => (CurrentUser.RoleDefault == "Admin" ? true : (r.RoleDefault == CurrentUser.RoleDefault)) && r.ID == ID));
        }

        public ActionResult CSVExport()
        {
            IQueryable<Contact> items = _db.Contacts.Where(r => (CurrentUser.RoleDefault == "Admin" ? true : (r.RoleDefault == CurrentUser.RoleDefault))).OrderByDescending(r => r.ContactDate);

            MemoryStream output = new MemoryStream();
            StreamWriter writer = new StreamWriter(output, Encoding.UTF8);
            writer.Write("ID,");
            writer.Write("Date,");
            writer.Write("Name,");
            writer.Write("Email,");
            writer.Write("Phone,");
            writer.Write("Data,");
            writer.Write("Role,");
            writer.Write("Rool,");
            writer.Write("Url,");
            writer.Write("Referal");
            writer.WriteLine();
            foreach (Contact item in items)
            {
                writer.Write(item.ID); writer.Write(","); writer.Write("\"");
                writer.Write(item.ContactDate); writer.Write("\""); writer.Write(","); writer.Write("\"");
                writer.Write(UcoString.ClearForCSV(item.ContactName)); writer.Write("\""); writer.Write(","); writer.Write("\"");
                writer.Write(UcoString.ClearForCSV(item.ContactEmail)); writer.Write("\""); writer.Write(","); writer.Write("\"");
                writer.Write(UcoString.ClearForCSV(item.ContactPhone)); writer.Write("\""); writer.Write(","); writer.Write("\"");
                writer.Write(UcoString.ReplaceBrByDot(UcoString.ClearForCSV(item.ContactData))); writer.Write("\""); writer.Write(","); writer.Write("\"");
                writer.Write(UcoString.ClearForCSV(item.RoleDefault)); writer.Write("\""); writer.Write(","); writer.Write("\"");
                writer.Write(UcoString.ClearForCSV(item.Rool)); writer.Write("\""); writer.Write(","); writer.Write("\"");
                writer.Write(UcoString.RemoveHTMLTagsFromString(UcoString.ClearForCSV(item.ContactUrl))); writer.Write("\""); writer.Write(","); writer.Write("\"");
                writer.Write(UcoString.ClearForCSV(item.ContactReferal)); writer.Write("\""); writer.WriteLine();
            }
            writer.Flush();
            output.Position = 0;
            return File(output, "application/csv", "Contact.csv");
        }
    }
}

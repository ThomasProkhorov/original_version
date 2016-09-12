using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Kendo.Mvc.UI;
using Kendo.Mvc.Extensions;
using Uco.Models;
using System.Data;
using System.Web.Security;
using Uco.Infrastructure;
using System.Data.Entity;
using Uco.Infrastructure.Providers;
using Uco.Infrastructure.Livecycle;

namespace Uco.Areas.Admin.Controllers
{
    [AuthorizeAdmin]
    public class RoleController : BaseAdminController
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
            IQueryable<Role> items = _db.Roles;
            DataSourceResult result = items.ToDataSourceResult(request);
            return Json(result);
        }

        [AcceptVerbs(HttpVerbs.Post)]
        public ActionResult _AjaxDelete(Role item, [DataSourceRequest]DataSourceRequest request)
        {
            if (ModelState.IsValid)
            {
                SURoleProvider rp = new SURoleProvider();
                if (!rp.DeleteRole(item.Title, true))
                {
                    ModelState.AddModelError("", "Not deleted");
                }
            }
            return Json(new[] { item }.ToDataSourceResult(request, ModelState));
        }

        public ActionResult Create()
        {
            return View(new Uco.Models.Role());
        }

        [HttpPost]
        public ActionResult Create(Role item)
        {
            if (ModelState.IsValid)
            {
                SURoleProvider rp = new SURoleProvider();
                rp.CreateRole(item.Title);

                try
                {
                    System.IO.Directory.CreateDirectory(Server.MapPath(string.Format("~/Content/UserFiles/{0}", item.Title)));
                    System.IO.Directory.CreateDirectory(Server.MapPath(string.Format("~/Content/UserFiles/{0}/Upload", item.Title)));
                    System.IO.Directory.CreateDirectory(Server.MapPath(string.Format("~/Content/UserFiles/{0}/Gallery", item.Title)));
                }
                catch
                {
                    TempData["MessageYellow"] = "Done but some group folders not created.";
                    return RedirectToAction("Index");
                }

                TempData["MessageGreen"] = "Done.";
                return RedirectToAction("Index");
            }
            return View(item);
        }

        public ActionResult Edit(int ID)
        {
            Role item = _db.Roles.First(r => r.ID == ID);
            if (item.IsSystem)
            {
                TempData["MessageRed"] = "Can't edit System role";
                return RedirectToAction("Index");
            }
            return View(item);
        }

        [HttpPost]
        public ActionResult Edit(Role item, List<string> MenuPermissions)
        {
            if (ModelState.IsValid)
            {
                item.MenuPermissions = SF.RolesListToString(MenuPermissions);
                _db.Entry(item).State = EntityState.Modified;
                _db.SaveChanges();

                CleanCache.CleanOutputCache();

                TempData["MessageGreen"] = "Done.";
                return RedirectToAction("Index");
            }
            item.MenuPermissions = SF.RolesListToString(MenuPermissions);
            return View(item);
        }
    }
}

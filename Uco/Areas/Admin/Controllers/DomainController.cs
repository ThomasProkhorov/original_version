using Kendo.Mvc.Extensions;
using Kendo.Mvc.UI;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web.Mvc;
using Uco.Infrastructure;
using Uco.Models;

namespace Uco.Areas.Admin.Controllers
{
    [AuthorizeAdmin]
    public class DomainController : BaseAdminController
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
            IQueryable<Settings> items = _db.SettingsAll;
            DataSourceResult result = items.ToDataSourceResult(request);
            return Json(result);
        }

        [AcceptVerbs(HttpVerbs.Post)]
        public ActionResult _AjaxDelete(Settings item, [DataSourceRequest]DataSourceRequest request)
        {
            if (ModelState.IsValid)
            {
                if (_db.SettingsAll.Count() == 1)
                {
                    ModelState.AddModelError("", "Not deleted");
                    return Json(new[] { item }.ToDataSourceResult(request, ModelState));
                }
                _db.SettingsAll.Remove(_db.SettingsAll.First(r => r.ID == item.ID));

                //delete pages and elements
                foreach (AbstractPage item2 in _db.AbstractPages.Where(r => r.DomainID == item.ID).ToList())
                {
                    _db.Entry(item2).State = EntityState.Modified;
                }
                foreach (TextComponent item2 in _db.TextComponents.Where(r => r.DomainID == item.ID).ToList())
                {
                    _db.Entry(item2).State = EntityState.Modified;
                }
                foreach (Contact item2 in _db.Contacts.Where(r => r.DomainID == item.ID).ToList())
                {
                    _db.Entry(item2).State = EntityState.Modified;
                }
                foreach (Newsletter item2 in _db.Newsletters.Where(r => r.DomainID == item.ID).ToList())
                {
                    _db.Entry(item2).State = EntityState.Modified;
                }

                _db.SaveChanges();

                //delete folders
                System.IO.Directory.Delete(Server.MapPath("~/Content/UserFiles/" + item.ID), true);
                System.IO.Directory.Delete(Server.MapPath("~/App_Data/cache/images/" + item.ID), true);

                CleanCache.CleanOutputCache();
                CleanCache.CleanSettingsAllCache();
            }
            return Json(new[] { item }.ToDataSourceResult(request, ModelState));
        }


        public ActionResult Create()
        {
            return View(new Settings());
        }

        [HttpPost]
        public ActionResult Create(Settings item, List<string> Roles)
        {
            if (ModelState.IsValid)
            {
                if (_db.SettingsAll.Count(r => r.Domain == item.Domain) > 0)
                {
                    ModelState.AddModelError("", "Domain + LanguageCode not unic. Please specify different Domain or LanguageCode");
                    return View(item);
                }

                string Title = "";
                if (string.IsNullOrEmpty(item.Domain)) Title = (_db.AbstractPages.Max(r => r.ID) + 1).ToString();
                else Title = item.Domain;

                DomainPage dp = new DomainPage();
                dp.Title = Title;
                dp.ParentID = 0;
                dp.DomainID = 0;
                dp.SeoUrlName = (_db.AbstractPages.Max(r => r.ID) + 1).ToString();
                dp.PermissionsView = SF.RolesListToString(new List<string>() { "Admin", "Anonymous" });
                dp.PermissionsEdit = SF.RolesListToString(new List<string>() { "Admin" });
                _db.DomainPages.Add(dp);
                _db.SaveChanges();

                List<string> DomainRoles = SF.GetRoleObjectsList().Where(r => r.IsSystem == true).Select(r => r.Title).ToList();
                if (Roles != null) DomainRoles.AddRange(Roles);
                item.Roles = SF.RolesListToString(DomainRoles);
                item.DomainPageID = dp.ID;
                if (string.IsNullOrEmpty(item.Domain)) item.Domain = Title;

                _db.SettingsAll.Add(item);
                _db.SaveChanges();

                dp.DomainID = item.ID;
                _db.Entry(dp).State = EntityState.Modified;
                _db.SaveChanges();

                System.IO.Directory.CreateDirectory(Server.MapPath("~/Content/UserFiles/" + item.ID));
                System.IO.Directory.CreateDirectory(Server.MapPath("~/App_Data/cache/images/" + item.ID));

                CleanCache.CleanOutputCache();
                CleanCache.CleanSettingsAllCache();

                return RedirectToAction("Index");
            }
            return View(item);
        }

        public ActionResult Edit(int ID)
        {
            return View(_db.SettingsAll.First(r => r.ID == ID));
        }

        [HttpPost]
        public ActionResult Edit(int ID, Settings item, List<string> Roles)
        {
            if (ModelState.IsValid)
            {
                List<string> DomainRoles = SF.GetRoleObjectsList().Where(r => r.IsSystem == true).Select(r => r.Title).ToList();
                if (Roles != null) DomainRoles.AddRange(Roles);
                item.Roles = SF.RolesListToString(DomainRoles);

                _db.Entry(item).State = EntityState.Modified;
                _db.SaveChanges();

                //update lang name of pages
                AbstractPage ap = _db.AbstractPages.FirstOrDefault(r => r.ID == item.DomainPageID);
                SF.SetLanguageCode(ap, 100, item.LanguageCode);

                CleanCache.CleanOutputCache();
                CleanCache.CleanSettingsCache();
                CleanCache.CleanMenuCache();

                return RedirectToAction("Index");
            }
            item.Roles = SF.RolesListToString(Roles);
            return View(item);
        }
    }
}

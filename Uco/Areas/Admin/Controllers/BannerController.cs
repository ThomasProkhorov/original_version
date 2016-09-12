using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Kendo.Mvc.UI;
using Kendo.Mvc.Extensions;
using Uco.Infrastructure;
using Uco.Models;
using System.Data;
using System.Data.Entity;

namespace Uco.Areas.Admin.Controllers
{
    [AuthorizeAdmin]
    public class BannerController : BaseAdminController
    {
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult _AjaxIndex([DataSourceRequest]DataSourceRequest request)
        {
            IQueryable<Banner> items = _db.Banners.Where(r => r.DomainID == AdminCurrentSettingsRepository.ID);
            DataSourceResult result = items.ToDataSourceResult(request);
            return Json(result);
        }

        [AcceptVerbs(HttpVerbs.Post)]
        public ActionResult _AjaxDelete(Banner item, [DataSourceRequest]DataSourceRequest request)
        {
            if (ModelState.IsValid)
            {
                _db.Banners.Remove(_db.Banners.First(r => r.ID == item.ID && r.DomainID == AdminCurrentSettingsRepository.ID));
                _db.SaveChanges();
                CleanCache.CleanOutputCache();
            }
            return Json(new[] { item }.ToDataSourceResult(request, ModelState));
        }


        //NOT AJAX GRID

        public ActionResult Create()
        {
            return View(new Banner() { ShowDateMax = DateTime.Now.AddYears(1) });
        }

        [HttpPost]
        public ActionResult Create(Banner item)
        {
            if (ModelState.IsValid)
            {
                _db.Banners.Add(item);
                _db.SaveChanges();

                CleanCache.CleanCacheAfterPageEdit();

                return RedirectToAction("Index");
            }
            return View(item);
        }

        public ActionResult Edit(int ID)
        {
            return View(_db.Banners.Find(ID));
        }

        [HttpPost]
        public ActionResult Edit(int ID, Banner item)
        {
            if (ModelState.IsValid)
            {
                item.DomainID = AdminCurrentSettingsRepository.ID;
                _db.Entry(item).State = EntityState.Modified;
                _db.SaveChanges();

                CleanCache.CleanOutputCache();

                return RedirectToAction("Index");
            }
            return View(item);
        }
    }
}

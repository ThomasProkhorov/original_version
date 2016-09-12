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
    public class ActivityLogController : BaseAdminController
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
            IQueryable<ActivityLog> items = _db.ActivityLogs;
            DataSourceResult result = items.ToDataSourceResult(request);
            return Json(result);
        }

        [AcceptVerbs(HttpVerbs.Post)]
        public ActionResult _AjaxDelete(ActivityLog item, [DataSourceRequest]DataSourceRequest request)
        {
            if (ModelState.IsValid)
            {
                _db.ActivityLogs.Remove(_db.ActivityLogs.First(r => r.ID == item.ID));
                _db.SaveChanges();
                CleanCache.CleanOutputCache();
            }
            return Json(new[] { item }.ToDataSourceResult(request, ModelState));
        }

        public ActionResult Create()
        {
            return View(new ActivityLog());
        }

        [HttpPost]
        public ActionResult Create(ActivityLog item)
        {
            if (ModelState.IsValid)
            {
                _db.ActivityLogs.Add(item);
                _db.SaveChanges();

                CleanCache.CleanOutputCache();

                return RedirectToAction("Index");
            }
            return View(item);
        }

        public ActionResult Edit(int ID)
        {
            return View(_db.ActivityLogs.First(r => r.ID == ID));
        }

        [HttpPost]
        public ActionResult Edit(int ID, ActivityLog item)
        {
            if (ModelState.IsValid)
            {
                _db.Entry(item).State = EntityState.Modified;
                _db.SaveChanges();

                CleanCache.CleanOutputCache();

                return RedirectToAction("Index");
            }
            return View(item);
        }

    }
}

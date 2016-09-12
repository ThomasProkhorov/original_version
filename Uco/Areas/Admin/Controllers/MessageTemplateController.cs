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
    public class MessageTemplateController : BaseAdminController
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
            IQueryable<MessageTemplate> items = _db.MessageTemplates.AsQueryable();
            DataSourceResult result = items.ToDataSourceResult(request);
            return Json(result);
        }

        [AcceptVerbs(HttpVerbs.Post)]
        public ActionResult _AjaxDelete(MessageTemplate item, [DataSourceRequest]DataSourceRequest request)
        {
            if (ModelState.IsValid)
            {
                _db.MessageTemplates.Remove(_db.MessageTemplates.First(r => r.ID == item.ID));
                _db.SaveChanges();
                CleanCache.CleanOutputCache();
            }
            return Json(new[] { item }.ToDataSourceResult(request, ModelState));
        }

        public ActionResult Create()
        {
            return View(new MessageTemplate());
        }

        [HttpPost]
        public ActionResult Create(MessageTemplate item)
        {
            if (ModelState.IsValid)
            {

                _db.MessageTemplates.Add(item);
                _db.SaveChanges();


                return RedirectToAction("Index");
            }
            return View(item);
        }

        public ActionResult Edit(int ID)
        {
            return View(_db.MessageTemplates.First(r => r.ID == ID));
        }

        [HttpPost]
        public ActionResult Edit(int ID, MessageTemplate item)
        {
            if (ModelState.IsValid)
            {
                _db.Entry(item).State = EntityState.Modified;
                _db.SaveChanges();


                return RedirectToAction("Index");
            }
            return View(item);
        }

    }
}

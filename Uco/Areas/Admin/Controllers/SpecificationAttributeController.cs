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
using Uco.Infrastructure.Livecycle;

namespace Uco.Areas.Admin.Controllers
{
    [AuthorizeAdmin]
    public class SpecificationAttributeController : BaseAdminController
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
            IQueryable<SpecificationAttribute> items = _db.SpecificationAttributes.AsQueryable();
            DataSourceResult result = items.ToDataSourceResult(request);
            var specTypes = LS.Get<SpecificationAttributeType>();
            foreach(var item  in (IEnumerable<SpecificationAttribute>)result.Data)
            {
                item.SpecificationAttributeType = specTypes
                    .FirstOrDefault(x => x.ID == item.SpecificationAttributeTypeID);
                if(item.SpecificationAttributeType==null)
                {
                    item.SpecificationAttributeType = new SpecificationAttributeType();
                }
            }
            return Json(result);
        }

        [AcceptVerbs(HttpVerbs.Post)]
        public ActionResult _AjaxDelete(SpecificationAttribute item, [DataSourceRequest]DataSourceRequest request)
        {
            if (ModelState.IsValid)
            {
                _db.SpecificationAttributes.Remove(_db.SpecificationAttributes.First(r => r.ID == item.ID ));
                _db.SaveChanges();
                CleanCache.CleanOutputCache();
            }
            return Json(new[] { item }.ToDataSourceResult(request, ModelState));
        }
        [AcceptVerbs(HttpVerbs.Post)]
        public ActionResult _AjaxInsert([DataSourceRequest] DataSourceRequest request, SpecificationAttribute item)
        {
            if (ModelState.IsValid)
            {
                _db.SpecificationAttributes.Add(item);
                _db.SaveChanges();

               }
            return Json(new[] { item }.ToDataSourceResult(request, ModelState));
        }

        [AcceptVerbs(HttpVerbs.Post)]
        public ActionResult _AjaxSave([DataSourceRequest] DataSourceRequest request, SpecificationAttribute item)
        {
            if (ModelState.IsValid)
            {
               _db.Entry(item).State = EntityState.Modified;
                _db.SaveChanges();

            }

            return Json(new[] { item }.ToDataSourceResult(request, ModelState));
        }
        public ActionResult Create()
        {
            return View(new SpecificationAttribute());
        }

        [HttpPost]
        public ActionResult Create(SpecificationAttribute item)
        {
            if (ModelState.IsValid)
            {
              
                _db.SpecificationAttributes.Add(item);
                _db.SaveChanges();

               
                return RedirectToAction("Index");
            }
            return View(item);
        }

        public ActionResult Edit(int ID)
        {
            return View(_db.SpecificationAttributes.First(r => r.ID == ID));
        }

        [HttpPost]
        public ActionResult Edit(int ID, SpecificationAttribute item)
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

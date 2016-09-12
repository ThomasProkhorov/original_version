using Kendo.Mvc.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Uco.Infrastructure;
using Uco.Infrastructure.Repositories;
using Uco.Models;
using Uco.Models.Shopping.Measure;
using Kendo.Mvc.Extensions;
using System.Data.Entity;

namespace Uco.Areas.Admin.Controllers
{
     [AuthorizeAdmin]
    public class ContentUnitMeasuresMapsController : BaseController
    {
        public ActionResult Index(string lang)
        {
            if (string.IsNullOrEmpty(lang)) ViewBag.Lang = SF.GetLangCodeThreading();
            else ViewBag.Lang = lang;

            ViewBag.ContentUnitMeasures = RP.GetContentUnitMeasures();
            return View();
        }

        public ActionResult _AjaxIndex([DataSourceRequest]DataSourceRequest request, string Lang)
        {
            IQueryable<ContentUnitMeasureMap> items = _db.ContentUnitMeasureMaps.AsNoTracking();
            DataSourceResult result = items.ToDataSourceResult(request);
            return Json(result);
        }

        [AcceptVerbs(HttpVerbs.Post)]
        public ActionResult _AjaxDelete(ContentUnitMeasureMap item, [DataSourceRequest]DataSourceRequest request, string Lang)
        {
            if (ModelState.IsValid)
            {
                _db.ContentUnitMeasureMaps.Remove(_db.ContentUnitMeasureMaps.First(r => r.ID == item.ID));
                _db.SaveChanges();
                RP.CleanContentUnitMeasureMapRepository();
               
            }
            return Json(new[] { item }.ToDataSourceResult(request, ModelState));
        }

        [AcceptVerbs(HttpVerbs.Post)]
        public ActionResult _AjaxInsert([DataSourceRequest] DataSourceRequest request, ContentUnitMeasureMap item)
        {
            if (ModelState.IsValid)
            {

                _db.ContentUnitMeasureMaps.Add(item);
                _db.SaveChanges();
                RP.CleanContentUnitMeasureMapRepository();
             
            }
            return Json(new[] { item }.ToDataSourceResult(request, ModelState));
        }

        [AcceptVerbs(HttpVerbs.Post)]
        public ActionResult _AjaxSave([DataSourceRequest] DataSourceRequest request, ContentUnitMeasureMap item)
        {
            if (ModelState.IsValid)
            {
                ContentUnitMeasureMap NewItem = _db.ContentUnitMeasureMaps.Find(item.ID);
                NewItem.ContentUnitMeasureID= item.ContentUnitMeasureID;
                NewItem.Multiplicator = item.Multiplicator;
                NewItem.Name = item.Name;
                NewItem.Synonymous = item.Synonymous;
                _db.Entry(NewItem).State = EntityState.Modified;
                _db.SaveChanges();
                RP.CleanContentUnitMeasureMapRepository();
               
            }

            return Json(new[] { item }.ToDataSourceResult(request, ModelState));
        }
    }
}
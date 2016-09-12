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
    public class ContentUnitMeasureController : BaseController
    {
        public ActionResult Index(string lang)
        {
            if (string.IsNullOrEmpty(lang)) ViewBag.Lang = SF.GetLangCodeThreading();
            else ViewBag.Lang = lang;

            return View();
        }

        public ActionResult _AjaxIndex([DataSourceRequest]DataSourceRequest request, string Lang)
        {
            IQueryable<ContentUnitMeasure> items = RP.GetContentUnitMeasures().AsQueryable();
            DataSourceResult result = items.ToDataSourceResult(request);
            return Json(result);
        }

        [AcceptVerbs(HttpVerbs.Post)]
        public ActionResult _AjaxDelete(ContentUnitMeasure item, [DataSourceRequest]DataSourceRequest request, string Lang)
        {
            if (ModelState.IsValid)
            {
                _db.ContentUnitMeasures.Remove(_db.ContentUnitMeasures.First(r => r.ID == item.ID));
                _db.SaveChanges();
                RP.CleanContentUnitMeasureRepository();
            }
            return Json(new[] { item }.ToDataSourceResult(request, ModelState));
        }

        [AcceptVerbs(HttpVerbs.Post)]
        public ActionResult _AjaxInsert([DataSourceRequest] DataSourceRequest request, ContentUnitMeasure item)
        {
            if (ModelState.IsValid)
            {

                _db.ContentUnitMeasures.Add(item);
                _db.SaveChanges();

                RP.CleanContentUnitMeasureRepository();
            }
            return Json(new[] { item }.ToDataSourceResult(request, ModelState));
        }

        [AcceptVerbs(HttpVerbs.Post)]
        public ActionResult _AjaxSave([DataSourceRequest] DataSourceRequest request, ContentUnitMeasure item)
        {
            if (ModelState.IsValid)
            {
                ContentUnitMeasure NewItem = _db.ContentUnitMeasures.Find(item.ID);
                NewItem.Name= item.Name;
                NewItem.DisplayName= item.DisplayName;
                _db.Entry(NewItem).State = EntityState.Modified;
                _db.SaveChanges();

                RP.CleanContentUnitMeasureRepository();
            }

            return Json(new[] { item }.ToDataSourceResult(request, ModelState));
        }

    }
}
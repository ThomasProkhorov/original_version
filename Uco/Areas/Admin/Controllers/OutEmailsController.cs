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

namespace Uco.Areas.Admin.Controllers
{
    public class OutEmailsController : BaseAdminController
    {
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult _AjaxIndex([DataSourceRequest]DataSourceRequest request)
        {
            IQueryable<OutEmail> items = _db.OutEmails;
            DataSourceResult result = items.ToDataSourceResult(request);
            return Json(result);
        }

        [AcceptVerbs(HttpVerbs.Post)]
        public ActionResult _AjaxDelete(OutEmail item, [DataSourceRequest]DataSourceRequest request)
        {
            if (ModelState.IsValid)
            {
                _db.OutEmails.Remove(_db.OutEmails.First(r => r.ID == item.ID));
                _db.SaveChanges();
                CleanCache.CleanOutputCache();
            }
            return Json(new[] { item }.ToDataSourceResult(request, ModelState));
        }
    }
}

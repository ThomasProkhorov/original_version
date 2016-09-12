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
	[Authorize(Roles = "Admin")]
    public class SearchController : BaseAdminController
    {
        #region Index
        public ActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public ActionResult Index(int DDParent, string DDLangCode, string DDSegment, string SeachText)
        {
            if (!string.IsNullOrEmpty(SeachText))
            {
                foreach (string item in SeachText.Split('\n'))
                {
                    _db.SearchModels.Add(new SearchModel() { LangCode = DDLangCode, SearchSegmentName = DDSegment, ParentID = DDParent, Title = item.Trim().Replace("\r", "")});
                }
                _db.SaveChanges();

                CleanCache.CleanSearchModelCache();
            }

            return View();
        }

        public ActionResult _AjaxIndex([DataSourceRequest]DataSourceRequest request)
        {
            IQueryable<SearchModel> items = _db.SearchModels;
            DataSourceResult result = items.ToDataSourceResult(request);
            return Json(result);      
        }


        [AcceptVerbs(HttpVerbs.Post)]
        public ActionResult _AjaxDelete([DataSourceRequest]DataSourceRequest request, SearchModel item)
        {

            if (ModelState.IsValid)
            {
            SearchModel obj = _db.SearchModels.Find(item.ID);
            _db.Entry(obj).State = EntityState.Deleted;

            DeleteChildRows(obj.LangCode, obj.SearchSegmentName, obj.ID);
            _db.SaveChanges();

            CleanCache.CleanSearchModelCache();
            }

            return Json(new[] { item }.ToDataSourceResult(request, ModelState));
        }


        //recursive delete
        public void DeleteChildRows(string LangCode,string  SearchSegmentName,int ParentID)
        {
            List<int> li = new List<int>();
            foreach (SearchModel item in _db.SearchModels.Where(r => r.LangCode == LangCode && r.SearchSegmentName == SearchSegmentName && r.ParentID == ParentID))
            {
                _db.Entry(item).State = EntityState.Deleted;
                li.Add(item.ID);
            }

            foreach(int item in li) DeleteChildRows(LangCode, SearchSegmentName, item);

        }

        public JsonResult _AjaxLoadingParent(string text)
        {
            List<Uco.Models.SearchModel> l = new List<Uco.Models.SearchModel>();
            l = _db.SearchModels.OrderBy(r => r.ParentID).ToList();
            l.Insert(0, new Uco.Models.SearchModel() { ID = 0, ParentID = 0, Title = "ראשי" });

            return Json(l, JsonRequestBehavior.AllowGet);

        }

		//AJAX GRID

        [AcceptVerbs(HttpVerbs.Post)]
        public ActionResult _AjaxSave([DataSourceRequest]DataSourceRequest request, SearchModel item)
        {
            if (ModelState.IsValid)
            {
                SearchModel obj = _db.SearchModels.Find(item.ID);
                TryUpdateModel(obj);
                _db.Entry(obj).State = EntityState.Modified;
                _db.SaveChanges();

                CleanCache.CleanSearchModelCache();
            }

            return Json(new[] { item }.ToDataSourceResult(request, ModelState));
        }

        [AcceptVerbs(HttpVerbs.Post)]
        public ActionResult _AjaxInsert([DataSourceRequest]DataSourceRequest request, SearchModel item)
        {

            if (ModelState.IsValid)
            {
                SearchModel obj = item;
                if (TryUpdateModel(obj))
                {
                    _db.SearchModels.Add(obj);
                    _db.SaveChanges();

                    item.ID = obj.ID;
                    CleanCache.CleanSearchModelCache();
                }
            }

            return Json(new[] { item }.ToDataSourceResult(request, ModelState));
        }
        #endregion

        //#region Pages

        //public ActionResult IndexPages()
        //{
        //    return View();
        //}

        //[GridAction]
        //public ActionResult _AjaxIndexPages()
        //{
        //    return View(new GridModel(
        //        _db.SearchPages.Select(r => new
        //        {
        //            ID = r.ID,
        //            Title = r.Title,
        //            SeoUrlName = r.SeoUrlName
        //        }).ToList()
        //    ));
        //}

        //[AcceptVerbs(HttpVerbs.Post)]
        //[GridAction]
        //public ActionResult _AjaxDeletePages(int ID)
        //{
        //    SearchPage obj = _db.SearchPages.Find(ID);
        //    _db.Entry(obj).State = EntityState.Deleted;

        //    _db.SaveChanges();

        //    CleanCache.CleanCacheAfterPageEdit();

        //    return View(new GridModel(
        //        _db.SearchPages.Select(r => new
        //        {
        //            ID = r.ID,
        //            Title = r.Title,
        //            SeoUrlName = r.SeoUrlName
        //        }).ToList()
        //    ));
        //}

        //public ActionResult EditPages(int ID)
        //{
        //    return View(_db.SearchPages.Find(ID));
        //}

        //[HttpPost]
        //public ActionResult EditPages(int ID, SearchPage obj)
        //{
        //    obj.ID = ID;
        //    _db.Entry(obj).State = EntityState.Modified;
        //    _db.SaveChanges();

        //    CleanCache.CleanCacheAfterPageEdit();

        //    return RedirectToAction("IndexPages");
        //}

        //[HttpPost]
        //public ActionResult _GetSearchTags(string text, string Segment)
        //{
        //    if (!string.IsNullOrEmpty(text))
        //    {
        //        return new JsonResult { Data = SF.GetSearchModelRepository().Where(r => r.SearchSegmentName == Segment && r.Title.Contains(text)).ToList().Select(p => p.Title).ToList() };
        //    }
        //    else return new JsonResult { Data = null };
        //}

        

        //#endregion

        //#region ArticlesSpinner

        //public ActionResult IndexArticlesSpinner()
        //{
        //    return View();
        //}

        //[GridAction]
        //public ActionResult _AjaxIndexArticlesSpinner()
        //{
        //    return View(new GridModel(
        //        _db.ArticlesSpinners.ToList()
        //    ));
        //}

        //[AcceptVerbs(HttpVerbs.Post)]
        //[GridAction]
        //public ActionResult _AjaxDeleteArticlesSpinner(int ID)
        //{
        //    ArticlesSpinner obj = _db.ArticlesSpinners.Find(ID);
        //    _db.Entry(obj).State = EntityState.Deleted;
        //    _db.SaveChanges();

        //    CleanCache.CleanArticlesSpinnerCache();

        //    return View(new GridModel(
        //        _db.ArticlesSpinners.ToList()
        //    ));
        //}

        //public ActionResult EditArticlesSpinner(int ID)
        //{
        //    return View(_db.ArticlesSpinners.Find(ID));
        //}

        //[HttpPost]
        //public ActionResult EditArticlesSpinner(int ID, ArticlesSpinner obj)
        //{
        //    obj.ID = ID;
        //    _db.Entry(obj).State = EntityState.Modified;
        //    _db.SaveChanges();

        //    CleanCache.CleanArticlesSpinnerCache();

        //    return RedirectToAction("IndexArticlesSpinner");
        //}

        //public ActionResult CreateArticlesSpinner()
        //{
        //    return View();
        //}

        //[HttpPost]
        //public ActionResult CreateArticlesSpinner(ArticlesSpinner tc)
        //{
        //    if (ModelState.IsValid)
        //    {
        //        _db.ArticlesSpinners.Add(tc);
        //        _db.SaveChanges();

        //        CleanCache.CleanArticlesSpinnerCache();

        //        return RedirectToAction("IndexArticlesSpinner");
        //    }
        //    return View(tc);
        //}

        //#endregion
    }
}

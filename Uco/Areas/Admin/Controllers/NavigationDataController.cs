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
using System.Collections;
using Uco.Infrastructure.Repositories;
using System.Data.Entity;
using System.Web.Helpers;

namespace Uco.Areas.Admin.Controllers
{
    [Authorize(Roles = "Admin,DomainAdmin")]
    public class NavigationDataController : BaseAdminController
    {
        #region Index
        public ActionResult Index(string Segment)
        {
            ViewBag.MessageRed = TempData["MessageRed"];
            ViewBag.MessageYellow = TempData["MessageYellow"];
            ViewBag.MessageGreen = TempData["MessageGreen"];

            if (string.IsNullOrEmpty(Segment))
            {
                string NavigationSegments = RP.GetAdminCurrentSettingsRepository().NavigationSegments;
                if (NavigationSegments == null) { NavigationSegments = string.Empty; }
                List<String> l = NavigationSegments.Split(',').ToList();
                if (l.Count != 0) Segment = l.FirstOrDefault();
            }
            ViewBag.CurrentSegment = Segment;
            return View();
        }

        public ActionResult _AjaxIndex([DataSourceRequest]DataSourceRequest request, string Segment)
        {
            IQueryable<NavigationData> items = _db.NavigationDatas.Where(r => r.DomainID == CurrentSettings.ID && r.Segment == Segment);
            DataSourceResult result = items.ToDataSourceResult(request);
            return Json(result);
        }


        [AcceptVerbs(HttpVerbs.Post)]
        public ActionResult _AjaxDelete(NavigationData item, [DataSourceRequest]DataSourceRequest request)
        {
            if (ModelState.IsValid)
            {
                _db.NavigationDatas.Remove(_db.NavigationDatas.First(r => r.ID == item.ID && r.DomainID == CurrentSettings.ID));
                DeleteChildRows(item.Segment, item.ID, 5);
                _db.SaveChanges();

                CleanCache.CleanOutputCache();
            }
            return Json(new[] { item }.ToDataSourceResult(request, ModelState));
        }


        //recursive delete
        public void DeleteChildRows(string Segment, int ParentID, int MaxLoops)
        {
            List<int> li = new List<int>();
            foreach (NavigationData item in _db.NavigationDatas.Where(r => r.Segment == Segment && r.ParentID == ParentID))
            {
                _db.Entry(item).State = EntityState.Deleted;
                li.Add(item.ID);
            }
            if (MaxLoops > 0)
            {
                foreach (int item in li) DeleteChildRows(Segment, item, MaxLoops - 1);
            }

        }

        //AJAX GRID

        [AcceptVerbs(HttpVerbs.Post)]
        public ActionResult _AjaxInsert([DataSourceRequest] DataSourceRequest request, NavigationData item)
        {
            if (ModelState.IsValid)
            {
                item.Segment = Request["Segment"];
                item.DomainID = RP.GetAdminCurrentSettingsRepository().ID;
                if (string.IsNullOrEmpty(item.LangCode)) item.LangCode = SF.GetLangCodeThreading();

                _db.NavigationDatas.Add(item);
                _db.SaveChanges();

                RP.CleanNavigationDataRepository(AdminCurrentSettingsRepository.ID);
            }
            return Json(new[] { item }.ToDataSourceResult(request, ModelState));           
        }

        [AcceptVerbs(HttpVerbs.Post)]
        public ActionResult _AjaxSave([DataSourceRequest] DataSourceRequest request, NavigationData item)
        {
            if (ModelState.IsValid)
            {
                item.Segment = Request["Segment"];
                item.DomainID = RP.GetAdminCurrentSettingsRepository().ID;
                if (string.IsNullOrEmpty(item.LangCode)) item.LangCode = SF.GetLangCodeThreading();

                _db.Entry(item).State = EntityState.Modified;
                _db.SaveChanges();

                RP.CleanNavigationDataRepository(AdminCurrentSettingsRepository.ID);
            }

            return Json(new[] { item }.ToDataSourceResult(request, ModelState));
        }

        [HttpPost]
        public ActionResult _AjaxLoadingParent()
        {
            string Segment = Request["Segment"];
            List<NavigationData> l = new List<NavigationData>();
            l.Add(new NavigationData() { ID = 0, Title = "-" });
            l.AddRange(RP.GetCurrentNavigationDataRepository().ToList());
            return new JsonResult { Data = new SelectList(l, "ID", "Title") };
        }

        #endregion

        //#region TreeView

        ////[AcceptVerbs(HttpVerbs.Post)]
        //public ActionResult _TreeViewLoading(TreeViewItem node, string Segment)
        //{
        //    int parentId = !string.IsNullOrEmpty(node.Id) ? (int)Convert.ToInt32(node.Id) : 0;

        //    if (parentId == 0)
        //    {
        //        IEnumerable nodes = RP.GetCurrentNavigationDataRepository().Where(r => r.Segment == Segment && r.ParentID == parentId)
        //            .Select(item => new
        //            {
        //                Text = item.Title,
        //                Value = item.ID.ToString(),
        //                LoadOnDemand = false,
        //                Expanded = true,
        //                Enabled = true,
        //                Items = RP.GetCurrentNavigationDataRepository().Where(r2 => r2.Segment == Segment && r2.ParentID == item.ID)
        //                    .Select(item2 => new
        //                    {
        //                        Text = item2.Title,
        //                        Value = item2.ID.ToString(),
        //                        LoadOnDemand = RP.GetCurrentNavigationDataRepository().Count(r => r.Segment == Segment && r.ParentID == item2.ID) != 0,
        //                        Enabled = true
        //                    })
        //            });
        //        return new JsonResult { Data = nodes };
        //    }
        //    else
        //    {
        //        IEnumerable nodes = RP.GetCurrentNavigationDataRepository().Where(r => r.Segment == Segment && r.ParentID == parentId)
        //            .Select(item => new
        //            {
        //                Text = item.Title,
        //                Value = item.ID.ToString(),
        //                LoadOnDemand = RP.GetCurrentNavigationDataRepository().Count(r => r.Segment == Segment && r.ParentID == item.ID) != 0,
        //                Enabled = true
        //            });
        //        return new JsonResult { Data = nodes };
        //    }
        //}

        //public ActionResult _TreeDrop(int item, int destinationitem, string position)
        //{
        //    NavigationData ItemPage = RP.GetCurrentNavigationDataRepository().FirstOrDefault(r => r.ID == item);
        //    NavigationData DestinationItemPage = RP.GetCurrentNavigationDataRepository().FirstOrDefault(r => r.ID == destinationitem);

        //    if (item == destinationitem) return Content("false");
        //    if (ItemPage.ID == DestinationItemPage.ParentID) return Content("false");

        //    if (position == "over")
        //    {
        //        ItemPage.ParentID = destinationitem;
        //        _db.Entry(ItemPage).State = EntityState.Modified;
        //        _db.SaveChanges();
        //        CleanCache.CleanMenuCache();
        //        return Content("true");
        //    }
        //    else if (position == "before")
        //    {
        //        ItemPage.ParentID = DestinationItemPage.ParentID;
        //        ItemPage.Order = DestinationItemPage.Order;
        //        _db.Entry(ItemPage).State = EntityState.Modified;

        //        foreach (NavigationData item2 in _db.NavigationDatas.Where(r => r.ParentID == (int)DestinationItemPage.ParentID && r.Order >= DestinationItemPage.Order && r.ID != (int)ItemPage.ID))
        //        {
        //            item2.Order = item2.Order + 1;
        //            _db.Entry(item2).State = EntityState.Modified;
        //        }
        //        _db.SaveChanges();

        //        CleanCache.CleanMenuCache();
        //        return Content("true");
        //    }
        //    else if (position == "after")
        //    {
        //        foreach (NavigationData item2 in _db.NavigationDatas.Where(r => r.ParentID == (int)DestinationItemPage.ParentID && r.Order >= DestinationItemPage.Order && r.ID != (int)DestinationItemPage.ID))
        //        {
        //            item2.Order = item2.Order + 2;
        //            _db.Entry(item2).State = EntityState.Modified;
        //        }

        //        ItemPage.ParentID = DestinationItemPage.ParentID;
        //        ItemPage.Order = DestinationItemPage.Order + 1;
        //        _db.Entry(ItemPage).State = EntityState.Modified;

        //        _db.SaveChanges();

        //        CleanCache.CleanMenuCache();
        //        return Content("true");
        //    }

        //    return Content("false");
        //}

        //public ActionResult _TreeItemDelete(int ID)
        //{
        //    NavigationData obj = _db.NavigationDatas.Find(ID);
        //    _db.Entry(obj).State = EntityState.Deleted;

        //    DeleteChildRows(obj.Segment, obj.ID, 5);

        //    _db.SaveChanges();

        //    RP.CleanNavigationDataRepository(AdminCurrentSettingsRepository.ID);

        //    return Content("true");
        //}

        //public ActionResult _TreeItemEdit(string EditItemTitle, string EditItemUrl, bool EditItemIsNew, string EditItemSegment, int EditItemID, int EditItemParentID)
        //{
        //    if (string.IsNullOrEmpty(EditItemTitle) || string.IsNullOrEmpty(EditItemUrl)) return Content("כותרת וקישור חובה");

        //    if (EditItemIsNew)
        //    {
        //        Uco.Models.NavigationData EditItem = new Uco.Models.NavigationData() { Segment = EditItemSegment, Order = 100, ParentID = EditItemParentID, DomainID = RP.GetAdminCurrentSettingsRepository().ID };
        //        EditItem.Title = EditItemTitle;
        //        EditItem.Url = EditItemUrl;
        //        _db.NavigationDatas.Add(EditItem);
        //        _db.SaveChanges();
        //        RP.CleanNavigationDataRepository(AdminCurrentSettingsRepository.ID);
        //        return Content("true");
        //    }
        //    else
        //    {
        //        NavigationData item = _db.NavigationDatas.FirstOrDefault(r => r.ID == EditItemID);
        //        item.Title = EditItemTitle;
        //        item.Url = EditItemUrl;
        //        _db.Entry(item).State = EntityState.Modified;
        //        _db.SaveChanges();
        //        RP.CleanNavigationDataRepository(AdminCurrentSettingsRepository.ID);
        //        return Content("true");
        //    }
        //}

        //public JsonResult _TreeItemGet(int ID)
        //{
        //    int DomainID = RP.GetAdminCurrentSettingsRepository().ID;
        //    NavigationData obj = _db.NavigationDatas.FirstOrDefault(r => r.ID == ID && r.DomainID == DomainID);
        //    return Json(obj);
        //}

        //[HttpPost]
        //public ActionResult _AjaxLoadingParent(string text)
        //{
        //    var l = RP.GetCurrentNavigationDataRepository();
        //    return new JsonResult { Data = new SelectList(l.ToList(), "ID", "Title") };
        //}

        //#endregion
    }
}

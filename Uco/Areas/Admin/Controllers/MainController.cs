using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Uco.Models;
using System.Collections;
using System.Reflection;
using System.Web.Security;
using Uco.Infrastructure;
using System.Data.Entity;
using Uco.Infrastructure.Repositories;
using System.Data;
using Uco.Infrastructure.Livecycle;
using Uco.Models.Overview;
using Kendo.Mvc.UI;
using Kendo.Mvc;
using Kendo.Mvc.Extensions;
using System.Linq.Expressions;
using Uco.Infrastructure.EntityExtensions;
using System.IO;
using System.Text;

namespace Uco.Areas.Admin.Controllers
{
    [AuthorizeAdmin]
    public class MainController : BaseAdminController
    {

        #region AbstractPageEdit

        public ActionResult Index()
        {
            return View();
        }

        public ActionResult View(int id)
        {
            if (_db.AbstractPages.Count(r => r.DomainID == AdminCurrentSettingsRepository.ID && r.ID == id) > 0)
            {
                AbstractPage ap = _db.AbstractPages.Single(r => r.DomainID == AdminCurrentSettingsRepository.ID && r.ID == id);
                return View(ap);
            }
            return Content("<h1>Can't find page</h1>");
        }

        [AcceptVerbs(HttpVerbs.Get)]
        public ActionResult CreateList(int id)
        {
            if (id == 0) return RedirectToAction("Index");
            ViewBag.id = id;
            return View(GetChildClasses(id));
        }

        [AcceptVerbs(HttpVerbs.Post)]
        public ActionResult CreateList(int MetadataToken, int id)
        {
            if (id == 0) return RedirectToAction("Index");
            if (MetadataToken != 0)
            {
                foreach (Type item in GetChildClasses(id))
                {
                    if (MetadataToken == item.MetadataToken) return RedirectToAction("Create", new { MetadataToken = item.MetadataToken, id = id });
                }
            }
            ViewBag.id = id;
            return View(GetChildClasses(id));
        }

        public List<Type> GetChildClasses(int ID)
        {
            List<Type> childClasses = new List<Type>();
            var ap = _db.AbstractPages.FirstOrDefault(r => r.DomainID == AdminCurrentSettingsRepository.ID && r.ID == ID);
            Type ParentClass = ap.GetType();
            foreach (Type item in RP.GetPageTypesReprository())
            {
                if (SF.GetTypeRestrictParentsAttribute(item).Contains(ParentClass.Name)) childClasses.Add(item);
            }
            return childClasses;
        }

        public object GetClassByToken(int MetadataToken, int id)
        {
            object AbstractClass;
            if (MetadataToken != 0)
            {
                foreach (Type item in GetChildClasses(id))
                {
                    if (MetadataToken == item.MetadataToken)
                    {
                        AbstractClass = Activator.CreateInstance(item);

                        MethodInfo method = item.GetMethod("BeforeCreate");
                        method.Invoke(AbstractClass, new Object[] { id });

                        return AbstractClass;
                    }
                }
            }
            return null;
        }

        [AcceptVerbs(HttpVerbs.Get)]
        public ActionResult Create(int id, int MetadataToken)
        {
            if (CheckPermissions(id) == false) return Redirect("~/Account/LogOn?returnUrl=/Admin");

            ViewBag.id = id;
            ViewBag.MetadataToken = MetadataToken;
            return View(GetClassByToken(MetadataToken, id));
        }

        [AcceptVerbs(HttpVerbs.Post)]
        public ActionResult Create(AbstractPage ap, int id)
        {

            if (ModelState.IsValid)
            {
                if (CheckPermissions(id) == false) return Redirect("~/Account/LogOn?returnUrl=/Admin");

                AbstractPage Parent = _db.AbstractPages.FirstOrDefault(r => r.ID == id);
                if (Parent == null)
                {
                    ap.ParentID = 0;
                    ap.DomainID = CurrentSettings.ID;
                    if (ap.RouteUrl != "l") ap.LanguageCode = CurrentSettings.LanguageCode;
                }
                else
                {
                    ap.ParentID = Parent.ID;
                    ap.DomainID = Parent.DomainID;
                    if (ap.RouteUrl != "l") ap.LanguageCode = Parent.LanguageCode;
                }

                if (ap.SeoUrlName == null || ap.SeoUrlName == "") ap.SeoUrlName = Uco.Infrastructure.SF.CleanUrl(ap.Title);
                else ap.SeoUrlName = Uco.Infrastructure.SF.CleanUrl(ap.SeoUrlName);

                if (_db.AbstractPages.Count(r => r.DomainID == ap.DomainID && r.SeoUrlName == ap.SeoUrlName) != 0)
                {
                    ModelState.AddModelError("SeoUrlName", "Title not unic. Please specify Url Name different from title");
                    return View(ap);
                }
                ap.CreateTime = DateTime.Now;
                if (_db.AbstractPages.Where(r => r.ParentID == ap.ParentID && r.DomainID == ap.DomainID).Count() == 0) ap.Order = 1;
                else ap.Order = _db.AbstractPages.Where(r => r.ParentID == ap.ParentID && r.DomainID == ap.DomainID).Max(r => r.Order) + 1;
                ap.OnCreate();
                _db.AbstractPages.Add(ap);

                try
                {
                    _db.SaveChanges();
                }
                catch (DataException error)
                {
                    //Log the error (add a variable name after DataException)
                    ModelState.AddModelError("", "Unable to save changes. Try again, and if the problem persists, see your system administrator. Error:" + error.Message + ". Data:" + error.Data);
                    View(ap);
                }

                ap.OnCreated();

                CleanCache.CleanCacheAfterPageEdit();

                return RedirectToAction("View", new { id = ap.ID });
            }
            return View(ap);
        }

        [AcceptVerbs(HttpVerbs.Get)]
        public ActionResult Edit(int id, bool? red)
        {
            if (CheckPermissions(id) == false) return Redirect("~/Account/LogOn?returnUrl=/Admin");

            if (_db.AbstractPages.Count(r => r.DomainID == AdminCurrentSettingsRepository.ID && r.ID == id) > 0)
            {
                var ap = _db.AbstractPages.Single(r => r.DomainID == AdminCurrentSettingsRepository.ID && r.ID == id);
                return View(ap);
            }
            return View();
        }

        [AcceptVerbs(HttpVerbs.Post)]
        public ActionResult Edit(AbstractPage ap, int id, bool? red)
        {
            if (ModelState.IsValid)
            {
                if (CheckPermissions(id) == false) return Redirect("~/Account/LogOn?returnUrl=/Admin");

                if (ap.SeoUrlName == null || ap.SeoUrlName == "") ap.SeoUrlName = Uco.Infrastructure.SF.CleanUrl(ap.Title);
                else ap.SeoUrlName = Uco.Infrastructure.SF.CleanUrl(ap.SeoUrlName);

                AbstractPage Parent = _db.AbstractPages.FirstOrDefault(r => r.ID == ap.ParentID);
                if (Parent == null)
                {
                    ap.ParentID = 0;
                    ap.DomainID = AdminCurrentSettingsRepository.ID;
                    if (ap.RouteUrl != "l") ap.LanguageCode = CurrentSettings.LanguageCode;
                }
                else
                {
                    ap.ParentID = Parent.ID;
                    ap.DomainID = Parent.DomainID;
                    if (ap.RouteUrl != "l") ap.LanguageCode = Parent.LanguageCode;
                }

                if (_db.AbstractPages.Count(r => r.DomainID == AdminCurrentSettingsRepository.ID && r.ID != ap.ID && r.RouteUrl == ap.RouteUrl && r.SeoUrlName == ap.SeoUrlName) != 0)
                {
                    ModelState.AddModelError("SeoUrlName", "Title not unic. Please specify Url Name different from title");
                    return View(ap);
                }
                ap.ChangeTime = DateTime.Now;
                ap.OnEdit();

                _db.Entry(ap).State = EntityState.Modified;

                try
                {
                    _db.SaveChanges();
                }
                catch (DataException error)
                {
                    ModelState.AddModelError("", "Unable to save changes. Try again, and if the problem persists, see your system administrator. Error:" + error.Message + ". Data:" + error.Data);
                    return View(ap);
                }

                ap.OnEdited();

                CleanCache.CleanCacheAfterPageEdit();

                if (red != null && red == true) Response.Redirect(ap.Url);
                else return RedirectToAction("View", new { id = ap.ID });
            }
            return View(ap);
        }

        [AcceptVerbs(HttpVerbs.Get)]
        public ActionResult Delete(int id)
        {
            if (CheckPermissions(id) == false) return Redirect("~/Account/LogOn?returnUrl=/Admin");

            if (id == 0) RedirectToAction("View", new { id = id });
            if (_db.AbstractPages.Count(r => r.DomainID == AdminCurrentSettingsRepository.ID && r.ID == id) == 0) throw new NotImplementedException("Can't find page to delete");

            AbstractPage ap = _db.AbstractPages.Single(r => r.DomainID == AdminCurrentSettingsRepository.ID && r.ID == id);
            ViewBag.id = id;
            ViewBag.Title = ap.Title;
            return View();
        }

        [AcceptVerbs(HttpVerbs.Post)]
        public ActionResult Delete(int id, string Title)
        {
            if (CheckPermissions(id) == false) return Redirect("~/Account/LogOn?returnUrl=/Admin");

            if (id == 0) RedirectToAction("View", new { id = id });
            if (id == 1) throw new NotImplementedException("Can't delete root page");

            AbstractPage ap = _db.AbstractPages.FirstOrDefault(r => r.DomainID == AdminCurrentSettingsRepository.ID && r.ID == id);
            int parent = ap.ParentID;
            ap.OnDelete();
            DeleteChildPages(ap);
            _db.Entry(ap).State = EntityState.Deleted;
            _db.SaveChanges();

            ap.OnDeleted();

            CleanCache.CleanCacheAfterPageEdit();

            return RedirectToAction("View", new { id = parent });
        }

        private void DeleteChildPages(AbstractPage ap)
        {
            foreach (AbstractPage item in _db.AbstractPages.Where(r => r.DomainID == AdminCurrentSettingsRepository.ID && r.ParentID == ap.ID).ToList())
            {
                DeleteChildPages(item);
                item.OnDelete();
                _db.Entry(item).State = EntityState.Deleted;
                _db.SaveChanges();
                item.OnDeleted();
            }
        }

        private bool CheckPermissions(int PageID)
        {
            if (SF.UsePermissions())
            {
                AbstractPage EditPage = _db.AbstractPages.FirstOrDefault(r => r.ID == PageID && r.DomainID == AdminCurrentSettingsRepository.ID);
                _db.Entry(EditPage).State = EntityState.Detached;
                if (EditPage == null) return false;
                if (LS.CurrentUser.RolesList.Intersect(EditPage.RolesEditList).Count() == 0)
                {
                    TempData["Error"] = "You don't have permissions to Edit this page";
                    return false;
                }
            }
            return true;
        }

        #endregion

        #region Admin changes

        [AcceptVerbs(HttpVerbs.Post)]
        public ActionResult ChangeLang(string LangSelectList)
        {
            if (!string.IsNullOrEmpty(LangSelectList)) Session["LangSelectList"] = LangSelectList;
            return Redirect(Request.UrlReferrer.ToString());
        }

        [AcceptVerbs(HttpVerbs.Post)]
        public ActionResult ChangeSkin(string SkinSelectList)
        {
            if (!string.IsNullOrEmpty(SkinSelectList)) Session["SkinSelectList"] = SkinSelectList;
            return Redirect(Request.UrlReferrer.ToString());
        }

        [Authorize(Roles = "Admin")]
        [AcceptVerbs(HttpVerbs.Post)]
        public ActionResult ChangeDomain(int? DomainSelectList)
        {
            if (DomainSelectList != null) RP.ChangeAdminCurrentSettingsRepository((int)DomainSelectList);
            return Redirect(Request.UrlReferrer.ToString());
        }

        #endregion

        #region TreeView

        public JsonpResult _TreeViewLoading(int? ID)
        {
            int parentId = (ID == null ? 0 : (int)ID);
            var pages = RP.GetAdminMenuRepository();

            string domain = string.Empty;

            if (!HttpContext.Request.IsLocal && SF.UseMultiDomain())
            {
                domain = "http://" + RP.GetAdminCurrentSettingsRepository().Domain;
            }

            IEnumerable nodes = pages.Where(r => r.ParentID == parentId)
                .Select(item => new
                {
                    Text = item.Title,
                    ID = item.ID.ToString(),
                    hasChildren = (pages.Count(r2 => r2.ParentID == item.ID) > 0),
                    imageUrl = domain + Url.Content(item.Image),
                    Url = (string.IsNullOrEmpty(item.RedirectTo) ? (domain + item.Url) : item.RedirectTo),
                    SpriteCssClass = "",
                    expanded = (item.IsLangRoot),
                    ReportsTo = parentId
                });

            return this.Jsonp(nodes);
        }

        public ActionResult _TreeDrop(int item, int destinationitem, string position)
        {
            if (CheckPermissions(item) == false || CheckPermissions(destinationitem) == false) return Content("false");

            AbstractPage ItemPage = _db.AbstractPages.FirstOrDefault(r => r.DomainID == AdminCurrentSettingsRepository.ID && r.ID == item);
            AbstractPage DestinationItemPage = _db.AbstractPages.FirstOrDefault(r => r.DomainID == AdminCurrentSettingsRepository.ID && r.ID == destinationitem);

            if (ItemPage.ParentID == 0 || ItemPage.RouteUrl == "d") return Content("false");

            if (position == "over")
            {
                Type t = ItemPage.GetType();
                if (GetChildClasses(destinationitem).Contains(t))
                {
                    AbstractPage Parent = _db.AbstractPages.FirstOrDefault(r => r.ID == destinationitem);
                    ItemPage.ParentID = Parent.ID;
                    ItemPage.DomainID = Parent.DomainID;
                    if (ItemPage.RouteUrl != "l") ItemPage.LanguageCode = Parent.LanguageCode;

                    _db.Entry(ItemPage).State = EntityState.Modified;
                    _db.SaveChanges();

                    CleanCache.CleanCacheAfterPageEdit();

                    return Content("true");
                }
                else
                {
                    return Content("false");
                }
            }
            else if (position == "before")
            {
                if (DestinationItemPage.ID == 1) return Content("false");
                if (GetChildClasses((int)DestinationItemPage.ParentID).Contains(ItemPage.GetType()))
                {
                    ItemPage.ParentID = DestinationItemPage.ParentID;
                    ItemPage.Order = DestinationItemPage.Order;
                    _db.Entry(ItemPage).State = EntityState.Modified;

                    foreach (AbstractPage item2 in _db.AbstractPages.Where(r => r.DomainID == AdminCurrentSettingsRepository.ID && r.ParentID == (int)DestinationItemPage.ParentID && r.Order >= DestinationItemPage.Order && r.ID != (int)ItemPage.ID))
                    {
                        item2.Order = item2.Order + 1;
                        _db.Entry(item2).State = EntityState.Modified;
                    }

                    _db.SaveChanges();

                    CleanCache.CleanCacheAfterPageEdit();

                    return Content("true");
                }
                else
                {
                    return Content("false");
                }
            }
            else if (position == "after")
            {
                if (DestinationItemPage.ID == 1) return Content("false");
                if (GetChildClasses((int)DestinationItemPage.ParentID).Contains(ItemPage.GetType()))
                {
                    foreach (AbstractPage item2 in _db.AbstractPages.Where(r => r.DomainID == AdminCurrentSettingsRepository.ID && r.ParentID == (int)DestinationItemPage.ParentID && r.Order >= DestinationItemPage.Order && r.ID != (int)DestinationItemPage.ID))
                    {
                        item2.Order = item2.Order + 2;
                        _db.Entry(item2).State = EntityState.Modified;
                    }

                    ItemPage.ParentID = DestinationItemPage.ParentID;
                    ItemPage.Order = DestinationItemPage.Order + 1;
                    _db.Entry(ItemPage).State = EntityState.Modified;

                    _db.SaveChanges();

                    CleanCache.CleanCacheAfterPageEdit();

                    return Content("true");
                }
                else
                {
                    return Content("false");
                }
            }

            return Content("false");
        }

        public ActionResult _TreeCopy(int item, int destinationitem)
        {
            if (CheckPermissions(item) == false || CheckPermissions(destinationitem) == false) return Content(LocalizationHelpers.GetLocalResource("~/Areas/Admin/Views/Shared/_TreeView.cshtml", "ErrorPermitions"));

            AbstractPage ItemPage = _db.AbstractPages.FirstOrDefault(r => r.DomainID == AdminCurrentSettingsRepository.ID && r.ID == item);
            AbstractPage DestinationItemPage = _db.AbstractPages.FirstOrDefault(r => r.DomainID == AdminCurrentSettingsRepository.ID && r.ID == destinationitem);

            if (ItemPage.ParentID == 0 || ItemPage.RouteUrl == "d") return Content(LocalizationHelpers.GetLocalResource("~/Areas/Admin/Views/Shared/_TreeView.cshtml", "ErrorDomain"));

            Type t = ItemPage.GetType();
            if (GetChildClasses(destinationitem).Contains(t))
            {
                AbstractPage ap = ItemPage;

                AbstractPage Parent = _db.AbstractPages.FirstOrDefault(r => r.ID == destinationitem);
                ap.ParentID = Parent.ID;
                ap.DomainID = Parent.DomainID;
                ap.PermissionsEdit = Parent.PermissionsEdit;
                ap.PermissionsView = Parent.PermissionsView;

                Session["TreeCopy"] = true;

                if (ap.RouteUrl != "l") ap.LanguageCode = Parent.LanguageCode;

                ap.Title = ap.Title + "-Copy";
                ap.SeoUrlName = (_db.AbstractPages.Max(r => r.ID) + 1).ToString();

                if (_db.AbstractPages.Count(r => r.DomainID == ap.DomainID && r.SeoUrlName == ap.SeoUrlName) != 0)
                {
                    return Content(LocalizationHelpers.GetLocalResource("~/Areas/Admin/Views/Shared/_TreeView.cshtml", "ErrorTitle"));
                }
                ap.CreateTime = DateTime.Now;
                if (_db.AbstractPages.Where(r => r.ParentID == ap.ParentID && r.DomainID == ap.DomainID).Count() == 0) ap.Order = 1;
                else ap.Order = _db.AbstractPages.Where(r => r.ParentID == ap.ParentID && r.DomainID == ap.DomainID).Max(r => r.Order) + 1;
                ap.OnCreate();
                _db.AbstractPages.Add(ap);

                try
                {
                    _db.SaveChanges();
                }
                catch (DataException error)
                {
                    return Content(LocalizationHelpers.GetLocalResource("~/Areas/Admin/Views/Shared/_TreeView.cshtml", "ErrorGeneral") + error.Message);
                }

                AbstractPage CopyItem = _db.AbstractPages.FirstOrDefault(r => r.DomainID == AdminCurrentSettingsRepository.ID && r.ID == item);
                foreach (AbstractPage item2 in _db.AbstractPages.Where(r => r.ParentID == CopyItem.ID).ToList())
                {
                    CopyPage(ap, item2, 5);
                }

                CleanCache.CleanCacheAfterPageEdit();

                return Content("true");
            }
            else
            {
                return Content(LocalizationHelpers.GetLocalResource("~/Areas/Admin/Views/Shared/_TreeView.cshtml", "ErrorParent"));
            }
        }

        private void CopyPage(AbstractPage DestinationItemPage, AbstractPage ItemPage, int MaxLoops)
        {
            //copy
            AbstractPage CopyItem = _db.AbstractPages.FirstOrDefault(r => r.DomainID == AdminCurrentSettingsRepository.ID && r.ID == ItemPage.ID);
            AbstractPage ap = ItemPage;

            AbstractPage Parent = _db.AbstractPages.FirstOrDefault(r => r.ID == DestinationItemPage.ID);
            if (Parent == null)
            {
                ap.ParentID = 0;
                ap.DomainID = CurrentSettings.ID;
                if (ap.RouteUrl != "l") ap.LanguageCode = CurrentSettings.LanguageCode;
            }
            else
            {
                ap.ParentID = Parent.ID;
                ap.DomainID = Parent.DomainID;
                if (ap.RouteUrl != "l") ap.LanguageCode = Parent.LanguageCode;
            }


            ap.Title = ap.Title + "-Copy";
            if (ap.SeoUrlName == null || ap.SeoUrlName == "") ap.SeoUrlName = Uco.Infrastructure.SF.CleanUrl(ap.Title + "-Copy");
            else ap.SeoUrlName = Uco.Infrastructure.SF.CleanUrl(ap.SeoUrlName + "-Copy");

            if (_db.AbstractPages.Count(r => r.DomainID == ap.DomainID && r.SeoUrlName == ap.SeoUrlName) != 0) return;

            ap.CreateTime = DateTime.Now;
            if (_db.AbstractPages.Where(r => r.ParentID == ap.ParentID && r.DomainID == ap.DomainID).Count() == 0) ap.Order = 1;
            else ap.Order = _db.AbstractPages.Where(r => r.ParentID == ap.ParentID && r.DomainID == ap.DomainID).Max(r => r.Order) + 1;
            ap.OnCreate();
            _db.AbstractPages.Add(ap);

            try
            {
                _db.SaveChanges();
            }
            catch
            {
                return;
            }

            if (MaxLoops <= 0) return;
            MaxLoops = MaxLoops - 1;

            foreach (AbstractPage item in _db.AbstractPages.Where(r => r.ParentID == CopyItem.ID).ToList())
            {
                CopyPage(ap, item, MaxLoops);
            }
        }


        [HttpPost]
        public ActionResult _AjaxLoadingParent(string text)
        {
            var l = RP.GetAdminMenuRepository();
            return new JsonResult { Data = new SelectList(l.ToList(), "ID", "Title") };
        }

        #endregion
        #region payments report
        public ActionResult PaymentReport()
        {


            var model = new PaymentReportModel();

            return View(model);
        }

        protected Expression<Func<Order, bool>> GetPredicate(Expression<Func<Order, bool>> predicate, IList<IFilterDescriptor> filters)
        {
            foreach (var f in filters)
            {
                if (f is CompositeFilterDescriptor)
                {
                    predicate = GetPredicate(predicate, (f as CompositeFilterDescriptor).FilterDescriptors);
                }
                else
                {
                    var filter = (f as FilterDescriptor);
                    if (filter.Member == "Date")
                    {

                        DateTime date = (DateTime)filter.Value;
                        if (filter.Operator == FilterOperator.IsLessThanOrEqualTo)
                        {
                            predicate = predicate.MultiSearchAndSql(z => z.CreateOn <= date);
                        }
                        else if (filter.Operator == FilterOperator.IsGreaterThanOrEqualTo)
                        {
                            predicate = predicate.MultiSearchAndSql(z => z.CreateOn >= date);
                        }
                    }
                    if (filter.Member == "ShopID")
                    {
                        int shopid = Convert.ToInt32(filter.Value);
                        predicate = predicate.MultiSearchAndSql(z => z.ShopID == shopid);
                    }
                    if (filter.Member == "IsPaidUp")
                    {
                        bool ispaid = (bool)filter.Value;
                        predicate = predicate.MultiSearchAndSql(z => z.IsPaidUp == ispaid);
                    }
                }
            }

            return predicate;
        }

        [HttpPost]
        public ActionResult _AjaxPaymentReport([DataSourceRequest]DataSourceRequest request)
        {

            //kendo reset manual sort to default =(
            if (request.Sorts.Count == 0)
            {
                request.Sorts.Add(new SortDescriptor("ID",
                    System.ComponentModel.ListSortDirection.Descending));
            }
            Expression<Func<Order, bool>> predicate = x => true
                //  && x.OrderStatus != OrderStatus.New
                // && x.OrderStatus != OrderStatus.Canceled
                //  && x.OrderStatus != OrderStatus.Rejected
       ;
            var items = _db.Orders.Where(predicate)
         .Select(x => new PaymentReportItemModel()
         {
             ID = x.ID,
             ShopID = x.ShopID,
             IsPaidUp = x.IsPaidUp,
             OrderID = x.ID,
             Date = x.CreateOn,
             PaymentMethod = x.PaymentMethod,
             Total = x.Total,


         });

            DataSourceResult result = items.ToDataSourceResult(request);
            foreach (var item in (IEnumerable<PaymentReportItemModel>)result.Data)
            {
                item.Total = item.Cash + item.Card;
                item.TotalStr = ShoppingService.FormatPrice(item.Total);
                item.PayedTo = item.PaymentMethod == PaymentMethod.Credit ? PayedToType.ToAdmin : PayedToType.ToShop;
                item.PayedToStr = RP.T("Enums." + item.PayedTo.ToString()).ToString();
                item.PaymentMethodStr = RP.T("Enums." + item.PaymentMethod.ToString()).ToString();
                item.DateStr = item.Date.HasValue ? item.Date.Value.ToString("dd/MM HH:mm") : "";
            }

            //prepare report model
            var model = new PaymentReportModel();
            //prepare filters, get from kendo grid filter
            if (request.Filters != null)
            {
                predicate = GetPredicate(predicate, request.Filters);

            }
            model.TotalShop = _db.Orders.Where(predicate).Where(x => x.PaymentMethod == PaymentMethod.CreditShopOwner).Select(x => x.Total).DefaultIfEmpty(0).Sum();
            //  model.TotalShop += _db.Orders.Where(predicate).Select(x => x.TotalCash).DefaultIfEmpty(0).Sum();
            model.TotalAdmin = _db.Orders.Where(predicate).Where(x => x.PaymentMethod == PaymentMethod.Credit).Select(x => x.Total).DefaultIfEmpty(0).Sum();
            var grupTotalAdmin = (from r in _db.Orders.Where(predicate).Where(x => !x.LessFee)
                                  group r by new
                                  {
                                      r.ShopID
                                  } into g
                                  select new
                                  {
                                      g.Key.ShopID,
                                      Total = g.Select(x => x.Total).DefaultIfEmpty(0).Sum(),
                                      TotalShopCard = g.Where(x => x.PaymentMethod == PaymentMethod.CreditShopOwner).Select(x => x.Total).DefaultIfEmpty(0).Sum(),
                                  });
            var grupTotalSpecialAdmin = (from r in _db.Orders.Where(predicate).Where(x => x.LessFee)
                                         group r by new
                                         {
                                             r.ShopID
                                         } into g
                                         select new
                                         {
                                             g.Key.ShopID,
                                             Total = g.Select(x => x.Total).DefaultIfEmpty(0).Sum(),
                                             TotalShopCard = g.Where(x => x.PaymentMethod == PaymentMethod.CreditShopOwner).Select(x => x.Total).DefaultIfEmpty(0).Sum(),

                                         });
            // model.TotalFee = (model.TotalShop * 10 / 100) - model.TotalAdmin;
            foreach (var aT in grupTotalAdmin.ToList())
            {
                var shop = ShoppingService.GetShopByID(aT.ShopID);
                if (shop != null)
                {
                    var date1 = items.Where(x => x.Date.HasValue && x.ShopID == aT.ShopID).Select(x => x.Date.Value).DefaultIfEmpty(DateTime.Now).Max();
                    var date2 = items.Where(x => x.Date.HasValue && x.ShopID == aT.ShopID).Select(x => x.Date.Value).DefaultIfEmpty(DateTime.Now).Min();
                    int mouthes = ((date1.Year - date2.Year) * 12) + date1.Month - date2.Month;
                    decimal mounthFee = shop.MounthlyFee * mouthes;

                    model.TotalFee += ((aT.Total + aT.TotalShopCard) * shop.PercentFee / 100) + mounthFee;
                }
            }
            foreach (var aT in grupTotalSpecialAdmin.ToList())
            {
                var shop = ShoppingService.GetShopByID(aT.ShopID);
                if (shop != null)
                {
                    model.TotalFee += ((aT.Total + aT.TotalShopCard) * shop.SpecialPercentFee / 100);// +mounthFee;
                }
            }
            model.TotalFee -= model.TotalAdmin;


            // if (model.TotalFee < 0) { model.TotalFee = 0; }

            model.TotalAdminStr = ShoppingService.FormatPrice(model.TotalAdmin);
            model.TotalShopStr = ShoppingService.FormatPrice(model.TotalShop);
            model.TotalFeeStr = ShoppingService.FormatPrice(model.TotalFee);


            return Json(new { result.Data, result.AggregateResults, result.Errors, result.Total, model });
        }

        [HttpPost]
        public ActionResult _AjaxPaymentMarkAsPaid(List<int> IDs)
        {
            if (IDs != null)
            {
                var orders = _db.Orders.Where(x => IDs.Contains(x.ID)).ToList();
                foreach (var o in orders)
                {
                    o.IsPaidUp = true;

                }
                _db.SaveChanges();
            }
            return Json(new { result = "ok", message = RP.T("Admin.Main._AjaxPaymentMarkAsPaid.MarkedSuccess").ToString() });
        }

        #region payment export
        public ActionResult CSVExport([DataSourceRequest]DataSourceRequest request)
        {
            Expression<Func<Order, bool>> predicate = x => true
                //  && x.OrderStatus != OrderStatus.New
                // && x.OrderStatus != OrderStatus.Canceled
                //  && x.OrderStatus != OrderStatus.Rejected
       ;
            if (request.Filters != null)
            {
                predicate = GetPredicate(predicate, request.Filters);

            }
            var items = _db.Orders.Where(predicate).Select(x => new PaymentReportItemModel()
             {
                 ID = x.ID,
                 ShopID = x.ShopID,
                 IsPaidUp = x.IsPaidUp,
                 OrderID = x.ID,
                 Date = x.CreateOn,
                 PaymentMethod = x.PaymentMethod,
                 Total = x.Total


             }).ToList();
            foreach (var item in items)
            {
                item.Total = item.Cash + item.Card;
                item.TotalStr = ShoppingService.FormatPrice(item.Total);
                item.PayedTo = item.Card > 0 ? PayedToType.ToAdmin : PayedToType.ToShop;
                item.PayedToStr = RP.T("Enums." + item.PayedTo.ToString()).ToString();
                item.PaymentMethodStr = RP.T("Enums." + item.PaymentMethod.ToString()).ToString();
                item.DateStr = item.Date.HasValue ? item.Date.Value.ToString("dd/MM HH:mm") : "";
            }
            // var items = _db.AbstractPages.Where(r => r.Visible == true).OrderBy(r => r.Title).ToList();
            MemoryStream output = new MemoryStream();
            StreamWriter writer = new StreamWriter(output, Encoding.UTF8);
            writer.Write(RP.M("PaymentReportItemModel", "OrderID") + ",");
            writer.Write(RP.M("PaymentReportItemModel", "Date") + ",");
            writer.Write(RP.M("PaymentReportItemModel", "Total") + ",");
            writer.Write(RP.M("PaymentReportItemModel", "PaymentMethod") + ",");
            writer.Write(RP.M("PaymentReportItemModel", "PayedTo"));
            writer.WriteLine();
            var csvQuote = "\"";
            foreach (var item in items)
            {
                writer.Write(item.OrderID); writer.Write(",\"");
                writer.Write(item.DateStr.Replace(csvQuote, csvQuote + csvQuote)); writer.Write("\",\"");
                writer.Write(item.TotalStr.Replace(csvQuote, csvQuote + csvQuote)); writer.Write("\",\"");
                writer.Write(item.PaymentMethodStr.Replace(csvQuote, csvQuote + csvQuote)); writer.Write("\",\"");
                writer.Write(item.PayedToStr.Replace(csvQuote, csvQuote + csvQuote)); writer.Write("\"");
                writer.WriteLine();
            }
            writer.Flush();
            output.Position = 0;
            Encoding heb = Encoding.GetEncoding("windows-1255");
            return File(heb.GetBytes(new StreamReader(output).ReadToEnd()), "text/csv", "PaymentReport_" + (DateTime.Now.ToString("dd/MM HH:mm")) + ".csv");
        }
        #endregion

        #endregion

        #region Other

        public ActionResult LogOut()
        {

            Session.Clear();
            FormsAuthentication.SignOut();
            if (!Response.IsRequestBeingRedirected) Response.Redirect("~/");
            return RedirectToAction("Index");
        }

        #endregion

        #region ajax

        private ShopShipTime GetTime(int ShopID, DayOfWeek en, int From = 480, int To = 1200)
        {
            var time = new ShopShipTime();
            time.ShopID = ShopID;
            time.IsSpecial = false;
            time.Day = en;
            time.Date = DateTime.Now;
            time.TimeFrom = From;
            time.TimeTo = To;
            time.Active = true;
            return time;
        }
        private ShopWorkTime GetWorkTime(int ShopID, DayOfWeek en, int From = 480, int To = 1200)
        {
            var time = new ShopWorkTime();
            time.ShopID = ShopID;
            time.IsSpecial = false;
            time.Day = en;
            time.Date = DateTime.Now;
            time.TimeFrom = From;
            time.TimeTo = To;
            time.Active = true;
            return time;
        }
        [HttpPost]
        public ActionResult ReadShipTimes(int ShopID)
        {

            var list = _db.ShopShipTimes.Where(x => x.ShopID == ShopID
               && !x.IsSpecial).ToList();

            foreach (var en in Enum.GetValues(typeof(DayOfWeek)))
            {
                if (!list.Any(x => x.Day == (DayOfWeek)en))
                {
                    var time = GetTime(ShopID, (DayOfWeek)en);
                    if ((int)en == 0)
                    {
                        time = GetTime(ShopID, (DayOfWeek)en, 1140, 1440);
                        time.Active = false;
                    }
                    else if ((int)en == 5)
                    {
                        time = GetTime(ShopID, (DayOfWeek)en, 480, 840);

                    }

                    _db.ShopShipTimes.Add(time);
                    _db.SaveChanges();
                    list.Add(time);
                }
            }

            return Json(new { result = "ok", data = list });
        }
        [HttpPost]
        public ActionResult ShipTimeChange(ShopShipTime time)
        {
            // if (CurrentShop == null) return Json(new { result = "error", message="You are not registered" });

            var CurTime = _db.ShopShipTimes.FirstOrDefault(x => x.ShopID == time.ShopID
                && x.Day == time.Day
                && !x.IsSpecial);
            if (CurTime != null) //already exist
            {
                CurTime.Active = time.Active;
                CurTime.TimeFrom = time.TimeFrom;
                CurTime.TimeTo = time.TimeTo;
                _db.SaveChanges();
            }
            else
            { //create new day
                time.ShopID = time.ShopID;
                time.IsSpecial = false;
                time.Date = DateTime.Now;
                _db.ShopShipTimes.Add(time);
                _db.SaveChanges();
                CurTime = time;
            }
            return Json(new { result = "ok", data = CurTime });
        }
        [HttpPost]
        public ActionResult ReadWorkTimes(int ShopID)
        {

            var list = _db.ShopWorkTimes.Where(x => x.ShopID == ShopID
               && !x.IsSpecial).ToList();

            foreach (var en in Enum.GetValues(typeof(DayOfWeek)))
            {
                if (!list.Any(x => x.Day == (DayOfWeek)en))
                {
                    var time = GetWorkTime(ShopID, (DayOfWeek)en);
                    if ((int)en == 0)
                    {
                        time = GetWorkTime(ShopID, (DayOfWeek)en, 1140, 1440);
                        time.Active = false;
                    }
                    else if ((int)en == 5)
                    {
                        time = GetWorkTime(ShopID, (DayOfWeek)en, 480, 840);

                    }

                    _db.ShopWorkTimes.Add(time);
                    _db.SaveChanges();
                    list.Add(time);
                }
            }

            return Json(new { result = "ok", data = list });
        }

        [HttpPost]
        public ActionResult WorkTimeChange(ShopWorkTime time)
        {

            var CurTime = _db.ShopWorkTimes.FirstOrDefault(x => x.ShopID == time.ShopID
                && x.Day == time.Day
                && !x.IsSpecial);
            if (CurTime != null) //already exist
            {
                CurTime.Active = time.Active;
                CurTime.TimeFrom = time.TimeFrom;
                CurTime.TimeTo = time.TimeTo;
                _db.SaveChanges();
            }
            else
            { //create new day
                time.ShopID = time.ShopID;
                time.IsSpecial = false;
                time.Date = DateTime.Now;
                _db.ShopWorkTimes.Add(time);
                _db.SaveChanges();
                CurTime = time;
            }
            return Json(new { result = "ok", data = CurTime });
        }
        #endregion
    }
}

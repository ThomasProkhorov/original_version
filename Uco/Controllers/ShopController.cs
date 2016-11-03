using System.Web.Mvc;
using Uco.Infrastructure;
using Uco.Models;
using System.Linq;
using System.Collections.Generic;
using Uco.Infrastructure.Livecycle;
using Uco.Models.Overview;
using System;
using Uco.Infrastructure.Services;
using Uco.Infrastructure.Repositories;
using System.Diagnostics;

using Kendo.Mvc.UI;
using Kendo.Mvc.Extensions;
using System.Linq.Expressions;
using Uco.Infrastructure.EntityExtensions;
using Uco.Models.Shopping;
using System.Threading.Tasks;
using StackExchange.Profiling;
using Uco.Infrastructure.Helpers;

namespace Uco.Controllers
{
    [Localization]
    public partial class ShopController : BaseController
    {
        private ShoppingCartService _shoppingCartService;
        public ShopController()
        {
            _shoppingCartService = new ShoppingCartService(_db);

        }

        #region Service

        private ProductOverviewModel GetProductByID(int productShopID)
        {
            var find = (from ps in _db.ProductShopMap
                        join p in _db.Products
                        on ps.ProductID equals p.ID
                        where ps.ID == productShopID
                        select new ProductOverviewModel()
                        {
                            FullDescription = p.FullDescription,
                            ID = ps.ID,
                            Image = p.Image,
                            Name = p.Name,
                            Price = ps.Price,
                            PriceByUnit = ps.PriceByUnit,
                            ProductID = p.ID,
                            ProductShopID = ps.ID,
                            Quantity = ps.Quantity,
                            Rate = p.Rate,
                            RateCount = p.RateCount,
                            ShopID = ps.ShopID,
                            ShortDescription = p.ShortDescription,
                            SKU = p.SKU,
                            SellCount = ps.SellCount,
                            //  Manufacturer = p.Manufacturer,
                            NoTax = p.NoTax,
                            MeasureUnit = p.MeasureUnit,
                            MeasureUnitStep = p.MeasureUnitStep,
                            ProductMeasureID = p.ProductMeasureID,
                            SoldByWeight = p.SoldByWeight,
                            CategoryID = p.CategoryID,
                            HasImage = p.HasImage,
                            ProductManufacturerID = p.ProductManufacturerID,
                            DisplayOrder = p.DisplayOrder,
                            OrderPosition = ps.OrderPosition,
                            SeoDescription = p.SeoDescription,
                            SeoKeywords = p.SeoKeywords,
                            ContentUnitMeasureID=p.ContentUnitMeasureID,
                            ContentUnitPriceMultiplicator=p.ContentUnitPriceMultiplicator
                        }).FirstOrDefault();

            find.PrepareContentUnitMeasure();

            return find;
        }

        #endregion

        #region Action
        [SSLrequired(MobileOnly = true)]
        public ActionResult Index(int ID = 0, string _escaped_fragment_ = null)
        {
            if (ID == 0)
            {
                if (LS.CurrentHttpContext.Session["ShopID"] != null)
                {
                    ID = (int)LS.CurrentHttpContext.Session["ShopID"];

                    if (ID > 0)
                        return RedirectToAction("Index", new { ID = ID });
                }
            }

            Shop shop = LS.Get<Shop>().FirstOrDefault(r => r.ID == ID);
            if (shop == null || !shop.Active) return Redirect("~/");
            ViewBag.Shop = shop;
            if (Request.Url.AbsolutePath.ToLower().Contains("shop/index/") && shop.SeoUrl != null)
            {
                return Redirect("/" + shop.SeoUrl);
            }
            UserActivityService.InsertShopOpen(LS.CurrentUser.ID, ID
                  , Request.RawUrl,
                  Request.UrlReferrer != null ? Request.UrlReferrer.OriginalString : null
                  , LS.GetUser_IP(Request));
            LS.CurrentHttpContext.Session["ShopID"] = shop.ID;

            if (!string.IsNullOrEmpty(_escaped_fragment_))
            {
                EscapedFragmentDescriptor descriptor = null;
                try
                {
                    descriptor = new EscapedFragmentDescriptor(_escaped_fragment_);
                }
                catch (IndexOutOfRangeException)
                {
                    return HttpNotFound();
                }

                switch (descriptor.Target)
                {
                    case EscapedFragmentDescriptor.Targets.Category:
                        {
                            ViewBag.UseLayout = true;
                            return _GetProductByCategoryAndFilters(descriptor.ShopID, descriptor.CategoryID, null, isForSpider: true);

                        }
                    case EscapedFragmentDescriptor.Targets.Product:
                        {
                            ViewBag.UseLayout = true;
                            return _GetProductPopup(descriptor.ShopID);
                        }
                }
            }
            if (!string.IsNullOrEmpty(shop.Theme))
            {
                this.HttpContext.Items["ShopTheme"] = shop.Theme;
            }
            shop.ShopMessages = new List<ShopMessage>();
            if (Request.Cookies["_slkmess" + shop.ID.ToString()] == null)
            {
                //get messages
                var curDate = DateTime.Now;
                var mess = LS.Get<ShopMessage>().Where(x => x.ShopID == shop.ID
                    && x.Active
                    && (!x.StartDate.HasValue || x.StartDate.Value < curDate)
                    && (!x.EndDate.HasValue || x.EndDate.Value > curDate)).OrderByDescending(x => x.StartDate).FirstOrDefault();
                if (mess != null)
                {
                    shop.ShopMessages.Add(mess);
                }
            }

            return View(shop);
        }

        [HttpGet]
        public ActionResult _ChangeShop()
        {
            return Redirect("~/Landing/Main/Index.html");
        }

        public ActionResult Contact(int? ID)
        {
            if (ID.HasValue)
            {
                if (TempData["ViewData"] != null)
                {
                    ViewData = (ViewDataDictionary)TempData["ViewData"];
                }
                Contact contact = new Contact();
                var shop = LS.GetFirst<Shop>(x => x.ID == ID);
                if (shop != null)
                {
                    contact.DropDownItems = new List<string> {  RP.T("Views.Shared.Contact.DropDownOneText").ToString(), 
                                                                RP.T("Views.Shared.Contact.DropDownTwoText").ToString(), 
                                                                RP.T("Views.Shared.Contact.DropDownThreeText").ToString(),                                                           
                    };
                    if (!string.IsNullOrEmpty(shop.Theme))
                    {
                        this.HttpContext.Items["ShopTheme"] = shop.Theme;
                    }
                    contact.Shop = shop;
                    return View(contact);
                }
            }
            return Redirect("~/Landing/Main/Index.html");
        }

        [HttpPost]
        public ActionResult _AddContactData(int shopID, Contact model)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    UserActivityService.InsertShopContact(LS.CurrentUser.ID, shopID
                        , model.ContactData,
                        model.ContactEmail,
                        model.ContactName
                        , model.ContactPhone
                        , model.DropDownItems

                  , Request.RawUrl,
                  Request.UrlReferrer != null ? Request.UrlReferrer.OriginalString : null
                  , LS.GetUser_IP(Request)
                  );


                    model.ContactData = RP.T("Views.Shared.Contact.DropDownChooseText").ToString().ToLower() + ": " + model.ContactData;
                    if (model.DropDownItems.Count > 0)
                    {
                        model.ContactData = model.ContactData + "<br />" +
                            RP.T("Views.Shared.Contact.TextAreaPlaceholder").ToString().ToLower() + ": " + model.DropDownItems.FirstOrDefault();
                    }
                    model.ContactDate = DateTime.Now;
                    model.ContactReferal = SF.GetCookie("Referal");
                    model.ContactUrl = "<a target='_blank' href='" + Request.UrlReferrer.ToString() + "'>" + Request.UrlReferrer.ToString() + "</a>";
                    model.RoleDefault = "Member";
                    ShoppingService.AddContact(model);

                    return Redirect("/c/contact-sent");
                }
                catch (Exception exc)
                {
                    SF.LogError(exc);
                }
            }
            TempData["ViewData"] = ViewData;
            return RedirectToAction("Contact", new { ID = shopID });
        }

        public async Task<ActionResult> LandingSelectShop(int shopType, string address, string addressTyped, decimal latitude, decimal longitude)
        {
            var foundedshops = ShoppingService.GetNearestShop(shopType, latitude, longitude, address);
            var bestshop = foundedshops.FirstOrDefault();
            Session["address"] = address;
            Session["latitude"] = latitude;
            Session["longitude"] = longitude;
            var sht = LS.Get<ShopType>();
            string type = sht.Where(x => x.ID == shopType).Select(x => x.Name).DefaultIfEmpty("").FirstOrDefault();

            var userActivity = new UserAddressSearchActivity();
            userActivity.Address = address;
            userActivity.CreateOn = DateTime.Now;
            userActivity.IP = LS.GetUser_IP(Request);
            userActivity.RefererUrl = Request.UrlReferrer != null ? Request.UrlReferrer.OriginalString : null;
            userActivity.PageUrl = Request.RawUrl;
            userActivity.Latitude = latitude;
            userActivity.Longitude = longitude;
            userActivity.UserID = LS.CurrentUser.ID;
            userActivity.AddressWroten = addressTyped;
            userActivity.ShopType = type;

            var _url = "/c/noshop";
            if (bestshop != null)
            {
                _url = string.IsNullOrEmpty(bestshop.SeoUrl) ? "/Shop/Index/" + bestshop.ID : "/" + bestshop.SeoUrl;
                userActivity.ShopID = bestshop.ID;
                userActivity.ShopName = bestshop.Name;
                if (foundedshops.Count() > 1)
                {
                    Session["OpenedFromHomePage" + bestshop.ID.ToString()] = DateTime.UtcNow;
                }

            }
            _db.UserAddressSearchActivities.Add(userActivity);
            await _db.SaveChangesAsync();

            return Content(_url);
        }

        public ActionResult NoShop()
        {
            return View();
        }
        #endregion


        public ActionResult MakeCategoryMenuItems()
        {
            var category1 = LS.Get<Category>()
                //.Where(x => x.ParentCategoryID == 0)
                .ToList();
            var shopCatMap = LS.Get<ShopCategory>().Where(x => x.Published);
            foreach (var shop in LS.Get<Shop>())
            {
                var shopID = shop.ID;
                var category = (from c in category1
                                join sc in shopCatMap
                                on c.ID equals sc.CategoryID
                                where sc.ShopID == shopID
                                && c.ParentCategoryID == 0
                                orderby sc.DisplayOrder
                                select c
                            ).Distinct().Take(11).ToList();

                var IDs = category.Select(x => x.ID).ToList();
                var subcats =
                    (from c in category1
                     join sc in shopCatMap
                     on c.ID equals sc.CategoryID
                     where sc.ShopID == shopID
                     && IDs.Contains(c.ParentCategoryID)
                     orderby sc.DisplayOrder
                     select c
                            ).ToList();
                // LS.Get<Category>().Where(x => IDs.Contains(x.ParentCategoryID)).OrderBy(x => x.DisplayOrder).ToList();
                category.AddRange(subcats);
                //process list
                var currentShopModel = _db.ShopCategoryMenus.Where(x => x.ShopID == shopID).ToList();
                _db.ShopCategoryMenus.RemoveRange(currentShopModel);
                _db.SaveChanges();
                int groupNum = 0;
                foreach (var c in category.Where(x => x.ParentCategoryID == 0))
                {
                    int displayOrder = 0;
                    var shopMenu = new ShopCategoryMenu()
                    {
                        CategoryID = c.ID,
                        DisplayOrder = displayOrder,
                        GroupNumber = groupNum,
                        Published = c.Published,
                        ShopID = shopID,
                        Level = 0
                    };
                    _db.ShopCategoryMenus.Add(shopMenu);
                    displayOrder++;
                    foreach (var sub in category.Where(x => x.ParentCategoryID == c.ID))
                    {
                        var shopsubMenu = new ShopCategoryMenu()
                        {
                            CategoryID = sub.ID,
                            DisplayOrder = displayOrder,
                            GroupNumber = groupNum,
                            Published = sub.Published,
                            ShopID = shopID,
                            Level = 1
                        };
                        displayOrder++;
                        _db.ShopCategoryMenus.Add(shopsubMenu);
                    }

                    groupNum++;

                }
                _db.SaveChanges();
            }
            return Content("Coppy categories");
        }


        #region ChildAction

        [ChildActionOnly]
        public ActionResult _TopMenu(int shopID)
        {
            var category1 = LS.Get<Category>()
                //.Where(x => x.ParentCategoryID == 0)
                .ToList();

            var shopMenuCategory = LS.Get<ShopCategoryMenu>().Where(x => x.Published && x.ShopID == shopID).ToList();
            if (shopMenuCategory.Count > 0)
            {
                shopMenuCategory.ForEach((item) =>
                {
                    item.Category = category1.FirstOrDefault(x => x.ID == item.CategoryID);
                });
                ViewBag.ShopID = shopID;
                return View("_TopMenuCustom", shopMenuCategory);
            }

            var shopCatMap = LS.Get<ShopCategory>().Where(x => x.Published);

            var category = (from c in category1
                            join sc in shopCatMap
                            on c.ID equals sc.CategoryID
                            where sc.ShopID == shopID
                            && c.ParentCategoryID == 0
                            orderby sc.DisplayOrder
                            select c
                        ).Distinct().Take(11).ToList();

            var IDs = category.Select(x => x.ID).ToList();
            var subcats =
                (from c in category1
                 join sc in shopCatMap
                 on c.ID equals sc.CategoryID
                 where sc.ShopID == shopID
                 && IDs.Contains(c.ParentCategoryID)
                 orderby sc.DisplayOrder
                 select c
                        ).ToList();
            // LS.Get<Category>().Where(x => IDs.Contains(x.ParentCategoryID)).OrderBy(x => x.DisplayOrder).ToList();
            category.AddRange(subcats);
            ViewBag.ShopID = shopID;

            return View(category);
        }

        [ChildActionOnly]
        public ActionResult _Filters(int shopID)
        {
            //to do method
            //get filter list for shopID            
            var filters = LS.GetForTest<SpecificationAttributeOption>(shopID).ToList();

            return View(filters);
        }


        #endregion

        #region Ajax

        [AcceptVerbs(HttpVerbs.Post)]
        public ActionResult _GetProductsForEndlessScrolling([DataSourceRequest] DataSourceRequest request,
            int shopID, int? categoryID, int[] filtersArray = null, bool isBestSelling = false, string productName = "", bool refreshFilters = false,
            bool favorite = false, bool allOrderedProducts = false, bool deals = false, string keywords = null)
        {
            bool firstBatch = request.Page == 1 ? true : false;
            if (request.PageSize == 0) request.PageSize = 20;
            var page = request.Page;
            List<SpecificationOptionModel> specifications = null;
            string tempString = null;
            List<ProductOverviewModel> list = new List<ProductOverviewModel>();

            if (favorite || allOrderedProducts || deals)
            {
                if (favorite)
                {
                    list.AddRange(LS.SearchProducts(shopID, out specifications, request.Page, request.PageSize, favorite: true, loadSpecifications: firstBatch, showDiscounts: true));
                }
                if (allOrderedProducts)
                {
                    list.AddRange(LS.SearchProducts(shopID, out specifications, request.Page, request.PageSize, allOrderedProducts: true, loadSpecifications: firstBatch, showDiscounts: true));
                }
                if (deals)
                {
                    //list.AddRange(LS.SearchProducts(shopID, out specifications, limit: int.MaxValue, discountedProducts: true, showDiscounts: true));
                    list.AddRange(LS.SearchProducts(shopID, out specifications, request.Page, request.PageSize, discountedProducts: true, showDiscounts: true));
                }
            }
            else
            {
                if (string.IsNullOrEmpty(productName))
                {
                    bool FeautureTop = false;
                    if (!categoryID.HasValue
                        && (filtersArray == null || filtersArray.Length == 0)
                        && string.IsNullOrEmpty(keywords)
                        && !isBestSelling
                        )
                    {
                        var sc = LS.Get<ShopCategoryMenu>().FirstOrDefault(x => x.ShopID == shopID && x.Published == true && x.Level == 0);


                        if (sc != null)
                        {
                            categoryID = sc.CategoryID;
                        }

                    }
                    list = LS.SearchProducts(shopID, out specifications, request.Page, request.PageSize, categoryID: categoryID, filters: filtersArray, loadSpecifications: firstBatch,
                        favorite: favorite, allOrderedProducts: allOrderedProducts, featuredTop: FeautureTop, showDiscounts: true, keywords: keywords).ToList();


                }
                else
                {
                    list = LS.GetProductByName(shopID, productName).Skip(--page * request.PageSize).Take(request.PageSize).ToList(); //search by single name
                }
            }

            int total = request.PageSize * request.Page + 1;
            if (list.Count() < request.PageSize)
            {
                total = request.PageSize * (request.Page - 1) + list.Count();
            }
            var viewList = new List<JsonKeyValue>();
            foreach (var item in list)
            {
                viewList.Add(new JsonKeyValue()
                {
                    Name = item.ProductShopID.ToString(),
                    Value = RenderPartialViewToString("_ProductGalleryItem", item),
                });
            }
            return Json(new
            {
                Data = viewList,
                Total = total,
                Errors = tempString,
                AggregateResults = tempString,
            });
        }

        public ActionResult _GetProductByCategoryAndFilters(int shopID, int? categoryID, int[] filters, string viewIn = "gallery", int skip = 0, int take = 20,
            string keywords = null,
            bool showFirstCategory = false,
            bool isBestSelling = false, string productName = "", bool refreshFilters = true, bool isForSpider = false)
        {

            List<SpecificationOptionModel> specifications = null;
            IEnumerable<ProductOverviewModel> data;
            var profiler = MiniProfiler.Current;
            using (profiler.Step("Step Controller _ByCategoryAndFilters"))
            {
                if (string.IsNullOrEmpty(productName))
                {
                    if (showFirstCategory)
                    {
                        //var sc = LS.Get<ShopCategory>().OrderBy(x => x.DisplayOrder).Where(x => x.Published).FirstOrDefault();
                        //var category1 = LS.Get<Category>().ToList();

                        var sc = LS.Get<ShopCategoryMenu>().FirstOrDefault(x => x.ShopID == shopID && x.Published == true && x.Level == 0);


                        if (sc != null)
                        {
                            categoryID = sc.CategoryID;
                        }
                        else
                        {
                            var shopCatMap = LS.Get<ShopCategory>().Where(x => x.Published);

                            var shc = shopCatMap.OrderBy(x => x.DisplayOrder).FirstOrDefault();
                            if (shc != null)
                            {
                                categoryID = shc.CategoryID;
                            }
                        }
                    }
                    bool FeautureTop = false;
                    if (!categoryID.HasValue
                        && (filters == null || filters.Length == 0)
                        && string.IsNullOrEmpty(keywords)
                        && !isBestSelling
                        )
                    {
                        FeautureTop = true;

                    }
                    data = LS.SearchProducts(
                        shopID: shopID,
                        options: out specifications,
                        page: isForSpider ? 1 : (skip / take) + 1,
                        limit: isForSpider ? -1 : take,
                        categoryID: categoryID,
                        filters: filters,
                        loadSpecifications: skip == 0,
                        keywords: keywords,
                        isBestSelling: isBestSelling,
                        featuredTop: FeautureTop,
                        showDiscounts: true);

                    foreach (var d in data)
                    {
                        //d.ProductNoteText = ShoppingService.GetUserNoteForProduct(d.ProductID, LS.CurrentUser.ID);
                    }
                }
                else
                {
                    data = LS.SearchProducts(shopID, out specifications, (skip / take) + 1, take
                        // , categoryID: categoryID
                        // , filters: filters
                        //, loadSpecifications: skip == 0,
                        , productName: productName
                        //, isBestSelling: isBestSelling
                        // , featuredTop: FeautureTop
                        , showDiscounts: true
                        );
                }


                ViewBag.LastProductNum = skip + take; //data.Count();
                ViewBag.Specifications = specifications;
                ViewBag.RefreshFilters = refreshFilters;
                ViewBag.CategoryID = categoryID;
                if (categoryID.HasValue)
                {
                    var cat = LS.Get<Category>().FirstOrDefault(x => x.ID == categoryID.Value);
                    if (cat != null)
                    {
                        ViewBag.Category = cat;
                    }
                }
                if (!string.IsNullOrEmpty(productName) && data.Count() == 0 || !string.IsNullOrEmpty(keywords) && data.Count() == 0)
                {
                    return Json(new
                    {
                        status = "productNotFound",
                        localizationMessage = RP.T("ShopController.SearchMessage.ProductNotFound").ToString(),
                        localizationTextComponent = RP.Text("ShopController.SearchMessage.ProductNotFoundComponent").ToString()
                    }, JsonRequestBehavior.AllowGet);
                }
            }

            if (data.Count() == 0)
                return Content(string.Empty);
            var shop = LS.Get<Shop>().FirstOrDefault(x => x.ID == shopID);
            if (shop != null)
            {
                if (!string.IsNullOrEmpty(shop.Theme))
                {
                    this.HttpContext.Items["ShopTheme"] = shop.Theme;
                }
                ViewBag.Shop = shop;
            }
            switch (viewIn)
            {
                case "gallery":
                    return PartialView("_ProductsGallery", data);
                case "table":
                    return PartialView("_ProductsTable", data);
                case "tableItemPartial":
                    return PartialView("_ProductTableItem", data);
            }
            return Content(string.Empty);
        }

        [ShopThemeAttribute("shopID")]
        public ActionResult _GetFirstCategoryProducts(int shopID, bool favorite = false, bool allOrderedProducts = false, bool deals = false)
        {
            List<ProductOverviewModel> data = new List<ProductOverviewModel>();
            List<SpecificationOptionModel> specifications = null;
            if (favorite)
            {
                data.AddRange(LS.SearchProducts(shopID, out specifications, limit: int.MaxValue, favorite: true));
            }
            if (allOrderedProducts)
            {
                data.AddRange(LS.SearchProducts(shopID, out specifications, limit: int.MaxValue, allOrderedProducts: true));
            }
            if (deals)
            {
                data.AddRange(LS.SearchProducts(shopID, out specifications, limit: 100, discountedProducts: true, showDiscounts: true));
            }
            return PartialView("_ProductsGallery", data);
        }

        [HttpPost]
        public void _AddShopRate(int shopID, int score)
        {
            int result = ShoppingService.AddShopRate(shopID, score, LS.CurrentUser.ID);
        }

        [HttpPost]
        public void _AddProductRate(int productID, int score)
        {
            int result = ShoppingService.AddProductRate(productID, score, LS.CurrentUser.ID);
        }

        [HttpPost]
        public ActionResult _AddShopComment(ShopCommentModel comment)//int shopID, string title, string comment, string userName)
        {
            if (comment != null)
            {
                ShopComment c = new ShopComment();
                c.ShopID = comment.ShopID;
                c.UserName = comment.UserName;
                c.Text = comment.Text;
                c.Title = comment.Title;

                ShoppingService.AddShopComment(c);
                return PartialView("_ShopComment", comment);
            }
            return Content(string.Empty);
        }

        [HttpPost]
        public ActionResult _AddProductComment(ProductComment comment)//int shopID, string title, string comment, string userName)
        {
            if (comment != null)
            {
                ProductComment c = new ProductComment();
                c.ProductID = comment.ProductID;
                c.UserName = comment.UserName;
                c.Text = comment.Text;
                c.Title = comment.Title;

                ShoppingService.AddProductComment(c);
                return PartialView("_ProductComment", comment);
            }
            return Content(string.Empty);
        }


        public JsonResult _GetShopTypeList()
        {
            var list = ShoppingService.GetShopTypes();
            return Json(list, JsonRequestBehavior.AllowGet);
        }


        public ActionResult _GetShopInfo(int shopID)
        {
            var culture = new System.Globalization.CultureInfo("he-IL");
            var data = LS.GetFirst<Shop>(x => x.ID == shopID);
            UserActivityService.InsertShopOpen(LS.CurrentUser.ID, shopID
                   , Request.RawUrl,
                   Request.UrlReferrer != null ? Request.UrlReferrer.OriginalString : null
                   , LS.GetUser_IP(Request));
            data.ShopCommentModels = ShoppingService.GetShopComments(shopID);
            //data.ShopType = LS.Get<ShopType>()
            //    .FirstOrDefault(x => data.ShopTypeIDs!= null && data.ShopTypeIDs.Contains(x.ID.ToString() ) );

            var curdate = DateTime.Now.Date;
            var lastdate = curdate.AddDays(7);
            data.WorkTimes = LS.CurrentEntityContext.ShopWorkTimes.Where(x => x.ShopID == shopID && x.Active &&
                (!x.IsSpecial || (x.Date >= curdate && x.Date <= lastdate)))
                .OrderBy(x => x.IsSpecial).ThenBy(x => x.Day).ThenBy(x => x.Date)
                .Select(x => new ShipTimeModel()
                {
                    Date = x.Date,
                    Day = x.Day,
                    TimeFromInt = x.TimeFrom,
                    TimeToInt = x.TimeTo,
                    IsSpecial = x.IsSpecial,

                }).ToList();

            foreach (var t in data.WorkTimes)
            {
                t.DayStr = culture.DateTimeFormat.GetDayName(t.Day);
                t.TimeFromeStr = TimeSpan.FromMinutes(t.TimeFromInt).ToString("hh':'mm"); //ntToTime(t.TimeFromInt);
                t.TimeToStr = TimeSpan.FromMinutes(t.TimeToInt).ToString("hh':'mm");
                t.DateStr = t.Date.ToString("dd/MM");
            }

            data.ShipTimes = _db.ShopShipTimes.Where(x => x.ShopID == shopID && x.Active && !x.IsSpecial)
                .Select(x => new ShipTimeModel()
                {
                    Date = x.Date,
                    Day = x.Day,
                    TimeFromInt = x.TimeFrom,
                    TimeToInt = x.TimeTo,
                }).ToList();
            foreach (var t in data.ShipTimes)
            {
                t.DayStr = t.DayStr = culture.DateTimeFormat.GetDayName(t.Day);
                t.TimeFromeStr = TimeSpan.FromMinutes(t.TimeFromInt).ToString("hh':'mm");
                t.TimeToStr = TimeSpan.FromMinutes(t.TimeToInt).ToString("hh':'mm");
            }

            if (!string.IsNullOrEmpty(data.Theme))
            {
                this.HttpContext.Items["ShopTheme"] = data.Theme;
            }

            return PartialView("_ShopInfoPopup", data);
        }

        public ActionResult ChangeShop(int ID, int ToShopID)
        {
            //if (!LS.isLogined())
            //{
            //    return RedirectToAction("Index","Main");
            //}
            var curShopModel = new ShoppingCartOverviewModel();
            curShopModel.Items = _shoppingCartService.GetShoppingCartItems(ID, true);

            var model = new ShoppingCartOverviewModel();
            model.Items = ShoppingCartService.GetShoppingCartItemsByList(ToShopID, curShopModel.Items);
            var items = model.Items.Where(x => !x.IsNotAvaliable
                       && !x.SelectedAttributeNotAvaliable
                       && !x.IsHaveNotQuantity).ToList();

            foreach (var oi in items)
            {
                if (oi.Quantity > 0)
                    ShoppingCartService.AddToCart(LS.CurrentUser.ID, new ShoppingCartItem()
                    {
                        ProductAttributeOptionID = oi.ProductAttributeOptionID,
                        ProductShopID = oi.ProductShopID,
                        Quantity = oi.Quantity,
                        ShopID = oi.ShopID,

                    });
            }
            Shop shop = _db.Shops.FirstOrDefault(r => r.ID == ToShopID);
            return Redirect("/" + shop.SeoUrl);
        }


        public ActionResult _ChangeShopPopup(int shopID)
        {
            var data = new List<ShoppingCartOverviewModel>();
            var curdate = DateTime.Now.Date;
            var lastdate = curdate.AddDays(7);
            var culture = new System.Globalization.CultureInfo("he-IL");
            ViewBag.CurrentShopID = shopID;
            var curShop = ShoppingService.GetShopByID(shopID);
            var curShopModel = new ShoppingCartOverviewModel();
            curShopModel.Items = _shoppingCartService.GetShoppingCartItems(shopID, true);
            curShopModel.ShopID = shopID;
            curShopModel.Shop = curShop;
            curShopModel.Total = curShopModel.Items.Count > 0 ? curShopModel.Items.Sum(x => x.UnitPrice) : 0;
            curShopModel.TotalStr = ShoppingService.FormatPrice(curShopModel.Total);
            curShopModel.Count = curShopModel.Items.Count;
            //work times
            curShopModel.WorkTimes = _db.ShopWorkTimes.Where(x => x.ShopID == shopID && x.Active &&
                        (!x.IsSpecial || (x.Date >= curdate && x.Date <= lastdate))
                        ).OrderBy(x => x.IsSpecial).ThenBy(x => x.Day).ThenBy(x => x.Date)
                        .Select(x => new ShipTimeModel()
                        {
                            Date = x.Date,
                            Day = x.Day,
                            TimeFromInt = x.TimeFrom,
                            TimeToInt = x.TimeTo,
                            IsSpecial = x.IsSpecial,

                        })
                        .ToList();
            foreach (var t in curShopModel.WorkTimes)
            {
                t.DayStr = culture.DateTimeFormat.GetDayName(t.Day);
                t.TimeFromeStr = TimeSpan.FromMinutes(t.TimeFromInt).ToString("hh':'mm");
                t.TimeToStr = TimeSpan.FromMinutes(t.TimeToInt).ToString("hh':'mm");
                t.DateStr = t.Date.ToString("dd/MM");

            }
            data.Add(curShopModel);
            //missing products


            // find by address
            decimal longitude = 0;
            decimal latitude = 0;
            if (LS.isLogined())
            {
                if (LS.CurrentUser.Latitude != 0)
                {
                    latitude = LS.CurrentUser.Latitude;
                }
                if (LS.CurrentUser.Longitude != 0)
                {
                    longitude = LS.CurrentUser.Longitude;
                }
            }


            //if not regognized
            if (longitude == 0)
            {
                if (Session["longitude"] != null)
                {
                    longitude = (decimal)Session["longitude"];
                }
            }
            if (latitude == 0)
            {
                if (Session["latitude"] != null)
                {
                    latitude = (decimal)Session["latitude"];
                }
            }
            string address = "";
            if (Session["address"] != null)
            {
                address = (string)Session["address"];
            }
            var shops = new List<Shop>();
            if (latitude == 0 || longitude == 0)
            {
                shops = ShoppingService.GetNearestShop(0, latitude, longitude, address, true).ToList();
            }
            else
            {
                shops = ShoppingService.GetNearestShop(0, latitude, longitude, address).ToList();
            }
            string[] ids = new string[] { };
            if (!string.IsNullOrEmpty(curShop.ShopTypeIDs))
            {
                ids = curShop.ShopTypeIDs.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
            }
            Expression<Func<Shop, bool>> predicate = null;
            foreach (var s in ids)
            {
                if (predicate != null)
                {
                    predicate = predicate.MultiSearchOr(x => x.ShopTypeIDs != null
                        && (x.ShopTypeIDs.Contains("," + s + ",")
                        || x.ShopTypeIDs == s
                        || x.ShopTypeIDs.StartsWith(s + ",")
                        || x.ShopTypeIDs.EndsWith("," + s)
                        )
                        );
                }
                else
                {
                    predicate = x => x.ShopTypeIDs != null && (x.ShopTypeIDs.Contains("," + s + ",")
                          || x.ShopTypeIDs == s
                        || x.ShopTypeIDs.StartsWith(s + ",")
                        || x.ShopTypeIDs.EndsWith("," + s)
                        );
                }
            }
            if (predicate != null)
            {
                shops = shops.Where(predicate.Compile()).ToList();
            }

            foreach (var shop in shops)
            {
                if (shop.ID != curShopModel.ShopID)
                {
                    var model = new ShoppingCartOverviewModel();
                    // model.Items = ShoppingCartService.GetShoppingCartItems(shop.ID, true);
                    model.Items = ShoppingCartService.GetShoppingCartItemsByList(shop.ID, curShopModel.Items);
                    model.ShopID = shop.ID;
                    model.Shop = shop;
                    var items = model.Items.Where(x => !x.IsNotAvaliable
                        && !x.SelectedAttributeNotAvaliable
                        && !x.IsHaveNotQuantity).ToList();
                    model.NotAvaliableItems = model.Items.Where(x => x.IsNotAvaliable
                        || x.SelectedAttributeNotAvaliable
                        || x.IsHaveNotQuantity).ToList();
                    model.Total = items.Count > 0 ? items.Sum(x => x.UnitPrice) : 0;
                    model.TotalStr = ShoppingService.FormatPrice(model.Total);
                    model.Count = model.Items.Count;
                    //work times

                    model.WorkTimes = _db.ShopWorkTimes.Where(x => x.ShopID == shopID && x.Active &&
                        (!x.IsSpecial || (x.Date >= curdate && x.Date <= lastdate))
                        ).OrderBy(x => x.IsSpecial).ThenBy(x => x.Day).ThenBy(x => x.Date)
                        .Select(x => new ShipTimeModel()
                        {
                            Date = x.Date,
                            Day = x.Day,
                            TimeFromInt = x.TimeFrom,
                            TimeToInt = x.TimeTo,
                            IsSpecial = x.IsSpecial,

                        })
                        .ToList();
                    foreach (var t in model.WorkTimes)
                    {
                        t.DayStr = culture.DateTimeFormat.GetDayName(t.Day);
                        t.TimeFromeStr = TimeSpan.FromMinutes(t.TimeFromInt).ToString("hh':'mm");
                        t.TimeToStr = TimeSpan.FromMinutes(t.TimeToInt).ToString("hh':'mm");
                        t.DateStr = t.Date.ToString("dd/MM");

                    }

                    data.Add(model);
                }
            }

            if (!string.IsNullOrEmpty(curShop.Theme))
            {
                this.HttpContext.Items["ShopTheme"] = curShop.Theme;
            }

            return PartialView(data);
        }


        public ActionResult _GetSMSPopup()
        {
            return PartialView("_SMSPopup");
        }


        public ActionResult _GetProductPopup(int productShopID)
        {
            List<SpecificationOptionModel> specifications = null;
            var productInfo = GetProductByID(productShopID);

            UserActivityService.InsertProductOpen(LS.CurrentUser.ID, productShopID, productInfo.ProductID
                   , Request.RawUrl,
                   Request.UrlReferrer != null ? Request.UrlReferrer.OriginalString : null
                    , LS.GetUser_IP(Request));


            productInfo.ProductComments = ShoppingService.GetProductComments(productInfo.ProductID).ToList();

            // productInfo.RelatedProducts = LS.SearchProducts(productInfo.ShopID, out specifications, limit: 4).ToList();


            var cart = LS.CurrentEntityContext.ShoppingCartItems.Where(x => x.ShopID == productInfo.ShopID
                && x.UserID == LS.CurrentUser.ID)
                .ToList();

            var curCartItem = cart.FirstOrDefault(x => x.ProductShopID == productInfo.ProductShopID);
            if (curCartItem != null)
            {
                productInfo.isInShoppingCart = true;
                productInfo.QuantityToBuy = curCartItem.Quantity;
                productInfo.QuantityType = curCartItem.QuantityType;
            }

            //save last seen Product
            // if (LS.isLogined())
            //   {
            //      ShoppingService.AddLastSeenProduct(productShopID);
            //  }
            return PartialView("_ProductPopup", productInfo);
        }

        [HttpPost]
        public ActionResult _AddProductToFavorite(int productShopID)
        {
            if (LS.isLogined())
            {
                ShoppingService.AddProductToFavorite(productShopID, LS.CurrentUser.ID);

            }
            return Content(string.Empty);
        }

        [HttpPost]
        public ActionResult _RemoveProductFromFavorite(int productShopID)
        {
            if (LS.isLogined())
            {
                ShoppingService.RemoveProductFromFavorite(productShopID, LS.CurrentUser.ID);
            }
            return Content(string.Empty);
        }

        [HttpPost]
        public string _AddNoteForProduct(int productID, string text)
        {
            if (!string.IsNullOrEmpty(text))
            {
                var data = ShoppingService.AddOrEditNoteForProduct(productID, text);

                return data.Text;
            }
            return string.Empty;
        }


        public ActionResult _GetLoginSignupPopup()
        {
            return PartialView("_LoginSignupPopup");
        }

        public ActionResult _GetForgotPopup()
        {
            return PartialView("_ForgotPopup");
        }


        [HttpPost]
        public JsonResult _GetProductsForAutoComplete(int shopID, string text)
        {
            var data = ShoppingService.GetProductAutocomplete(shopID, text);
            return Json(data);
        }


        public string GetUserNoteForProduct(int productID)
        {
            return ShoppingService.GetUserNoteForProduct(productID, LS.CurrentUser.ID);
        }

        #endregion

        #region Shopping cart
        [HttpPost]
        public ActionResult AddToCartAjx(ShoppingCartItem item, decimal? OverrideQuantity, bool isMobile = false)
        {

            if (!LS.isHaveID())
            {
                return Json(new { result = "error", action = "login", message = "You must login first", data = item });
            }
            var addmodel = ShoppingCartService.AddToCart(LS.CurrentUser.ID, item, OverrideQuantity, isMobile: isMobile);
            if (addmodel.errors.Count > 0)
            {
                return Json(new { result = "error", message = addmodel.errors.FirstOrDefault(), data = addmodel.item });

            }
            return Json(new { result = "ok", data = addmodel.item });
        }
        #endregion

    }
}

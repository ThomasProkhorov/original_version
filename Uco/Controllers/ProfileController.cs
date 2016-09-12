using System.Web.Mvc;
using Uco.Infrastructure;
using Uco.Models;
using System.Linq;
using System.Collections.Generic;
using Uco.Infrastructure.Livecycle;
using Uco.Models.Overview;
using Kendo.Mvc.UI;
using Kendo.Mvc.Extensions;
using Kendo.Mvc;
using Uco.Infrastructure.Repositories;
using System;
using System.Web.Security;
using Uco.Infrastructure.Services;


namespace Uco.Controllers
{


    [Localization]
    public partial class ProfileController : BaseController
    {
        [ChildActionOnly]
        public ActionResult _TopStatus()
        {
            if (!LS.isHaveID())
            {
                return Content("");
            }
            var model = new ProfileStatusLine();

            var startCurDate = DateTime.Now.Date;

            var items = _db.Orders.Where(x => x.UserID == LS.CurrentUser.ID
                && x.CreateOn >= startCurDate)
                //.Select(x=>new {
                //    x.ID,x.ShopID,x.OrderStatus,x.Total,x.TotalCash,x.CreateOn,x.DeliveredOn,x.PayedOn,
                //    x.SentOn
                //})
                .OrderByDescending(x => x.ID)
                .ToList();

            foreach (Order order in items)
            {
                OrderStatusLine line = new OrderStatusLine();
                line.OrderDate = order.CreateOn.ToString("dd/MM HH:mm");
                line.OrderID = order.ID;
                line.OrderTotal = ShoppingService.FormatPrice(order.Total);
                line.StepStatus = (int)order.OrderStatus;
                line.Status = order.OrderStatus.ToString();

                if (order.ShippingMethod == ShippingMethod.Manual)
                {
                    line.OrderEndDate = SF.MoveOrderReadyTimeToWorkHours(order).AddMinutes(30).ToString("dd/MM HH:mm");
                }
                else
                {
                    line.OrderEndDate =
                        order.DeliveredOn.HasValue ? order.DeliveredOn.Value.ToString("HH:mm") :
                            order.SentOn.HasValue ? order.SentOn.Value.AddMinutes(ShoppingService.GetShopByID(order.ShopID).DeliveryTime).ToString("HH:mm") :
                                order.PayedOn.HasValue ? order.PayedOn.Value.AddMinutes(ShoppingService.GetShopByID(order.ShopID).DeliveryTime + 15).ToString("HH:mm") :
                                    order.CreateOn.AddMinutes(ShoppingService.GetShopByID(order.ShopID).DeliveryTime + 30).ToString("HH:mm");
                }

                model.Orders.Add(line);
            }

            model.Orders = items.Select(x => new OrderStatusLine()
            {
                OrderDate = x.CreateOn.ToString("dd/MM HH:mm"),
                OrderEndDate =
                    x.DeliveredOn.HasValue ? x.DeliveredOn.Value.ToString("HH:mm") :
                        x.SentOn.HasValue ? x.SentOn.Value.AddMinutes(ShoppingService.GetShopByID(x.ShopID).DeliveryTime).ToString("HH:mm") :
                            x.PayedOn.HasValue ? x.PayedOn.Value.AddMinutes(ShoppingService.GetShopByID(x.ShopID).DeliveryTime + 15).ToString("HH:mm") :
                                x.CreateOn.AddMinutes(ShoppingService.GetShopByID(x.ShopID).DeliveryTime + 30).ToString("HH:mm")
                ,
                OrderID = x.ID,
                OrderTotal = ShoppingService.FormatPrice(x.Total),
                StepStatus = (int)x.OrderStatus,
                Status = x.OrderStatus.ToString()
            })

            .ToList();

            return View(model);
        }

        [HttpPost]
        public ActionResult _OrderNote(int OrderID, [DataSourceRequest]DataSourceRequest request)
        {
            if (!LS.isHaveID())
            {
                return Json(new { });
            }
            var items = _db.OrderNotes.AsNoTracking().Where(x => x.OrderID == OrderID);

            DataSourceResult result = items.ToDataSourceResult(request);
            return Json(result);
        }

        [HttpPost]
        public ActionResult OrderRegularAjax([DataSourceRequest]DataSourceRequest request)
        {
            if (!LS.isHaveID())
            {
                return Json(new { });
            }
            if (request.Sorts == null)
            {
                request.Sorts = new List<SortDescriptor>();
            }
            //kendo reset manual sort to default =(
            if (request.Sorts.Count == 0)
            {
                request.Sorts.Add(new SortDescriptor("ID",
                    System.ComponentModel.ListSortDirection.Descending));
            }
            var items = _db.Orders.Where(x => x.UserID == LS.CurrentUser.ID && x.RegularOrder);

            DataSourceResult result = items.ToDataSourceResult(request);
            foreach (var item in (IEnumerable<Order>)result.Data)
            {
                item.TotalStr = ShoppingService.FormatPrice(item.Total);
                item.RegularIntervalStr = RP.T("Enums." + item.RegularInterval.ToString()).ToString();
                item.OrderStatusStr = RP.T("Enums." + item.OrderStatus.ToString()).ToString();
            }
            return Json(result);
        }
        [HttpPost]
        public ActionResult OrderAjax([DataSourceRequest]DataSourceRequest request)
        {
            if (!LS.isHaveID())
            {
                return Json(new { });
            }
            //kendo reset manual sort to default =(
            if (request.Sorts == null)
            {
                request.Sorts = new List<SortDescriptor>();
            }
            if (request.Sorts.Count == 0)
            {
                request.Sorts.Add(new SortDescriptor("OrderStatus",
                    System.ComponentModel.ListSortDirection.Ascending));
                request.Sorts.Add(new SortDescriptor("ID",
                    System.ComponentModel.ListSortDirection.Descending));
            }
            var items = _db.Orders.Where(x => x.UserID == LS.CurrentUser.ID && !x.RegularOrder);

            DataSourceResult result = items.ToDataSourceResult(request);
            foreach (var item in (IEnumerable<Order>)result.Data)
            {
                item.TotalStr = ShoppingService.FormatPrice(item.Total);
                item.RegularIntervalStr = RP.T("Enums." + item.RegularInterval.ToString()).ToString();
                item.OrderStatusStr = RP.T("Enums." + item.OrderStatus.ToString()).ToString();
            }
            return Json(result);

        }
        public ActionResult Index()
        {

            UserActivityService.InsertUserClick(LS.CurrentUser.ID,
                Request.RawUrl,
                  Request.UrlReferrer != null ? Request.UrlReferrer.OriginalString : null
                  , LS.GetUser_IP(Request));

            if (!LS.isHaveID())
            {
                return Redirect("~/Account/LogOn");
            }
            var model = LS.CurrentUser;
            return View("Orders", model);
        }
        public ActionResult RegularOrders()
        {
            UserActivityService.InsertUserClick(LS.CurrentUser.ID,
                Request.RawUrl,
                  Request.UrlReferrer != null ? Request.UrlReferrer.OriginalString : null
                  , LS.GetUser_IP(Request));
            if (!LS.isHaveID())
            {
                return Redirect("~/");
            }
            var model = LS.CurrentUser;
            return View(model);

        }

        [HttpPost]
        public ActionResult ReOrder(int ID)
        {
            if (!LS.isHaveID())
            {
                return Json(new
                {
                    result = "error",
                    message = new Dictionary<string, Dictionary<string, List<string>>>() { 
                {"general",new Dictionary<string,List<string>>(){{"errors", new List<string>(){"Please log in"}}}}
                }
                });
            }
            var order = _db.Orders.FirstOrDefault(x => x.ID == ID && x.UserID == LS.CurrentUser.ID);
            if (order != null)
            {


                // add to cart
                var items = _db.OrderItems.Where(x => x.OrderID == order.ID).ToList();
                foreach (var oi in items)
                {
                    if (oi.Quantity > 0)
                        ShoppingCartService.AddToCart(order.UserID, new ShoppingCartItem()
                        {
                            ProductAttributeOptionID = oi.ProductAttributeOptionID,
                            ProductShopID = oi.ProductShopID,
                            Quantity = oi.Quantity,
                            ShopID = order.ShopID,

                        });
                }

                return Json(new
                {
                    result = "ok",
                    url = Url.Action("Index", "ShoppingCart", new { ID = order.ShopID })
                });
            }
            return Json(new
            {
                result = "error",
                message = new Dictionary<string, Dictionary<string, List<string>>>() { 
                {"general",new Dictionary<string,List<string>>(){{"errors", new List<string>(){"order not found"}}}}
                }
            });
        }

        [HttpPost]
        public ActionResult SaveOrder(Order model)
        {
            if (!LS.isHaveID())
            {
                return Json(new
                {
                    result = "error",
                    message = new Dictionary<string, Dictionary<string, List<string>>>() { 
                {"general",new Dictionary<string,List<string>>(){{"errors", new List<string>(){"Please log in"}}}}
                }
                });
            }

            var order = _db.Orders.FirstOrDefault(x => x.ID == model.ID && x.UserID == LS.CurrentUser.ID);
            if (order != null)
            {
                order.Active = model.Active;
                order.ShipTime = model.ShipTime;
                order.RegularInterval = model.RegularInterval;
                _db.SaveChanges();
                return Json(new
                {
                    result = "error",
                    message = new Dictionary<string, Dictionary<string, List<string>>>() { 
                {"general",new Dictionary<string,List<string>>(){{"errors", new List<string>(){"Saved"}}}}
                }
                });
            }
            return Json(new
            {
                result = "error",
                message = new Dictionary<string, Dictionary<string, List<string>>>() { 
                {"general",new Dictionary<string,List<string>>(){{"errors", new List<string>(){"order not found"}}}}
                }
            });
        }
        public ActionResult OrderDetail(int ID)
        {
            UserActivityService.InsertUserClick(LS.CurrentUser.ID,
                Request.RawUrl,
                  Request.UrlReferrer != null ? Request.UrlReferrer.OriginalString : null
                  , LS.GetUser_IP(Request));
            if (!LS.isHaveID())
            {
                return Redirect("~/");
            }
            var order = _db.Orders.FirstOrDefault(x => x.ID == ID && x.UserID == LS.CurrentUser.ID);
            if (order == null)
            {
                TempData["MessageRed"] = "Order doesn`t exists";
                return RedirectToAction("Index");
            }
            order.User = LS.CurrentUser;
            // order.OrderStatus = _db.OrderStatuses.FirstOrDefault(x => x.ID == order.OrderStatusID);

            return View(order);
        }
        public ActionResult ProductAttributeOption_AjaxAutoComplete([DataSourceRequest]DataSourceRequest request
            , int ProductShopID, string text)
        {
            if (!LS.isHaveID())
            {
                return Content("");
            }
            //custom
            var items = (from pao in _db.ProductAttributeOptions
                         //join pa in _db.ProductAttributes
                         //on pao.ProductAttributeID equals pa.ID
                         where pao.ProductShopID == ProductShopID
                         && pao.Name.Contains(text)
                         select new
                         {
                             Name = (pao.Name
                                 //   + ( pao.OverridenPrice > 0 ? " (" + pao.OverridenPrice + ")"  : "" )
                                 ),
                             ID = pao.ID
                         }).AsQueryable();
            DataSourceResult result = items.ToDataSourceResult(request);
            return Json(result);
        }

        public ActionResult ProductShop_AjaxAutoComplete([DataSourceRequest]DataSourceRequest request
            , int ShopID, string text)
        {
            if (!LS.isHaveID())
            {
                return Content("");
            }
            //custom
            var items = (from ps in _db.ProductShopMap
                         join p in _db.Products
                         on ps.ProductID equals p.ID
                         where ps.ShopID == ShopID
                         && p.Name.Contains(text)
                         select new
                         {
                             Name = (p.Name
                                 // +" ("+ps.Price+")"
                                 ),
                             ID = ps.ID
                         }).AsQueryable();
            DataSourceResult result = items.ToDataSourceResult(request);
            return Json(result);

            //redirect to generic auto ready action
            // var generic = new BaseGenericController<ProductShop>();
            // generic._db = this._db;
            //sreturn generic._AjaxAutoComplete(request, text);
        }
        public ActionResult OrderItemAjaxRead([DataSourceRequest]DataSourceRequest request)
        {
            if (!LS.isHaveID())
            {
                return Content("");
            }
            //redirect to generic auto ready action
            return new BaseGenericController<OrderItem>()._AjaxRead(request);
            //deprecated
            //var items = _db.OrderItems.AsQueryable();
            //var result = items.ToDataSourceResult(request);
            //var ProductShopsIDs = ((IEnumerable<OrderItem>)result.Data).Select(x => x.ProductShopID);
            //var ProductAttributesIDs = ((IEnumerable<OrderItem>)result.Data).Select(x => x.ProductAttributeOptionID);
            //var StatusesIDs = ((IEnumerable<OrderItem>)result.Data).Select(x => x.OrderItemStatusID);

            //return Json(result);
        }
        public ActionResult OrderItemAjaxInsert([DataSourceRequest] DataSourceRequest request, OrderItem model)
        {
            if (!LS.isHaveID())
            {
                ModelState.AddModelError("General", "You don't have permissions to edit this element");
                return Json(new[] { model }.ToDataSourceResult(request, ModelState));
            }
            var item = new OrderItem();

            var order = _db.Orders.FirstOrDefault(x => x.ID == model.OrderID);
            if (order != null)
            {
                //check user
                if (order.UserID != LS.CurrentUser.ID)
                {
                    ModelState.AddModelError("General", "You don't have permissions to edit this element");
                    return Json(new[] { model }.ToDataSourceResult(request, ModelState));
                }
                item.OrderID = order.ID;

                decimal newCardTotal = order.Total;

                var productShop = _db.ProductShopMap.FirstOrDefault(x => x.ID == model.ProductShopID);
                string sku = _db.Products.Where(x => x.ID == productShop.ProductID).Select(x => x.SKU).FirstOrDefault();
                string sttrdescr = "";
                if (model.ProductAttributeOptionID > 0)
                {
                    var attr = _db.ProductAttributeOptions.FirstOrDefault(x => x.ID == model.ProductAttributeOptionID);
                    if (attr != null)
                    {
                        sttrdescr = attr.Name;
                        //var attrName = LS.Get<ProductAttribute>().FirstOrDefault(x => x.ID == attr.ProductAttributeID);
                        //if (attrName != null)
                        //{
                        //    sttrdescr = attrName.Name + ": " + sttrdescr;
                        //}
                        if (attr.OverridenPrice.HasValue)
                            productShop.Price = attr.OverridenPrice.Value;
                        if (!string.IsNullOrEmpty(attr.OverridenSku))
                            sku = attr.OverridenSku;
                    }
                }

                item.ProductAttributeOptionID = model.ProductAttributeOptionID;
                item.ProductShopID = model.ProductShopID;
                item.SKU = sku;
                item.Price = productShop.Price;
                item.Quantity = model.Quantity;
                item.UnitPrice = item.Price * item.Quantity;
                item.DiscountAmount = 0;
                item.AttributeDescription = sttrdescr;
                item.OrderItemStatus = OrderItemStatus.New;

                _db.OrderItems.Add(item);
                _db.SaveChanges();
                newCardTotal += item.UnitPrice;
                order.Total = newCardTotal;

                _db.SaveChanges();
                return Json(new[] { item }.ToDataSourceResult(request));



            }


            return Json(new[] { model }.ToDataSourceResult(request, ModelState));
        }
        public ActionResult OrderItemAjaxDelete([DataSourceRequest] DataSourceRequest request, OrderItem model)
        {
            if (!LS.isHaveID())
            {
                ModelState.AddModelError("General", "You don't have permissions to edit this element");
                return Json(new[] { model }.ToDataSourceResult(request, ModelState));
            }
            var item = _db.OrderItems.FirstOrDefault(x => x.ID == model.ID);
            if (item != null)
            {
                var order = _db.Orders.FirstOrDefault(x => x.ID == item.OrderID);
                if (order != null)
                {
                    //check user
                    if (order.UserID != LS.CurrentUser.ID)
                    {
                        ModelState.AddModelError("General", "You don't have permissions to edit this element");
                        return Json(new[] { model }.ToDataSourceResult(request, ModelState));
                    }
                    decimal newCardTotal = order.Total;

                    newCardTotal += -item.UnitPrice;

                    order.Total = newCardTotal;
                    _db.OrderItems.Remove(item);
                    _db.SaveChanges();

                    return Json(new[] { item }.ToDataSourceResult(request, ModelState));
                }


            }


            return Json(new[] { model }.ToDataSourceResult(request, ModelState));
        }
        public ActionResult OrderItemAjaxUpdate([DataSourceRequest] DataSourceRequest request, OrderItem model)
        {
            if (!LS.isHaveID())
            {
                ModelState.AddModelError("General", "You don't have permissions to edit this element");
                return Json(new[] { model }.ToDataSourceResult(request, ModelState));
            }
            var item = _db.OrderItems.FirstOrDefault(x => x.ID == model.ID);
            if (item != null)
            {
                var order = _db.Orders.FirstOrDefault(x => x.ID == item.OrderID);
                if (order != null)
                {
                    //check user
                    if (order.UserID != LS.CurrentUser.ID)
                    {
                        ModelState.AddModelError("General", "You don't have permissions to edit this element");
                        return Json(new[] { model }.ToDataSourceResult(request, ModelState));
                    }
                    decimal newCardTotal = order.Total;

                    newCardTotal += -item.UnitPrice;

                    var productShop = _db.ProductShopMap.FirstOrDefault(x => x.ID == model.ProductShopID);
                    string sku = _db.Products.Where(x => x.ID == productShop.ProductID).Select(x => x.SKU).FirstOrDefault();
                    string sttrdescr = "";
                    if (model.ProductAttributeOptionID > 0)
                    {
                        var attr = _db.ProductAttributeOptions.FirstOrDefault(x => x.ID == model.ProductAttributeOptionID);
                        if (attr != null)
                        {
                            sttrdescr = attr.Name;
                            //  var attrName = LS.Get<ProductAttribute>().FirstOrDefault(x=>x.ID == attr.ProductAttributeID);
                            //if(attrName!=null)
                            //  {
                            //  sttrdescr = attrName.Name+": "+sttrdescr;
                            //  }
                            if (attr.OverridenPrice.HasValue)
                                productShop.Price = attr.OverridenPrice.Value;
                            if (!string.IsNullOrEmpty(attr.OverridenSku))
                                sku = attr.OverridenSku;
                        }
                    }

                    item.ProductAttributeOptionID = model.ProductAttributeOptionID;
                    item.ProductShopID = model.ProductShopID;
                    item.SKU = sku;
                    item.Price = productShop.Price;
                    item.Quantity = model.Quantity;
                    item.UnitPrice = item.Price * item.Quantity;
                    item.DiscountAmount = 0;
                    item.AttributeDescription = sttrdescr;
                    item.OrderItemStatus = OrderItemStatus.New;
                    _db.SaveChanges();
                    newCardTotal += item.UnitPrice;
                    order.Total = newCardTotal;
                    _db.SaveChanges();
                    return Json(new[] { item }.ToDataSourceResult(request));
                }


            }


            return Json(new[] { model }.ToDataSourceResult(request, ModelState));
        }

        public ActionResult Orders()
        {
            UserActivityService.InsertUserClick(LS.CurrentUser.ID,
                Request.RawUrl,
                  Request.UrlReferrer != null ? Request.UrlReferrer.OriginalString : null
                  , LS.GetUser_IP(Request));
            return Index();
            if (!LS.isHaveID())
            {
                return Redirect("~/");
            }
            var model = LS.CurrentUser;
            return View(model);
        }

        private CheckoutData GetCheckoutData(Guid UserID = new Guid())
        {
            if (UserID == Guid.Empty && LS.isHaveID())
            {
                UserID = LS.CurrentUser.ID;
            }
            if (UserID == Guid.Empty)
            {
                return null;
            }
            var checkoutData = _db.CheckoutDatas.FirstOrDefault(x => x.UserID == UserID);
            if (checkoutData == null)
            {
                //add to db if not exists
                checkoutData = new CheckoutData()
                {
                    UserID = LS.CurrentUser.ID,
                    ShipTime = DateTime.Now,

                };
                _db.CheckoutDatas.Add(checkoutData);
                _db.SaveChanges();
            }
            return checkoutData;
        }
        public ActionResult DiscountHistoryAjax([DataSourceRequest]DataSourceRequest request)
        {
            if (!LS.isHaveID())
            {
                return Content("");
            }
            //redirect to generic auto ready action
            return new BaseGenericController<DiscountUsage>()._AjaxRead(request);

        }
        public ActionResult DiscountHistory()
        {
            UserActivityService.InsertUserClick(LS.CurrentUser.ID,
                 Request.RawUrl,
                   Request.UrlReferrer != null ? Request.UrlReferrer.OriginalString : null
                   , LS.GetUser_IP(Request));
            if (!LS.isHaveID())
            {
                return Redirect("~/");
            }
            var model = LS.CurrentUser;
            return View(model);
        }

        public ActionResult Address()
        {
            UserActivityService.InsertUserClick(LS.CurrentUser.ID,
                Request.RawUrl,
                  Request.UrlReferrer != null ? Request.UrlReferrer.OriginalString : null
                  , LS.GetUser_IP(Request));
            if (!LS.isHaveID())
            {
                return Redirect("~/");
            }
            var model = LS.CurrentUser;
            if (string.IsNullOrEmpty(model.AddressMap))
            {
                var checkoutData = GetCheckoutData();
                if (checkoutData.Address != null)
                {
                    model.AddressMap = checkoutData.Address;
                }
            }
            return View(model);
        }

        [HttpPost]
        public ActionResult AddressSave(string AddressMap, decimal Latitude, decimal Longitude)
        {
            if (!LS.isHaveID())
            {
                return Json(new
                {
                    result = "error",
                    message = new Dictionary<string, Dictionary<string, List<string>>>() { 
                {"general",new Dictionary<string,List<string>>(){{"errors", new List<string>(){"Please log in"}}}}
                }
                });
            }
            if (string.IsNullOrEmpty(AddressMap))
            {
                return Json(new
                {
                    result = "error",
                    message = new Dictionary<string, Dictionary<string, List<string>>>() { 
                {"AddressMap",new Dictionary<string,List<string>>(){{"errors", new List<string>(){"Address can`t be empty"}}}}
                }
                });
            }
            var user = _db.Users.FirstOrDefault(x => x.ID == LS.CurrentUser.ID);
            if (user != null)
            {

                user.AddressMap = AddressMap;
                user.Longitude = Longitude;
                user.Latitude = Latitude;
                _db.SaveChanges();
            }
            return Json(new { result = "ok" });
        }

        public ActionResult Settings()
        {
            UserActivityService.InsertUserClick(LS.CurrentUser.ID,
                Request.RawUrl,
                  Request.UrlReferrer != null ? Request.UrlReferrer.OriginalString : null
                  , LS.GetUser_IP(Request));
            if (!LS.isLogined())
            {
                return Redirect("~/");
            }
            if (TempData["ViewData"] != null)
            {
                ViewData = (ViewDataDictionary)TempData["ViewData"];
            }
            var model = new UserModel();
            model.CompanyName = LS.CurrentUser.CompanyName;
            model.Email = LS.CurrentUser.Email;
            model.FirstName = LS.CurrentUser.FirstName;
            model.LastName = LS.CurrentUser.LastName;
            model.Phone = LS.CurrentUser.Phone;
            model.NewsLetter = _db.Newsletters.Any(x => x.NewsletterEmail == model.Email);
            return View(model);
        }
        [HttpPost]
        public ActionResult Settings(UserModel model)
        {
            if (!LS.isLogined())
            {
                return Redirect("~/");
            }
            if (ModelState.IsValid)
            {
                var user = _db.Users.FirstOrDefault(x => x.ID == LS.CurrentUser.ID);
                if (user != null)
                {
                    user.FirstName = model.FirstName;
                    user.LastName = model.LastName;
                    user.CompanyName = model.CompanyName;
                    user.Phone = model.Phone;
                    user.Email = model.Email;
                    if (model.NewsLetter)
                    {
                        var nl = _db.Newsletters.FirstOrDefault(x => x.NewsletterEmail == model.Email);
                        if (nl == null)
                        {
                            Newsletter n = new Newsletter()
                            {
                                NewsletterAccept = true,
                                NewsletterDate = DateTime.UtcNow,
                                NewsletterEmail = user.Email,
                                NewsletterName = user.FirstName + " " + user.LastName,
                                RoleDefault = "Register"
                            };
                            _db.Newsletters.Add(n);
                            _db.SaveChanges();
                            SF.AddToNewsletter(n);
                        }
                    }
                    else
                    {
                        var nl = _db.Newsletters.FirstOrDefault(x => x.NewsletterEmail == model.Email);
                        if (nl != null)
                        {

                            _db.Newsletters.Remove(nl);
                            _db.SaveChanges();
                        }
                    }
                    _db.SaveChanges();
                    TempData["SuccessMessage"] = "Save Successful";
                    return RedirectToAction("Settings");
                }
            }

            TempData["ViewData"] = ViewData;
            return RedirectToAction("Settings");
        }



        [Authorize]
        public ActionResult Password()
        {
            UserActivityService.InsertUserClick(LS.CurrentUser.ID,
                Request.RawUrl,
                  Request.UrlReferrer != null ? Request.UrlReferrer.OriginalString : null
                   , LS.GetUser_IP(Request));
            return View();
        }

        [Authorize]
        [HttpPost]
        public ActionResult Password(ChangePasswordModel model)
        {
            if (ModelState.IsValid)
            {

                // ChangePassword will throw an exception rather
                // than return false in certain failure scenarios.
                bool changePasswordSucceeded;
                try
                {
                    MembershipUser currentUser = Membership.GetUser(User.Identity.Name, true /* userIsOnline */);
                    changePasswordSucceeded = currentUser.ChangePassword(model.OldPassword, model.NewPassword);
                }
                catch (Exception)
                {
                    changePasswordSucceeded = false;
                }

                if (changePasswordSucceeded)
                {
                    TempData["SuccessMessage"] = "Save Successful";
                    return RedirectToAction("Password");
                }
                else
                {
                    TempData["SuccessMessage"] = "The current password is incorrect or the new password is invalid.";
                    ModelState.AddModelError("OldPassword", "The current password is incorrect or the new password is invalid.");
                }
            }
            TempData["ViewData"] = ViewData;
            // If we got this far, something failed, redisplay form
            return RedirectToAction("Password");
        }
    }
}

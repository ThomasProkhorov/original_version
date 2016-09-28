using System.Web.Mvc;
using Uco.Infrastructure;
using Uco.Models;
using System.Linq;
using System.Collections.Generic;
using Uco.Infrastructure.Livecycle;
using Uco.Models.Overview;
using System;
using Uco.Infrastructure.Services;
using System.Net;
using System.Text;
using System.IO;
using System.Globalization;
using System.Data.Entity;
using Uco.Infrastructure.Repositories;
using Uco.Infrastructure.Discounts;
using System.Xml;

namespace Uco.Controllers
{
    [Localization]   
    public partial class CheckoutController : BaseController
    {
        private ShoppingCartService _shoppingCartService;

        public CheckoutController()
        {
            _shoppingCartService = new ShoppingCartService(_db);
        }

        #region Shopping Cart

        [HttpPost]
        public ActionResult ApplyCode(int ID = 0, string code = null)
        {
            if (!LS.isHaveID())
            {
                return Json(new { result = "ok", text = RP.S("Checkout.ApplyCode.NotLoggined") });
            }
            if (ID == 0)
            {
                if (LS.CurrentHttpContext.Session["ShopID"] != null)
                {
                    ID = (int)LS.CurrentHttpContext.Session["ShopID"];

                    if (ID > 0)
                        return Json(new { result = "ok", text = RP.S("Checkout.ApplyCode.WrongShop") });
                }
                return Json(new { result = "ok", text = RP.S("Checkout.ApplyCode.WrongShop") });
            }
            var checkoutData = ShoppingCartService.GetCheckoutData();

            checkoutData.CouponCode = code;
            checkoutData.LastAction = DateTime.UtcNow;
            _db.SaveChanges();
            var cart = _shoppingCartService.GetShoppingCartModel(ID, loadattributes: false, withship: true, loadworktimes: true
                , checkQuantity: true);
            if (!string.IsNullOrEmpty(cart.DiscountByCouponeCodeText))
            {
                return Json(new { result = "ok", text = cart.DiscountByCouponeCodeText,total=cart.TotalStr});
            }
            return Json(new { result = "ok", text = RP.S("Checkout.ApplyCode.DiscountCodeCantBeApplied"), total = cart.TotalStr });
        }

        [SSLrequired(MobileOnly = false)]
        public ActionResult Index(int ID = 0)
        {
            if (!LS.isLogined())
            {
                return RedirectToAction("Index", "ShoppingCart", new { ID = ID });
            }
            //if (ID == 0)
            //{
            //    ID = ShoppingCartService.GetFirstShopID();
            //    if (ID > 0)
            //    {
            //        return RedirectToAction("Index", new { ID = ID }); //if not than cart is empty
            //    }
            //}
            if (ID == 0)
            {
                if (LS.CurrentHttpContext.Session["ShopID"] != null)
                {
                    ID = (int)LS.CurrentHttpContext.Session["ShopID"];

                    if (ID > 0)
                        return RedirectToAction("Index", new { ID = ID });
                }
            }

            ViewBag.MessageRed = TempData["MessageRed"];
            ViewBag.MessageYellow = TempData["MessageYellow"];
            ViewBag.MessageGreen = TempData["MessageGreen"];
            if (TempData["ViewData"] != null)
            {
                ViewData = (ViewDataDictionary)TempData["ViewData"];
            }
            var model = new CheckoutModel();
            model.Cart = _shoppingCartService.GetShoppingCartModel(ID, loadattributes: false, withship: true, loadworktimes: true
                , checkQuantity: true);
            if (model.Cart.Items.Any(x => !x.IsValid)) //not valid, not quantity or etc
            {
                return RedirectToAction("Index", "ShoppingCart", new { ID = ID });
            }
            if (model.Cart.Count == 0)
            {
                return RedirectToAction("Index", "ShoppingCart", new { ID = ID });
            }
            var checkoutData = ShoppingCartService.GetCheckoutData();

            if (Session["address"] != null)
            {
                model.Address = (string)Session["address"];
            }
            else
            {
                model.Address = checkoutData.Address;

            }
            model.ShopID = ID;
            model.Email = checkoutData.Email;
            model.FullName = LS.CurrentUser.FirstName + " " + LS.CurrentUser.LastName;
            model.CompanyName = LS.CurrentUser.FirstName + " " + LS.CurrentUser.LastName;
            model.Note = checkoutData.Note;
            model.RegularOrder = checkoutData.RegularOrder;
            model.ShippOn = checkoutData.ShippOn;
            model.ShipTime = checkoutData.ShipTime;
            model.CouponCode = checkoutData.CouponCode;
            model.PaymentMethod = checkoutData.PaymentMethod;
            model.Phone = checkoutData.Phone;
            model.ShippingMethod = checkoutData.ShippingMethod;

            LS.CurrentHttpContext.Session["ShopID"] = model.ShopID;

            //calculate UserCredits
            #region UserCredits
            var credits = _db.UserCredits.Where(x =>
                (x.UserID == LS.CurrentUser.ID && x.ShopID == ID)
                || (x.UserID == LS.CurrentUser.ID && x.ShopID == 0)
                ).OrderByDescending(x => x.ShopID).ToList();
            decimal totalCredits = 0;
            foreach (var credit in credits)
            {

                if (credit.Value > 0)
                {
                    totalCredits += credit.Value;
                }
            }
            model.Cart.TotalCredits = totalCredits;
            #endregion

            //_db.ShoppingCartItems.Where(x => x.UserID == LS.CurrentUser.ID).ToList();

            if (!string.IsNullOrEmpty(model.Cart.Shop.Theme))
            {
                this.HttpContext.Items["ShopTheme"] = model.Cart.Shop.Theme;
            }

            return View(model);
        }

        public ActionResult _AjaxGetDiscountAmount(string code)
        {
            int shopID = (int)LS.CurrentHttpContext.Session["ShopID"];
            var overview = _shoppingCartService.GetShoppingCartModel(shopID, loadattributes: false, withship: true, loadworktimes: true, checkQuantity: true);
            var discountService = new DiscountService(_db);
            //discountService.ProcessItems(overview, LS.CurrentUser);
            //discountService.ProcessTotals(overview, LS.CurrentUser);

            var discountValue = discountService.CalculateDiscountByCode(code, overview);

            string message = string.Format("{0}{1:C2}: הנחה", discountValue.InPercents ? "%" : "", discountValue.Amount);

            return Json(new { text = message }, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public ActionResult Index(int ID, CheckoutModel model)
        {
            if (!LS.isLogined())
            {
                return RedirectToAction("Index", "ShoppingCart", new { ID = ID });
            }
            if (ID == 0)
            {
                ID = ShoppingCartService.GetFirstShopID();
                if (ID > 0)
                {
                    return RedirectToAction("Index", new { ID = ID }); //if not than cart is empty
                }
            }
            if (ModelState.IsValid)
            {

                var checkoutData = ShoppingCartService.GetCheckoutData();
                checkoutData.Address = model.Address;
                checkoutData.Email = model.Email;
                checkoutData.FullName = model.FullName;
                checkoutData.CompanyName = model.CompanyName;
                checkoutData.Note = model.Note;
                checkoutData.RegularOrder = model.RegularOrder;
                checkoutData.ShippOn = model.ShippOn;
                checkoutData.LastAction = DateTime.UtcNow;

                checkoutData.ShipTime = model.ShipTime;

                checkoutData.PaymentMethod = model.PaymentMethod;
                checkoutData.Phone = model.Phone;
                User u = _db.Users.FirstOrDefault(r => r.ID == LS.CurrentUser.ID);
                u.Phone = model.Phone;
                if (!string.IsNullOrEmpty(model.FullName))
                {
                    string[] flnames = model.FullName.Split(new char[] { ' ' }, 2);
                    if (flnames.Length > 1)
                    {
                        u.FirstName = flnames[0];
                        u.LastName = flnames[1];
                    }

                }
                Session["address"] = checkoutData.Address;
                checkoutData.ShippingMethod = model.ShippingMethod;
                checkoutData.CouponCode = model.CouponCode;
                checkoutData.IsApproved = true;//set for confirmation, after order created need set to false (for prevent checkout step missing)
                _db.SaveChanges();

                model.Cart = _shoppingCartService.GetShoppingCartModel(ID, loadattributes: false
                    , withship: true, loadworktimes: true, nocache: true
                    , checkQuantity: true);
                if (model.Cart.Items.Any(x => !x.IsValid)) //not valid, not quantity or etc
                {
                    return RedirectToAction("Index", "ShoppingCart", new { ID = ID });
                }
                if (model.Cart.Count == 0)
                {
                    return RedirectToAction("Index", "ShoppingCart", new { ID = ID });
                }
                if (!model.ShippOn || model.ShipTime < DateTime.Now.AddMinutes(-10))
                {
                    model.ShipTime = DateTime.Now;
                }
                //check ship time
                bool validTime = true;
                if (!model.Cart.IsShipEnabled)
                {
                    model.ShippingMethod = ShippingMethod.Manual;
                }
                if (model.ShippingMethod == ShippingMethod.Courier)
                {
                    validTime = false;
                    var curDate = model.ShipTime.Date;
                    var time = model.Cart.ShipTimes.FirstOrDefault(
                        x => (x.IsSpecial && x.Date.Date == curDate)
                            ||
                            (!x.IsSpecial && x.Day == curDate.DayOfWeek)

                        );
                    if (time != null)
                    {
                        int curIntTime = model.ShipTime.Hour * 60 + model.ShipTime.Minute;
                        if (time.TimeFromInt <= curIntTime && curIntTime <= time.TimeToInt)
                        {
                            validTime = true;
                        }
                    }
                    if (!validTime)
                    {
                        ModelState.AddModelError("ShipTime", RP.T("Checkout.Validation.ShipNotPossiblde").ToString());
                    }
                }
                if (!model.Cart.IsShipEnabled && !model.Cart.InStorePickUpEnabled)
                {
                    validTime = false;
                    ModelState.AddModelError("ShipTime", RP.T("Checkout.Validation.ShippingMethodNotAvaliable").ToString());
                }

                if (validTime)
                {
                    var shop = model.Cart.Shop;
                    // save and return

                    //create order
                    var order = new Order();
                    order.UID = Guid.NewGuid();
                    order.CreateOn = DateTime.Now;
                    order.OrderStatus = OrderStatus.New; //ShoppingService.NewOrderStatus();
                    order.UserID = LS.CurrentUser.ID;
                    order.Email = LS.CurrentUser.Email;
                    checkoutData.Email = order.Email;
                    order.Address = model.Address;
                    //  order.Email = model.Email;
                    order.FullName = model.FullName;
                    order.CompanyName = model.CompanyName;
                    order.Note = model.Note;
                    order.RegularOrder = false;
                    order.RegularInterval = model.RegularInterval;
                    //set if order is regular
                    if (order.RegularInterval != RegularInterval.NotRegular)
                    {
                        order.RegularOrder = true;
                        order.Active = true;
                    }
                    order.ShippOn = model.ShippOn;
                    order.ShipTime = model.ShipTime;
                    order.PaymentMethod = model.PaymentMethod;
                    order.Phone = model.Phone;
                    order.ShippingMethod = model.ShippingMethod;
                    order.CouponCode = model.CouponCode;
                    order.ShipAddress = model.Address;
                    order.ShipCost = model.Cart.ShippingCost;
                    order.TotalDiscountAmount = model.Cart.TotalDiscount;
                    order.TotalDiscountDescription = model.Cart.DiscountDescription;
                    order.SubTotal = model.Cart.TotalWithoutShip;

                    if (model.ShippingMethod == ShippingMethod.Manual)
                    {
                        model.Cart.Total = model.Cart.Total - model.Cart.ShippingCost;
                    }

                    order.ShopID = model.Cart.Items.Select(x => x.ShopID).FirstOrDefault();
                    //calculate UserCredits
                    #region UserCredits
                    var credits = _db.UserCredits.Where(x =>
                        (x.UserID == u.ID && x.ShopID == order.ShopID)
                        || (x.UserID == u.ID && x.ShopID == 0)
                        ).OrderByDescending(x => x.ShopID).ToList();
                    foreach (var credit in credits)
                    {
                        //validate credit
                        if (credit.Value <= 0)
                        {
                            _db.UserCredits.Remove(credit);
                            _db.SaveChanges();
                            continue;
                        }
                        if (credit.Value > model.Cart.Total)
                        {
                            var val = model.Cart.Total;
                            credit.Value -= model.Cart.Total;
                            model.Cart.Total = 0;

                            _db.SaveChanges();
                            if (string.IsNullOrEmpty(order.MessageToOwner))
                            {
                                order.MessageToOwner = "Used user credits: " + ShoppingService.FormatPrice(val);
                            }
                            else
                            {
                                order.MessageToOwner += "\nUsed user credits: " + ShoppingService.FormatPrice(val);
                            }
                            break;
                        }
                        else
                        {
                            var val = credit.Value;
                            model.Cart.Total = model.Cart.Total - val;
                            credit.Value -= val;
                            if (credit.Value <= 0)
                            {
                                _db.UserCredits.Remove(credit);
                            }
                            _db.SaveChanges();
                            if (string.IsNullOrEmpty(order.MessageToOwner))
                            {
                                order.MessageToOwner = "Used user credits: " + ShoppingService.FormatPrice(val);
                            }
                            else
                            {
                                order.MessageToOwner += "\nUsed user credits: " + ShoppingService.FormatPrice(val);
                            }
                            if (model.Cart.Total <= 0)
                            {
                                break;
                            }
                        }
                    }
                    #endregion
                    if (model.PaymentMethod == PaymentMethod.Credit || model.PaymentMethod == PaymentMethod.CreditShopOwner)
                    {

                        order.PaymentToShopOwner = model.Cart.Total;
                        if (shop.IsToShopOwnerCredit)
                        {
                            model.PaymentMethod = PaymentMethod.CreditShopOwner;
                            order.PaymentMethod = PaymentMethod.CreditShopOwner;
                        }
                    }
                    order.Total = model.Cart.Total;



                    order.Fee = 0;
                    if (model.Cart.IsLessMemberFee || u.ShopID == order.ShopID)
                    {
                        order.LessFee = true;
                    }
                    if (model.Cart.IsLessMemberFee)
                    {
                        u.ShopID = order.ShopID;
                    }
                    _db.Orders.Add(order);
                    _db.SaveChanges();
                    //insert activity
                    UserActivityService.InsertOrder(order
                        , Request.RawUrl
                        , Request.UrlReferrer != null ? Request.UrlReferrer.OriginalString : null
                         , LS.GetUser_IP(Request));
                    //create order items
                    System.Web.Script.Serialization.JavaScriptSerializer js = new System.Web.Script.Serialization.JavaScriptSerializer();
                    var usageModel = new DiscountUsageModel()
                    {
                        Address = checkoutData.Address,
                        Email = checkoutData.Email,
                        FullName = checkoutData.FullName,
                        Phone = checkoutData.Phone
                    };
                    var json = js.Serialize(usageModel);
                    foreach (var ci in model.Cart.Items)
                    {
                        var item = new OrderItem();
                        item.OrderID = order.ID;
                        item.OrderItemStatus = OrderItemStatus.New;
                        item.Price = ci.Price;
                        item.ProductAttributeOptionID = ci.ProductAttributeOptionID;
                        item.ProductShopID = ci.ProductShopID;
                        item.Quantity = ci.Quantity;
                        item.QuantityType = ci.QuantityPriceType;
                        item.MeasureUnit = ci.MeasureUnit;
                        item.MeasureUnitStep = ci.MeasureUnitStep;
                        item.UnitPrice = ci.UnitPrice;
                        item.AttributeDescription = ci.AttributeDescription;
                        item.SKU = ci.SKU;
                        item.DiscountAmount = ci.TotalDiscountAmount;
                        item.DiscountDescription = ci.DiscountDescription;
                        _db.OrderItems.Add(item);
                        //adding usage disounts
                        foreach (var dID in ci.DiscountIDs)
                        {
                            var usageHistory = new DiscountUsage()
                            {
                                DiscountID = dID,
                                OrderID = order.ID,
                                ShopID = order.ShopID,
                                UsedTimes = 1,
                                UserID = order.UserID,
                                UsageData = json
                            };
                            _db.DiscountUsages.Add(usageHistory);
                            //clean usage cache for next usage
                            DiscountService.ClearHistoryCache(dID, order.UserID);
                        }
                    }
                    //adding total using discounts

                    foreach (var dID in model.Cart.DiscountIDs)
                    {

                        var usageHistory = new DiscountUsage()
                        {
                            DiscountID = dID,
                            OrderID = order.ID,
                            ShopID = order.ShopID,
                            UsedTimes = 1,
                            UserID = order.UserID,
                            UsageData = json
                        };
                        _db.DiscountUsages.Add(usageHistory);
                        //clean usage cache for next usage
                        DiscountService.ClearHistoryCache(dID, order.UserID);
                    }
                    _db.SaveChanges();

                    //send mail and sms
                    if (model.PaymentMethod != PaymentMethod.Credit
                        && model.PaymentMethod != PaymentMethod.CreditShopOwner)
                        //&& (model.PaymentMethod != PaymentMethod.Cash))// || u.ApprovedBySms))
                    {
                        var messService = new MessageService(_db);
                        messService.SendNewOrderSMSToMember(order, shop);
                        messService.SendNewOrderEmailToMember(order, shop);
                        messService.SendNewOrderEmailToUser(order, shop);
                        messService.SendNewOrderSMSToUser(order, shop, u);
                    }
                    //clear shopping cart

                    foreach (var scim in model.Cart.Items)
                    {
                        //need change quantity
                        if (scim.QuantityType != ProductQuantityType.NotCheck)
                        {
                            if (scim.QuantityType == ProductQuantityType.CheckByProduct)
                            {
                                var pshop = _db.ProductShopMap.FirstOrDefault(x => x.ID == scim.ProductShopID);
                                if (pshop != null)
                                {
                                    pshop.Quantity -= scim.Quantity;
                                }
                                _db.SaveChanges();
                            }
                            if (scim.QuantityType == ProductQuantityType.CheckByProductOptions)
                            {
                                var poption = _db.ProductAttributeOptions.FirstOrDefault(x => x.ID == scim.ProductAttributeOptionID);
                                if (poption != null)
                                {
                                    poption.Quantity -= scim.Quantity;
                                }
                                _db.SaveChanges();
                            }
                        }
                        //and remove
                        var i = new ShoppingCartItem()
                        {
                            ID = scim.ID,
                        };
                        _db.ShoppingCartItems.Attach(i);
                        _db.ShoppingCartItems.Remove(i);
                    }

                    _db.SaveChanges();

                    return RedirectToAction("Pay", new { OrderID = order.ID });
                }
                else
                {
                    checkoutData = ShoppingCartService.GetCheckoutData();
                    checkoutData.ShipTime = model.ShipTime;
                    checkoutData.ShippOn = true;
                    checkoutData.LastAction = DateTime.UtcNow;
                    _db.SaveChanges();
                }
            }
            TempData["ViewData"] = ViewData;
            model.Cart = _shoppingCartService.GetShoppingCartModel(ID, loadattributes: false, withship: true, loadworktimes: true
               , checkQuantity: true);
            model.ShopID = ID;


            LS.CurrentHttpContext.Session["ShopID"] = model.ShopID;

            //calculate UserCredits
            #region UserCredits
            var credits2 = _db.UserCredits.Where(x =>
                (x.UserID == LS.CurrentUser.ID && x.ShopID == ID)
                || (x.UserID == LS.CurrentUser.ID && x.ShopID == 0)
                ).OrderByDescending(x => x.ShopID).ToList();
            decimal totalCredits = 0;
            foreach (var credit in credits2)
            {

                if (credit.Value > 0)
                {
                    totalCredits += credit.Value;
                }
            }
            model.Cart.TotalCredits = totalCredits;
            #endregion

            if (!string.IsNullOrEmpty(model.Cart.Shop.Theme))
            {
                this.HttpContext.Items["ShopTheme"] = model.Cart.Shop.Theme;
            }

            return View(model);

        }
        [SSLrequired(MobileOnly = false)]
        public ActionResult Pay(int? OrderID)
        {
            if (!LS.isLogined())
            {
                return Redirect("~/");
            }
            var checkoutData = ShoppingCartService.GetCheckoutData();
            if (!checkoutData.IsApproved) //user missed
            {
                if (OrderID.HasValue)
                {
                    return RedirectToAction("Index", new { ID = OrderID.Value });
                }
                return RedirectToAction("Index");
            }
            Order model = null;
            if (OrderID.HasValue)
            {
                model = _db.Orders.FirstOrDefault(x => x.ID == OrderID.Value
                    && x.UserID == LS.CurrentUser.ID
                    && x.OrderStatus == OrderStatus.New);
            }
            else
            {
                model = _db.Orders.FirstOrDefault(x => x.UserID == LS.CurrentUser.ID && x.OrderStatus == OrderStatus.New);
            }
            if (model == null)
            {
                if (OrderID.HasValue)
                {
                    return RedirectToAction("Index", "ShoppingCart", new { ID = OrderID.Value });
                }
                return RedirectToAction("Index", "ShoppingCart");
            }
            model.OrderItems = _db.OrderItems.Where(x => x.OrderID == model.ID).ToList();
            var shop = LS.GetFirst<Shop>(x => x.ID == model.ShopID);
            if (shop != null)
            {
                if (!string.IsNullOrEmpty(shop.Theme))
                {
                    this.HttpContext.Items["ShopTheme"] = shop.Theme;
                }
            }
            return View(model);
        }

        #endregion

        #region sms gateway

        public ActionResult SendSms(string phone)
        {
            if (string.IsNullOrEmpty(phone))
            {
                return Json(new
                {
                    result = "error",
                    message = new Dictionary<string, Dictionary<string, List<string>>>() { 
                {"phone",new Dictionary<string,List<string>>(){ { "errors", new List<string>() { RP.T("CheckoutController.Error.PhoneInvalid").ToString() } } } }
                }
                });
            }
            //generate code
            var chars = "0123456789";
            var random = new Random();
            var result = new string(
                Enumerable.Repeat(chars, 6)
                          .Select(s => s[random.Next(s.Length)])
                          .ToArray());
            var checkoutData = ShoppingCartService.GetCheckoutData();
            checkoutData.smsCode = result;
            checkoutData.smsCodeUsed = false;
            checkoutData.Phone = phone;
            checkoutData.LastAction = DateTime.UtcNow;
            User u = _db.Users.FirstOrDefault(r => r.ID == LS.CurrentUser.ID);
            u.Phone = phone;
            _db.SaveChanges();
            //send sms to phone
            var messService = new MessageService(_db);
            messService.CheckoutSmsConfirmSmsToUser(checkoutData, LS.CurrentUser);

            return Json(new
                {
                    result = "error",
                    message = new Dictionary<string, Dictionary<string, List<string>>>() { 
                {"phone",new Dictionary<string,List<string>>(){{"errors", new List<string>(){ RP.T("CheckoutController.Info.SmsSended").ToString() }}}}
                }
                });
        }

        public ActionResult CheckSmsCode(int OrderID, string smscode)
        {
            if (string.IsNullOrEmpty(smscode))
            {
                return Json(new
                {
                    result = "error",
                    message = new Dictionary<string, Dictionary<string, List<string>>>() { 
                {"smscode",new Dictionary<string,List<string>>(){{"errors", new List<string>(){ RP.T("CheckoutController.Error.SmsCodeInvalid").ToString() }}}}
                }
                });
            }

            var checkoutData = ShoppingCartService.GetCheckoutData();
            if (checkoutData.smsCode != smscode.Trim())
            {
                return Json(new
                {
                    result = "error",
                    message = new Dictionary<string, Dictionary<string, List<string>>>() { 
                {"smscode",new Dictionary<string,List<string>>(){{"errors", new List<string>(){ RP.T("CheckoutController.Error.SmsCodeInvalid").ToString() }}}}
                }
                });
            }
            var user = _db.Users.FirstOrDefault(x => x.ID == LS.CurrentUser.ID);
            if (user != null)
            {
                user.ApprovedBySms = true;
                _db.SaveChanges();

                var order = _db.Orders.FirstOrDefault(x => x.ID == OrderID);
                if (order != null)
                {
                    var shop = LS.Get<Shop>().FirstOrDefault(x => x.ID == order.ShopID);
                    if (shop != null)
                    {
                        var messService = new MessageService(_db);
                        messService.SendNewOrderSMSToMember(order, shop);
                        messService.SendNewOrderEmailToMember(order, shop);
                        messService.SendNewOrderEmailToUser(order, shop);
                        messService.SendNewOrderSMSToUser(order, shop, user);
                    }
                }

            }
            //check sms code
            return Json(new { result = "ok" });
        }

        #endregion

        #region ClubCard

        public ActionResult _PayClubCard(int ID)
        {
            Order o = _db.Orders.FirstOrDefault(r => r.ID == ID);
            if (o == null) return Content("Can't find order");

            return View(o);

        }

        [HttpPost]
        public ActionResult _ClubCardSubmit(int ID, string CardType, string CardNumber, string CardID)
        {
            var message = new Dictionary<string, jsonresult>();
            Order o = _db.Orders.FirstOrDefault(r => r.ID == ID);
            if (o == null)
            {
                message.Add("general", new jsonresult() { errors = new List<string>() { "Can`t find order" } });
                return Json(new { result = "error", message = message });
            }
            if (!string.IsNullOrEmpty(CardType) && !string.IsNullOrEmpty(CardNumber) && !string.IsNullOrEmpty(CardID))
            {
                var clubOrderInfo = _db.OrderNotes.FirstOrDefault(x => x.OrderID == ID && x.Field == "ClubCard");
                if (clubOrderInfo != null)
                {
                    message.Add("general", new jsonresult() { errors = new List<string>() { "Order already processed" } });
                    return Json(new { result = "error", message = message });
                }
                clubOrderInfo = new OrderNote()
                {
                    Field = "ClubCard",
                    OrderID = ID,
                    Note = "CardType: " + CardType + ", CardNumber: " + CardNumber + ", CardID: " + CardID,
                    CreateDate = DateTime.Now
                };
                _db.OrderNotes.Add(clubOrderInfo);
                _db.SaveChanges();
                var shop = ShoppingService.GetShopByID(o.ShopID);
                var messService = new MessageService(_db);
                messService.SendOrderPayedEmailToMember(o, shop);
                messService.SendOrderPayedEmailToUser(o, shop);
                return Json(new { result = "ok" });
            }
            message.Add("general", new jsonresult() { errors = new List<string>() { "Data issue" } });
            return Json(new { result = "error", message = message });
        }
        public class jsonresult
        {
            public List<string> errors { get; set; }
        }
        #endregion

        #region CreditGuard
        private void LogCreditTestData(string LogText)
        {
            System.IO.File.AppendAllText(Server.MapPath("~/App_Data/credittestlog.txt"), string.Format("{0} IP:{2} - {1}\r\n", DateTime.Now, LogText, Request.ServerVariables["LOCAL_ADDR"]));
        }
        public ActionResult _PayCreditSecondPhase(int ID)
        {
            Order o = _db.Orders.FirstOrDefault(r => r.ID == ID);
            if (o == null) return Content("Can't find order");


            //Check if already payed
            //if (o.OrderStatus == 0) return Content("Already payed");
            var shop = ShoppingService.GetShopByID(o.ShopID);
            if (shop == null) return Content("Shop doesn`t exist");
            string CreditGuardUser = shop.CreditGuardUser;
            string CreditGuardPass = shop.CreditGuardPass;
            string CreditGuardTerminal = shop.CreditGuardTerminal;
            string CreditGuardMid = shop.CreditGuardMid;
            string CreditGuardUrl = shop.CreditGuardUrl;

            string returnUrl = "https://" + Request.ServerVariables["HTTP_HOST"] + "/Checkout/PayCreditTestGuardOK";
            string cancelReturnUrl = "https://" + Request.ServerVariables["HTTP_HOST"] + "/Checkout/PayCreditGuardNotOK";

            // this is what we are sending
            string xml_data = "<ashrait>" +
                    "<request>" +
                        "<version>1001</version>" +
                        "<language>HEB</language>" +
                        "<dateTime/>" +
                        "<command>doDeal</command>" +
                        "<requestid/>" +
                            "<doDeal>" +
                                "<terminalNumber>" + CreditGuardTerminal + "</terminalNumber>" +
                                "<cardId>" + o.ApprovedToken + "</cardId>" +
                                "<total>" + Math.Round(o.Total * 100, 0).ToString("0", CultureInfo.InvariantCulture) + "</total>" +
                                "<transactionType>Debit</transactionType>" +
                                "<creditType>RegularCredit</creditType>" +
                                "<currency>ILS</currency>" +
                                "<transactionCode>Phone</transactionCode>" +
                                "<validation>AutoComm</validation>" +
                                "<cardExpiration>" + o.ApprovedGuid + "</cardExpiration>" +
                                "<user>" + CreditGuardUser + "</user>" +

                            "</doDeal>" +
                        "</request>" +
                    "</ashrait>";

            string post_data = "user=" + CreditGuardUser + "&password=" + CreditGuardPass + "&int_in=" + xml_data;
            // this is where we will send it
            string uri = CreditGuardUrl;
            //"https://cguat2.creditguard.co.il/xpo/Relay";

            // create a request
            HttpWebRequest request = (HttpWebRequest)
            WebRequest.Create(uri);
            request.KeepAlive = false;
            request.ProtocolVersion = HttpVersion.Version10;
            request.Method = "POST";

            // turn our request string into a byte stream
            //byte[] postBytes = Encoding.ASCII.GetBytes(post_data);
            LogCreditTestData("sent phase2: " + post_data);
            byte[] postBytes = Encoding.GetEncoding("UTF-8").GetBytes(post_data);

            // this is important - make sure you specify type this way
            request.ContentType = "application/x-www-form-urlencoded";
            request.ContentLength = postBytes.Length;
            Stream requestStream = request.GetRequestStream();

            // now send it
            requestStream.Write(postBytes, 0, postBytes.Length);
            requestStream.Close();

            // grab te response and print it out to the console along with the status code
            HttpWebResponse response = (HttpWebResponse)request.GetResponse();
            string responseString = new StreamReader(response.GetResponseStream(), Encoding.GetEncoding("UTF-8")).ReadToEnd();
            LogCreditTestData("response phase2: " + responseString);


            return Content("");
        }
        public ActionResult _PayCreditGuard(int ID)
        {
            Order o = _db.Orders.FirstOrDefault(r => r.ID == ID);
            if (o == null) return Content("Can't find order");

            o.ApprovedGuid = Guid.NewGuid().ToString();
            _db.SaveChanges();

            //Check if already payed
            //if (o.OrderStatus == 0) return Content("Already payed");
            var shop = ShoppingService.GetShopByID(o.ShopID);
            if (shop == null) return Content("Shop doesn`t exist");
            string CreditGuardUser = shop.CreditGuardUser;
            string CreditGuardPass = shop.CreditGuardPass;
            string CreditGuardTerminal = shop.CreditGuardTerminal;
            string CreditGuardMid = shop.CreditGuardMid;
            string CreditGuardUrl = shop.CreditGuardUrl;

            string returnUrl = "https://" + Request.ServerVariables["HTTP_HOST"] + "/Checkout/PayCreditVerifyGuardOK";
            string cancelReturnUrl = "https://" + Request.ServerVariables["HTTP_HOST"] + "/Checkout/PayCreditGuardNotOK";

            // this is what we are sending
            string xml_data = "<ashrait>" +
                    "<request>" +
                        "<version>1001</version>" +
                        "<language>HEB</language>" +
                        "<dateTime/>" +
                        "<command>doDeal</command>" +
                        "<requestid/>" +
                            "<doDeal>" +
                                "<terminalNumber>" + CreditGuardTerminal + "</terminalNumber>" +
                                "<cardNo>CGMPI</cardNo>" +
                                "<total>" + Math.Round((o.Total) * 100, 0).ToString("0", CultureInfo.InvariantCulture) + "</total>" +
                                "<transactionType>Debit</transactionType>" +
                                "<creditType>RegularCredit</creditType>" +
                                "<currency>ILS</currency>" +
                                "<transactionCode>Phone</transactionCode>" +
                                "<validation>TxnSetup</validation>" +
                                "<firstPayment></firstPayment>" +
                                "<periodicalPayment></periodicalPayment>" +
                                "<numberOfPayments></numberOfPayments>" +
                                "<user>" + o.FullName + "</user>" +
                                "<eci></eci>" +
                                "<mid>" + CreditGuardMid + "</mid>" +
                                "<uniqueid>" + o.ApprovedGuid + "</uniqueid>" +
                                "<mpiValidation>Token</mpiValidation>" +
                                "<description>" + "site slk" + "</description>" +
                                "<email>" + o.Email + "</email>" +
                                "<successUrl>" + returnUrl + "</successUrl>" +
                                "<errorUrl>" + cancelReturnUrl + "</errorUrl>" +
                                "<cancelUrl>" + cancelReturnUrl + "</cancelUrl>" +
                                "<customerData>" +
                                    "<userData1/>" +
                                    "<userData2/>" +
                                    "<userData3/>" +
                                    "<userData4/>" +
                                    "<userData5/>" +
                                    "<userData6/>" +
                                    "<userData7/>" +
                                    "<userData8/>" +
                                    "<userData9/>" +
                                    "<userData10/>" +
                                "</customerData>" +
                            "</doDeal>" +
                        "</request>" +
                    "</ashrait>";

            string post_data = "user=" + CreditGuardUser + "&password=" + CreditGuardPass + "&int_in=" + xml_data;
            // this is where we will send it
            string uri = CreditGuardUrl;
            //"https://cguat2.creditguard.co.il/xpo/Relay";

            // create a request
            HttpWebRequest request = (HttpWebRequest)
            WebRequest.Create(uri);
            request.KeepAlive = false;
            request.ProtocolVersion = HttpVersion.Version10;
            request.Method = "POST";

            // turn our request string into a byte stream
            //byte[] postBytes = Encoding.ASCII.GetBytes(post_data);
            LogCreditTestData("sent: " + post_data);
            byte[] postBytes = Encoding.GetEncoding("UTF-8").GetBytes(post_data);

            // this is important - make sure you specify type this way
            request.ContentType = "application/x-www-form-urlencoded";
            request.ContentLength = postBytes.Length;
            Stream requestStream = request.GetRequestStream();

            // now send it
            requestStream.Write(postBytes, 0, postBytes.Length);
            requestStream.Close();

            // grab te response and print it out to the console along with the status code
            HttpWebResponse response = (HttpWebResponse)request.GetResponse();
            string responseString = new StreamReader(response.GetResponseStream(), Encoding.GetEncoding("UTF-8")).ReadToEnd();
            LogCreditTestData("response: " + responseString);
            XmlDocument doc = new XmlDocument();
            doc.LoadXml(responseString);
            XmlNodeList Nodes1 = doc.GetElementsByTagName("mpiHostedPageUrl");
            XmlNodeList Nodes2 = doc.GetElementsByTagName("userMessage");
            XmlNodeList Nodes3 = doc.GetElementsByTagName("message");
            XmlNodeList Nodes4 = doc.GetElementsByTagName("result");

            if (!responseString.Contains("https://")) return Content(string.Format("{0}: {1} - {2}", Nodes2[0].InnerText, Nodes4[0].InnerText, Nodes3[0].InnerText));

            string ResponseUrl = Nodes1[0].InnerText;

            ViewBag.Url = ResponseUrl;
            return View();

        }
        public ActionResult PayCreditVerifyGuardOK(string uniqueID, string txId, string authNumber, string cardToken, string cardExp)
        {
            LogCreditGuardData("PayCreditGuardOK, uniqueID:" + uniqueID + ", txId:" + txId
                + ", authNumber:" + authNumber + ", cardToken: " + cardToken + ", cardExp: " + cardExp);

            Order o = _db.Orders.FirstOrDefault(r => r.ApprovedGuid == uniqueID);
            string error = "";
            if (o == null) error = RP.T("CheckoutController.Error.CantFindOrder").ToString();

            if (!string.IsNullOrEmpty(error))
            {
                LogCreditGuardData(error);
                return Content(error);
            }

            o.OrderStatus = OrderStatus.Paid;
            o.PayedOn = DateTime.Now;

            o.ApprovedGuid = cardExp;
            o.ApprovedToken = cardToken;
            o.ApprovedDate = DateTime.Now;

            //o.Log = o.Log + DateTime.Now + " - תשלום התקבל דרך CreditGuard. IP - " + Request.ServerVariables["LOCAL_ADDR"] + "\r\n";

            _db.SaveChanges();
            var shop = ShoppingService.GetShopByID(o.ShopID);
            var messService = new MessageService(_db);
            messService.SendOrderPayedEmailToMember(o, shop);
            messService.SendOrderPayedEmailToUser(o, shop);

            return RedirectToAction("PayDone");
        }
        [SSLrequired(MobileOnly = false)]
        public ActionResult _PayCreditStandart(int ID)
        {
            Order o = _db.Orders.FirstOrDefault(r => r.ID == ID);
            if (o == null) return Content("Can't find order");

            o.ApprovedGuid = Guid.NewGuid().ToString();
            _db.SaveChanges();

            //Check if already payed
            //if (o.OrderStatus == 0) return Content("Already payed");
            var shop = ShoppingService.GetShopByID(o.ShopID);
            if (shop == null) return Content("Shop doesn`t exist");
            string CreditGuardUser = shop.CreditGuardUser;
            string CreditGuardPass = shop.CreditGuardPass;
            string CreditGuardTerminal = shop.CreditGuardTerminal;
            string CreditGuardMid = shop.CreditGuardMid;
            string CreditGuardUrl = shop.CreditGuardUrl;

            string returnUrl = "https://" + Request.ServerVariables["HTTP_HOST"] + "/Checkout/PayCreditGuardOK";
            string cancelReturnUrl = "https://" + Request.ServerVariables["HTTP_HOST"] + "/Checkout/PayCreditGuardNotOK";

            // this is what we are sending
            string xml_data = "<ashrait>" +
                    "<request>" +
                        "<version>1001</version>" +
                        "<language>HEB</language>" +
                        "<dateTime/>" +
                        "<command>doDeal</command>" +
                        "<requestid/>" +
                            "<doDeal>" +
                                "<terminalNumber>" + CreditGuardTerminal + "</terminalNumber>" +
                                "<cardNo>CGMPI</cardNo>" +
                                "<total>" + Math.Round((o.Total) * 100, 0).ToString("0", CultureInfo.InvariantCulture) + "</total>" +
                                "<transactionType>Debit</transactionType>" +
                                "<creditType>RegularCredit</creditType>" +
                                "<currency>ILS</currency>" +
                                "<transactionCode>Phone</transactionCode>" +
                                "<validation>TxnSetup</validation>" +
                                "<firstPayment></firstPayment>" +
                                "<periodicalPayment></periodicalPayment>" +
                                "<numberOfPayments></numberOfPayments>" +
                                "<user>" + o.FullName + "</user>" +
                                "<eci></eci>" +
                                "<mid>" + CreditGuardMid + "</mid>" +
                                "<uniqueid>" + o.ApprovedGuid + "</uniqueid>" +
                                "<mpiValidation>AutoComm</mpiValidation>" +
                                "<description>" + "site slk" + "</description>" +
                                "<email>" + o.Email + "</email>" +
                                "<successUrl>" + returnUrl + "</successUrl>" +
                                "<errorUrl>" + cancelReturnUrl + "</errorUrl>" +
                                "<cancelUrl>" + cancelReturnUrl + "</cancelUrl>" +
                                "<customerData>" +
                                    "<userData1/>" +
                                    "<userData2/>" +
                                    "<userData3/>" +
                                    "<userData4/>" +
                                    "<userData5/>" +
                                    "<userData6/>" +
                                    "<userData7/>" +
                                    "<userData8/>" +
                                    "<userData9/>" +
                                    "<userData10/>" +
                                "</customerData>" +
                            "</doDeal>" +
                        "</request>" +
                    "</ashrait>";

            string post_data = "user=" + CreditGuardUser + "&password=" + CreditGuardPass + "&int_in=" + xml_data;
            // this is where we will send it
            string uri = CreditGuardUrl;
            //"https://cguat2.creditguard.co.il/xpo/Relay";

            // create a request
            HttpWebRequest request = (HttpWebRequest)
            WebRequest.Create(uri);
            request.KeepAlive = false;
            request.ProtocolVersion = HttpVersion.Version10;
            request.Method = "POST";

            // turn our request string into a byte stream
            //byte[] postBytes = Encoding.ASCII.GetBytes(post_data);
            byte[] postBytes = Encoding.GetEncoding("UTF-8").GetBytes(post_data);

            // this is important - make sure you specify type this way
            request.ContentType = "application/x-www-form-urlencoded";
            request.ContentLength = postBytes.Length;
            Stream requestStream = request.GetRequestStream();

            // now send it
            requestStream.Write(postBytes, 0, postBytes.Length);
            requestStream.Close();

            // grab te response and print it out to the console along with the status code
            HttpWebResponse response = (HttpWebResponse)request.GetResponse();
            string responseString = new StreamReader(response.GetResponseStream(), Encoding.GetEncoding("UTF-8")).ReadToEnd();

            XmlDocument doc = new XmlDocument();
            doc.LoadXml(responseString);
            XmlNodeList Nodes1 = doc.GetElementsByTagName("mpiHostedPageUrl");
            XmlNodeList Nodes2 = doc.GetElementsByTagName("userMessage");
            XmlNodeList Nodes3 = doc.GetElementsByTagName("message");
            XmlNodeList Nodes4 = doc.GetElementsByTagName("result");

            if (!responseString.Contains("https://")) return Content(string.Format("{0}: {1} - {2}", Nodes2[0].InnerText, Nodes4[0].InnerText, Nodes3[0].InnerText));

            string ResponseUrl = Nodes1[0].InnerText;

            ViewBag.Url = ResponseUrl;
            return View();
        }
        [SSLrequired(MobileOnly = false)]
        public ActionResult PayCreditGuardOK(string uniqueID, string txId, string authNumber, string cardToken, string cardExp)
        {
            LogCreditGuardData("PayCreditGuardOK, uniqueID:" + uniqueID + ", txId:" + txId + ", authNumber:" + authNumber);

            Order o = _db.Orders.FirstOrDefault(r => r.ApprovedGuid == uniqueID);
            string error = "";
            if (o == null) error = RP.T("CheckoutController.Error.CantFindOrder").ToString();

            if (!string.IsNullOrEmpty(error))
            {
                LogCreditGuardData(error);
                return Content(error);
            }

            o.OrderStatus = OrderStatus.Accepted;
            o.PayedOn = DateTime.Now;

            o.ApprovedGuid = txId;
            o.ApprovedToken = authNumber;
            o.ApprovedDate = DateTime.Now;

            //o.Log = o.Log + DateTime.Now + " - תשלום התקבל דרך CreditGuard. IP - " + Request.ServerVariables["LOCAL_ADDR"] + "\r\n";

            _db.SaveChanges();
            var shop = ShoppingService.GetShopByID(o.ShopID);
            var messService = new MessageService(_db);
            messService.SendOrderPayedEmailToMember(o, shop);
            messService.SendOrderPayedEmailToUser(o, shop);

            return RedirectToAction("PayDone");
        }
        [SSLrequired(MobileOnly = false)]
        public ActionResult PayDone()
        {
            return View();
        }
        [SSLrequired(MobileOnly = false)]
        public ActionResult PayNotDone()
        {
            if (TempData["ErrorText"] != null
                &&
             TempData["ErrorCode"] != null)
            {
                ViewBag.ErrorText = TempData["ErrorText"];
                ViewBag.ErrorCode = TempData["ErrorCode"];
            }
            return View();
        }
        [SSLrequired(MobileOnly = false)]
        public ActionResult PayCreditGuardNotOK(string uniqueID, string txId, string authNumber, string ErrorText, string ErrorCode)
        {
            LogCreditGuardData("PayCreditGuardNotOK, uniqueID:" + uniqueID + ", txId:" + txId + ", authNumber:" + authNumber + ", ErrorCode:" + ErrorCode + ", ErrorText:" + ErrorText);

            string theme = CurrentSettings.Themes;
            if (string.IsNullOrEmpty(theme)) ViewBag.Layout = "~/Views/Shared/_Layout1Column.cshtml";
            else ViewBag.Layout = "~/Themes/" + theme + "/Views/Shared/_Layout1Column.cshtml";
            TempData["ErrorText"] = ErrorText;
            TempData["ErrorCode"] = ErrorCode;
            return RedirectToAction("PayNotDone");
        }

        private void LogCreditGuardData(string LogText)
        {
            System.IO.File.AppendAllText(Server.MapPath("~/App_Data/creditguardlog.txt"), string.Format("{0} IP:{2} - {1}\r\n", DateTime.Now, LogText, Request.ServerVariables["LOCAL_ADDR"]));
        }

        #endregion
    }
}

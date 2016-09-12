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
using System.Text;
using Uco.Infrastructure.Services;
using Uco.Models.Overview;
using Kendo.Mvc.UI;
using Kendo.Mvc;
using Kendo.Mvc.Extensions;
using System.Linq.Expressions;
using Uco.Infrastructure.EntityExtensions;
using System.IO;
using System.Net;
using System.Globalization;
using System.Xml;

namespace Uco.Areas.Member.Controllers
{
     [Authorize(Roles = "Member")]
    public class MainController : BaseMemberController
    {

         public ActionResult Index()
         {
             if (CurrentShop == null) return Content("Shop not found. Please register");
             ViewBag.MessageRed = TempData["MessageRed"];
             ViewBag.MessageYellow = TempData["MessageYellow"];
             ViewBag.MessageGreen = TempData["MessageGreen"];

             OrderListModel model = new OrderListModel();

             model.AcceptedCount = _db.Orders.Count(x => x.ShopID == CurrentShop.ID
                 && x.OrderStatus == OrderStatus.Accepted);

             model.NewCount = _db.Orders.Count(x => x.ShopID == CurrentShop.ID
               && x.OrderStatus == OrderStatus.New);

             model.PayedCount = _db.Orders.Count(x => x.ShopID == CurrentShop.ID
               && x.OrderStatus == OrderStatus.Paid);

             model.SentCount = _db.Orders.Count(x => x.ShopID == CurrentShop.ID
               && x.OrderStatus == OrderStatus.Sent);

             model.DelivereCount = _db.Orders.Count(x => x.ShopID == CurrentShop.ID
               && x.OrderStatus == OrderStatus.Delivered);
             model.NewAndPyedCount = model.NewCount + model.PayedCount;

             model.CanceledCount = _db.Orders.Count(x => x.ShopID == CurrentShop.ID
               && x.OrderStatus == OrderStatus.Canceled);
             return View(model);
         }

         public ActionResult PaymentReport()
         {
             if (CurrentShop == null) return Content("Shop not found. Please register");
             ViewBag.MessageRed = TempData["MessageRed"];
             ViewBag.MessageYellow = TempData["MessageYellow"];
             ViewBag.MessageGreen = TempData["MessageGreen"];

             var model = new PaymentReportModel();

             return View(model);
         }
         
         [HttpPost]
         public ActionResult SaveCell(int ID, string Field, string Value, string OrderNote)
         {
             //if(string.IsNullOrEmpty(OrderNote))
             //{
             //    return Json(new { result = "error", message = "Note required", value = Value });
             //}
             var order = _db.Orders.FirstOrDefault(x => x.ID == ID);
             if(order != null)
             {
                var property = order.GetType().GetProperty(Field);
                 if(property!=null)
                 {
                     var oldValue = property.GetValue(order);
                     order.SetValueFromString(Field,Value);
                     var newValue = property.GetValue(order);
                     var oldValueStr = oldValue.ToString();
                     var newValueStr = newValue.ToString();
                     if(property.PropertyType.Name == "Double")
                     {
                          oldValueStr = ((decimal)oldValue).ToString("0.00");
                          newValueStr = ((decimal)newValue).ToString("0.00");
                     }

                     if (Field != "Total" && Field != "TotalCash")
                     {
                         //fix totals
                         if (order.ShippingMethod == ShippingMethod.Manual)
                         {
                             order.ShipCost = 0;
                         }
                           order.Total = order.SubTotal + order.ShipCost + order.Fee - order.TotalDiscountAmount;
                        
                             
                     }

                     if (newValueStr != oldValueStr)
                     {
                         _db.SaveChanges();
                         //adding note
                         if (string.IsNullOrEmpty(OrderNote))
                         {
                             OrderNote = RP.S("Member.OrderNote.DefaultMessage");
                         }
                             var note = new OrderNote()
                             {
                                 CreateDate = DateTime.Now,
                                 Field = RP.M("Order", Field),
                                 OrderID = order.ID,
                                 Note = OrderNote,
                                 NewValue = newValueStr,
                                 OldValue = oldValueStr
                             };
                             _db.OrderNotes.Add(note);
                             _db.SaveChanges();
                         
                         return Json(new { result = "ok", value = Value });
                     }
                     return Json(new { result = "error", message = "Value not changed", value = Value });
                 }
                 return Json(new { result = "error", message = "no property", value = Value });
             }
             return Json(new { result = "error", message="not found", value = Value });
         }
         

         [HttpPost]
         public ActionResult _OrderItemAjaxRead([DataSourceRequest]DataSourceRequest request)
         {
             if (CurrentShop == null)
             {
                 return Json(new { });
             }
             var ShopID = CurrentShop.ID;
             var decimalInt = (double)((FilterDescriptor)request.Filters.FirstOrDefault()).ConvertedValue;
             var orderID = Convert.ToInt32 ( decimalInt);
             var query = (
              from oi in LS.CurrentEntityContext.OrderItems.Where(x => x.OrderID == orderID)
              from ps in LS.CurrentEntityContext.ProductShopMap.Where(x => x.ID == oi.ProductShopID
                   && x.ShopID == ShopID).DefaultIfEmpty()
              from p in LS.CurrentEntityContext.Products.Where(x => x.ID == ps.ProductID).DefaultIfEmpty()
              orderby p.CategoryID
              select oi).Distinct();
             var data = query.ToList();

             var productShopsIds = data.Select(x => x.ProductShopID).ToList();
             var productShops = LS.CurrentEntityContext.ProductShopMap.Where(x => productShopsIds.Contains(x.ID)).ToList();

             var productIds = productShops.Select(x => x.ProductID).ToList();
             var products = LS.CurrentEntityContext.Products.Where(x => productIds.Contains(x.ID)).ToList();
             foreach(var group in data)
             {

                 var ps = productShops.FirstOrDefault(x=>x.ID == group.ProductShopID);
                 if(ps!=null)
                 {
                     group.ProductShop = ps;
                     var p = products.FirstOrDefault(x=>x.ID == ps.ProductID);
                     if(p!=null)
                     {
                         group.ProductShop.Product = p;
                         group.VirtualOrder = p.CategoryID;
                         var cat = LS.Get<Category>().FirstOrDefault(x=>x.ID == p.CategoryID);
                         if(cat != null)
                         {
                             group.ProductShop.Product.Category = cat;
                         }
                     }
                 }

             }



             var result = data.OrderBy(x=>x.VirtualOrder).Take(request.PageSize).Skip((request.Page - 1) * request.PageSize).ToList();//.Select(x => x.oi).ToList();
             return Json(new { Data = result, Errors = (List<string>)null, AggregateResults = (List<string>)null, Total = query.Count() });
         }

                [HttpPost]
         public ActionResult _AjaxPaymentReport([DataSourceRequest]DataSourceRequest request)
        {
            if (CurrentShop == null)
            {
                return Json(new {});
            }
            //kendo reset manual sort to default =(
             if (request.Sorts.Count == 0)
             {
                 request.Sorts.Add(new SortDescriptor("ID",
                     System.ComponentModel.ListSortDirection.Descending));
             }
                    Expression<Func<Order,bool>> predicate = x => x.ShopID == CurrentShop.ID
               //  && x.OrderStatus != OrderStatus.New
                // && x.OrderStatus != OrderStatus.Canceled
               //  && x.OrderStatus != OrderStatus.Rejected
               ;
                    var items = _db.Orders.Where(predicate)
                 .Select(x=> new PaymentReportItemModel(){
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
                item.PayedToStr = RP.T("Enums." +item.PayedTo.ToString()).ToString();
                item.PaymentMethodStr = RP.T("Enums." +item.PaymentMethod.ToString()).ToString();
                item.DateStr = item.Date.HasValue ? item.Date.Value.ToString("dd/MM HH:mm") : "";
            }
                  
                    //prepare report model
            var model = new PaymentReportModel();
                    //prepare filters, get from kendo grid filter
            if (request.Filters != null)
            {
                predicate = GetPredicate(predicate, request.Filters);

            }
            model.TotalShop = _db.Orders.Where(predicate).Where(x=>x.PaymentMethod == PaymentMethod.CreditShopOwner && !x.LessFee)
                .Select(x => x.Total).DefaultIfEmpty(0).Sum();
            model.TotalShop += _db.Orders.Where(predicate).Where(x=> !x.LessFee).Select(x => x.Total).DefaultIfEmpty(0).Sum();
            var totalSpecial =  _db.Orders.Where(predicate).Where(x => x.PaymentMethod == PaymentMethod.CreditShopOwner && x.LessFee)
        .Select(x => x.Total).DefaultIfEmpty(0).Sum();
            totalSpecial += _db.Orders.Where(predicate).Where(x => x.LessFee).Select(x => x.Total).DefaultIfEmpty(0).Sum();


            model.TotalAdmin = _db.Orders.Where(predicate).Where(x => x.PaymentMethod == PaymentMethod.Credit)
                .Select(x => x.Total).DefaultIfEmpty(0).Sum();
                
            var date1 = items.Where(x => x.Date.HasValue).Select(x => x.Date.Value).DefaultIfEmpty(DateTime.Now).Max();
            var date2 = items.Where(x => x.Date.HasValue).Select(x => x.Date.Value).DefaultIfEmpty(DateTime.Now).Min();
            int mouthes = ((date1.Year - date2.Year) * 12) + date1.Month - date2.Month;
            decimal mounthFee = CurrentShop.MounthlyFee * mouthes;

                    model.TotalFee = (model.TotalShop * CurrentShop.PercentFee / 100) - model.TotalAdmin + mounthFee;
                    model.TotalFee += (totalSpecial * CurrentShop.SpecialPercentFee / 100);
                    model.TotalShop += totalSpecial;
           // if (model.TotalFee < 0) { model.TotalFee = 0; }

            model.TotalAdminStr = ShoppingService.FormatPrice(model.TotalAdmin);
            model.TotalShopStr = ShoppingService.FormatPrice(model.TotalShop);
            model.TotalFeeStr = ShoppingService.FormatPrice(model.TotalFee);


            return Json(new { result.Data, result.AggregateResults,result.Errors,result.Total,model  });
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
                #region payment export
                public ActionResult CSVExport([DataSourceRequest]DataSourceRequest request)
                {
                    if (CurrentShop == null) return Content("Shop not found. Please register");
                    Expression<Func<Order, bool>> predicate = x => x.ShopID == CurrentShop.ID
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
                        Total = x.Total,
                      

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
         //public ActionResult DistanceTest(decimal latitude=0,decimal longitude=0)
         //{

         //    var shops = ShoppingService.GetNearestShop(0,latitude, longitude);
         //    StringBuilder content = new StringBuilder();
         //    content.AppendLine(string.Format("You are on {0}lt {1}lg <br/><table>",latitude,longitude));
         //    content.AppendLine("<tr><td>name</td><td>order</td><td>lt</td><td>lg</td></tr>");
           
         //    foreach (var s in shops)
         //    {
         //        content.AppendLine(string.Format("<tr><td>{0}</td><td>{1}</td><td>{2}lt</td><td>{3}lg</td></tr>", s.Name, s.DisplayOrder, s.Latitude, s.Longitude));
         //    }
         //    content.AppendLine("</table>");
         //    return Content(content.ToString());
         //}

         [HttpGet]
        public ActionResult ShopProfile()
        {
            if (CurrentShop == null) return Content("Shop not found. Please register");
            ViewBag.MessageRed = TempData["MessageRed"];
            ViewBag.MessageYellow = TempData["MessageYellow"];
            ViewBag.MessageGreen = TempData["MessageGreen"];
            return View(CurrentShop);
        }

         [HttpPost]
         public ActionResult ShopProfile(Shop shop)
         {
             if (CurrentShop == null) return Content("Shop not found. Please register");
             if (shop.ID == CurrentShop.ID)
             {
                 CurrentShop.Address = shop.Address;
                 CurrentShop.AddressMap = shop.AddressMap;
                 CurrentShop.RadiusLatitude = shop.RadiusLatitude;
                 CurrentShop.RadiusLongitude = shop.RadiusLongitude;
                 CurrentShop.DisplayOrder = shop.DisplayOrder;
                 CurrentShop.Email = shop.Email;
                 CurrentShop.FullDescription = shop.FullDescription;
                 CurrentShop.Image = shop.Image;
                 CurrentShop.Kosher = shop.Kosher;
                 CurrentShop.Latitude = shop.Latitude;
                 CurrentShop.Longitude = shop.Longitude;
                 CurrentShop.Name = shop.Name;
                 CurrentShop.Phone = shop.Phone;
                 CurrentShop.ShipCost = shop.ShipCost;
                 CurrentShop.FreeShipFrom = shop.FreeShipFrom;
                 //CurrentShop.ShipHourFrom = shop.ShipHourFrom;
                 //CurrentShop.ShipHourTo = shop.ShipHourTo;
                 CurrentShop.ShipRadius = shop.ShipRadius;
                 //CurrentShop.ShopTypeID = shop.ShopTypeID;
                 CurrentShop.ShopTypeIDs = shop.ShopTypeIDs;
                 CurrentShop.ShortDescription = shop.ShortDescription;
                 //CurrentShop.TraidingDayFrom = shop.TraidingDayFrom;
                 //CurrentShop.TraidingDayTo = shop.TraidingDayTo;
                 //CurrentShop.TraidingHourFrom = shop.TraidingHourFrom;
                 //CurrentShop.TraidingHourTo = shop.TraidingHourTo;
                 CurrentShop.Youtube = shop.Youtube;
                 CurrentShop.Active = shop.Active;
                 CurrentShop.DeliveryManualDescription = shop.DeliveryManualDescription;
                 CurrentShop.DeliveryTime = shop.DeliveryTime;
                 CurrentShop.InStorePickUpEnabled = shop.InStorePickUpEnabled;
                 CurrentShop.IsShipEnabled = shop.IsShipEnabled;
                 CurrentShop.Theme = shop.Theme;
                 CurrentShop.FavIcon = shop.FavIcon;
                 _db.SaveChanges();

                 ViewBag.MessageGreen = RP.T("Member.Controlers.MainController.Saved").ToString();
             }
             return View(CurrentShop);
         }
           [SSLrequired(MobileOnly = true)]
        public ActionResult Detail(int ID)
        {
            var order = _db.Orders.FirstOrDefault(x => x.ID == ID);
            if (order == null) {
                TempData["MessageRed"] = "Order doesn`t exists";
                return RedirectToAction("Index");
            }
            order.User = _db.Users.FirstOrDefault(x => x.ID == order.UserID);
           // order.OrderStatus = _db.OrderStatuses.FirstOrDefault(x => x.ID == order.OrderStatusID);
           
            return View(order);
        }

           public ActionResult AjaxDetail(int ID)
           {
               var order = _db.Orders.FirstOrDefault(x => x.ID == ID);
               if (order == null)
               {
                   TempData["MessageRed"] = "Order doesn`t exists";
                   return RedirectToAction("Index");
               }
               order.User = _db.Users.FirstOrDefault(x => x.ID == order.UserID);
               // order.OrderStatus = _db.OrderStatuses.FirstOrDefault(x => x.ID == order.OrderStatusID);

               return View(order);
           }

        #region times
        private ShopShipTime GetTime(DayOfWeek en,int From = 480, int To = 1200)
        {
            var time = new ShopShipTime();
            time.ShopID = CurrentShop.ID;
            time.IsSpecial = false;
            time.Day = en;
            time.Date = DateTime.Now;
            time.TimeFrom = From;
            time.TimeTo = To;
            time.Active = true;
            return time;
        }
             [HttpPost]
        public ActionResult ReadShipTimes()
        {
            if (CurrentShop == null) return Json(new { result = "error", message = RP.S("Member.Error.ShopNotFound") });
            var list = _db.ShopShipTimes.Where(x => x.ShopID == CurrentShop.ID
               && !x.IsSpecial).ToList();
            foreach (var en in Enum.GetValues(typeof(DayOfWeek)))
            {
                if (!list.Any(x => x.Day == (DayOfWeek)en))
                {
                    var time = GetTime((DayOfWeek)en);
                    if((int)en == 6)
                    {
                        time = GetTime((DayOfWeek)en, 1140, 1440);
                        time.Active = false;
                    }
                    else if ((int)en == 5)
                    {
                        time = GetTime((DayOfWeek)en, 480, 840);
                        
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
                 if (CurrentShop == null) return Json(new { result = "error", message = RP.S("Member.Error.ShopNotFound") });

                 var CurTime = _db.ShopShipTimes.FirstOrDefault(x => x.ShopID == CurrentShop.ID
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
                     time.ShopID = CurrentShop.ID;
                     time.IsSpecial = false;
                     time.Date = DateTime.Now;
                     _db.ShopShipTimes.Add(time);
                     _db.SaveChanges();
                     CurTime = time;
                 }
                 return Json(new { result = "ok", data = CurTime });
             }

             private ShopWorkTime GetWorkTime(DayOfWeek en, int From = 480, int To = 1200)
             {
                 var time = new ShopWorkTime();
                 time.ShopID = CurrentShop.ID;
                 time.IsSpecial = false;
                 time.Day = en;
                 time.Date = DateTime.Now;
                 time.TimeFrom = From;
                 time.TimeTo = To;
                 time.Active = true;
                 return time;
             }

             [HttpPost]
             public ActionResult ReadWorkTimes()
             {
                 if (CurrentShop == null) return Json(new { result = "error", message = RP.S("Member.Error.ShopNotFound") });
                 var list = _db.ShopWorkTimes.Where(x => x.ShopID == CurrentShop.ID
                    && !x.IsSpecial).ToList();

                 foreach (var en in Enum.GetValues(typeof(DayOfWeek)))
                 {
                     if (!list.Any(x => x.Day == (DayOfWeek)en))
                     {
                         var time = GetWorkTime((DayOfWeek)en);
                         if ((int)en == 6)
                         {
                             time = GetWorkTime((DayOfWeek)en,1140, 1440);
                             time.Active = false;
                         }
                         else if ((int)en == 5)
                         {
                             time = GetWorkTime((DayOfWeek)en,480, 840);
                            
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
                 if (CurrentShop == null) return Json(new { result = "error", message = RP.S("Member.Error.ShopNotFound") });

                 var CurTime = _db.ShopWorkTimes.FirstOrDefault(x => x.ShopID == CurrentShop.ID
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
                     time.ShopID = CurrentShop.ID;
                     time.IsSpecial = false;
                     time.Date = DateTime.Now;
                     _db.ShopWorkTimes.Add(time);
                     _db.SaveChanges();
                     CurTime = time;
                 }
                 return Json(new { result = "ok", data = CurTime });
             }
#endregion
             #region ajax
             [HttpPost]
             public ActionResult OrderReject(int ID)
             {
                 if (CurrentShop == null) return Json(new { result = "error", message = RP.S("Member.Error.ShopNotFound") });
          var order = _db.Orders.FirstOrDefault(x => x.ID == ID && x.ShopID == CurrentShop.ID);
             if(order == null)
             {
                 return Json(new { result = "error", message = RP.S("Member.Error.OrderNotFound") });
             }
             if (order.OrderStatus != OrderStatus.New && order.OrderStatus != OrderStatus.Paid)
             {
                 return Json(new { result = "error", message = RP.S("Member.Error.OrderAlreadyProcessed") });
             }
             order.OrderStatus = OrderStatus.Rejected;//ShoppingService.RejectedOrderStatus();
             _db.SaveChanges();
             // add to cart
             var items = _db.OrderItems.Where(x => x.OrderID == order.ID).ToList();
             OrderItemStatus outItemStatus = OrderItemStatus.OutOfStock;
             foreach (var oi in items)
             {
                 if (oi.OrderItemStatus != outItemStatus && oi.Quantity > 0)
                     ShoppingCartService.AddToCart(order.UserID, new ShoppingCartItem()
                     {
                         ProductAttributeOptionID = oi.ProductAttributeOptionID,
                         ProductShopID = oi.ProductShopID,
                         Quantity = oi.Quantity,
                         ShopID = order.ShopID,

                     });
             }
              //send email
             var messService = new MessageService(_db);
             var user = _db.Users.FirstOrDefault(x => x.ID == order.UserID);

             var shop = _db.Shops.FirstOrDefault(x => x.ID == order.ShopID);

             messService.OrderCanceledSmsToUser(order, user);
             messService.OrderCanceledEmailToUser(order, shop);
             return Json(new { result = "ok", message = RP.S("Member.OrderChange.Rejected") });
         }
         [HttpPost]
         public ActionResult OrderMissing(int ID)
         {
             if (CurrentShop == null) return Json(new { result = "error", message = RP.S("Member.Error.ShopNotFound") });
             var order = _db.Orders.FirstOrDefault(x => x.ID == ID && x.ShopID == CurrentShop.ID);
             if (order == null)
             {
                 return Json(new { result = "error", message = RP.S("Member.Error.OrderNotFound") });
             }
             if (order.OrderStatus != OrderStatus.New && order.OrderStatus != OrderStatus.Paid)
             {
                 return Json(new { result = "error", message = RP.S("Member.Error.OrderAlreadyProcessed") });
             }
             OrderItemStatus outItemStatus = OrderItemStatus.OutOfStock;
             OrderItemStatus changedItemStatus = OrderItemStatus.Changed;
             var any = _db.OrderItems.Any(x => x.OrderID == order.ID && (x.OrderItemStatus == outItemStatus
                 || 
                 x.OrderItemStatus == changedItemStatus)
                 );
             if (!any) //check out of stock items
             {
                 return Json(new { result = "error", message = RP.S("Member.Error.YouDontHaveChangedProducts") });
             }
             order.OrderStatus = OrderStatus.Rejected ;//ShoppingService.RejectedOrderStatus();//canceled
             _db.SaveChanges();
             // add to cart
             var items = _db.OrderItems.Where(x => x.OrderID == order.ID).ToList();
             foreach(var oi in items)
             {
                 if (oi.OrderItemStatus != outItemStatus && oi.Quantity > 0)
                 ShoppingCartService.AddToCart(order.UserID, new ShoppingCartItem() { 
                 ProductAttributeOptionID = oi.ProductAttributeOptionID,
                 ProductShopID = oi.ProductShopID,
                 Quantity = oi.Quantity,
                 ShopID = order.ShopID,
                
                 });
             }
             //send email
             var messService = new MessageService(_db);
             messService.SendOrderChangedEmailToUser(order,items
                 .Where(x=>x.OrderItemStatus == OrderItemStatus.Changed
                 ).ToList(),
                 items
                 .Where(x => x.OrderItemStatus == OrderItemStatus.OutOfStock
                 ).ToList()
                 );
             var user = _db.Users.FirstOrDefault(x => x.ID == order.UserID);
             messService.SendOrderChangedSmsToUser(order, user);
            
             //
             return Json(new { result = "ok", message = RP.S("Member.OrderChanged.ProductsReturnedToClientOrderRejected") });
         }
         [HttpPost]
         public ActionResult OrderConfirm(int ID)
         {
             if (CurrentShop == null) return Json(new { result = "error", message = RP.S("Member.Error.ShopNotFound") });
             var order = _db.Orders.FirstOrDefault(x => x.ID == ID && x.ShopID == CurrentShop.ID);
             if (order == null)
             {
                 return Json(new { result = "error", message = RP.S("Member.Error.OrderNotFound") });
             }
             if (order.OrderStatus != OrderStatus.New && order.OrderStatus != OrderStatus.Paid)
             {
                 return Json(new { result = "error", message = RP.S("Member.Error.OrderAlreadyProcessed") });
             }
             //check if member accept all items
             OrderItemStatus acceptedItemStatus = OrderItemStatus.Accepted;
             OrderItemStatus changedItemStatus = OrderItemStatus.Changed;
             var any = _db.OrderItems.Any(x => x.OrderID == order.ID && x.OrderItemStatus != acceptedItemStatus && x.OrderItemStatus != changedItemStatus);
             if (any) //member missing some products
             {
                 return Json(new { result = "error", message = RP.S("Member.Error.YouMustAcceptAllItemsFirst") });
             }
             if (order.PaymentMethod == PaymentMethod.Credit)
             {
                 if(order.OrderStatus == OrderStatus.Paid)
                 {
                     order.OrderStatus = OrderStatus.Accepted;
                     _db.SaveChanges();
                 }else
                 {
                     //run paid procedure
                    order.PayedOn = DateTime.Now;
                     //
                      order.OrderStatus = OrderStatus.Accepted;
                     _db.SaveChanges();
                     //and accept
                    // return Json(new { result = "error", message = "Oreder not paid, yet" });
                 }
             }
             else
             {
                 order.OrderStatus = OrderStatus.Accepted ;//ShoppingService.AcceptedOrderStatus();
                 order.PayedOn = DateTime.Now;
                 _db.SaveChanges();

             }

             return Json(new { result = "ok", message = RP.S("Member.OrderChanged.Confirmed") });
         }
         private void LogCreditTestData(string LogText)
         {
             System.IO.File.AppendAllText(Server.MapPath("~/App_Data/credittestlog.txt"), string.Format("{0} IP:{2} - {1}\r\n", DateTime.Now, LogText, Request.ServerVariables["LOCAL_ADDR"]));
         }
         private string ChargeCredit(int ID)
         {
             string result = "";
             Order o = _db.Orders.FirstOrDefault(r => r.ID == ID);
             if (o == null) return "order not found";


             //Check if already payed
             //if (o.OrderStatus == 0) return Content("Already payed");
             var shop = ShoppingService.GetShopByID(o.ShopID);
             if (shop == null) return "Shop not found";
             string CreditGuardUser = shop.CreditGuardUser;
             string CreditGuardPass = shop.CreditGuardPass;
             string CreditGuardTerminal = shop.CreditGuardTerminal;
             string CreditGuardMid = shop.CreditGuardMid;
             string CreditGuardUrl = shop.CreditGuardUrl;

             string returnUrl = "http://" + Request.ServerVariables["HTTP_HOST"] + "/Checkout/PayCreditTestGuardOK";
             string cancelReturnUrl = "http://" + Request.ServerVariables["HTTP_HOST"] + "/Checkout/PayCreditGuardNotOK";

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
             XmlDocument doc = new XmlDocument();
             doc.LoadXml(responseString);
             XmlNodeList Nodes3 = doc.GetElementsByTagName("message");
             XmlNodeList Nodes4 = doc.GetElementsByTagName("result");
             if(Nodes4.Count > 0)
             {
                 int resCode = 0;
                 int.TryParse(Nodes4[0].InnerText, out resCode);
                 if(resCode > 0)
                 {
                     if(Nodes3.Count > 0)
                     {
                         return Nodes3[0].InnerText;
                     }
                 }
             }
             return result;
         }
             [HttpPost]
         public ActionResult OrderConfirmAndCharge(int ID)
         {
             if (CurrentShop == null) return Json(new { result = "error", message = RP.S("Member.Error.ShopNotFound") });
             var order = _db.Orders.FirstOrDefault(x => x.ID == ID && x.ShopID == CurrentShop.ID);
             if (order == null)
             {
                 return Json(new { result = "error", message = RP.S("Member.Error.OrderNotFound") });
             }
             if (order.OrderStatus != OrderStatus.New && order.OrderStatus != OrderStatus.Paid)
             {
                 return Json(new { result = "error", message = RP.S("Member.Error.OrderAlreadyProcessed") });
             }
             //check if member accept all items
             OrderItemStatus acceptedItemStatus = OrderItemStatus.Accepted;

             var any = _db.OrderItems.Any(x => x.OrderID == order.ID && x.OrderItemStatus != acceptedItemStatus);
             if (any) //member missing some products
             {
                 return Json(new { result = "error", message = RP.S("Member.Error.YouMustAcceptAllItemsFirst") });
             }
             if (order.PaymentMethod == PaymentMethod.Credit || order.PaymentMethod == PaymentMethod.CreditShopOwner)
             {
                 if(order.OrderStatus == OrderStatus.Paid)
                 {
                     var responseMessage = ChargeCredit(order.ID);
                     if (string.IsNullOrEmpty(responseMessage))
                     {
                         order.OrderStatus = OrderStatus.Accepted;
                         _db.SaveChanges();
                     }
                     else
                     {
                         return Json(new { result = "error", message = responseMessage });
                     }
                 }else
                 {
                     //change to pay by phone 
                     order.PaymentMethod = PaymentMethod.ByPhone;
                    order.PayedOn = DateTime.Now;
                     //
                      order.OrderStatus = OrderStatus.Accepted;
                     _db.SaveChanges();
                     //and accept
                   //  return Json(new { result = "error", message = "Oreder not paid, yet" });
                 }
             }
             else
             {
                 order.OrderStatus = OrderStatus.Accepted ;//ShoppingService.AcceptedOrderStatus();
                 order.PayedOn = DateTime.Now;
                 _db.SaveChanges();

             }

             return Json(new { result = "ok", message = RP.S("Member.OrderChanged.Confirmed") });
         }

         [HttpPost]
         public ActionResult OrderSent(int ID)
         {
             if (CurrentShop == null) return Json(new { result = "error", message = RP.S("Member.Error.ShopNotFound") });
             var order = _db.Orders.FirstOrDefault(x => x.ID == ID && x.ShopID == CurrentShop.ID);
             if (order == null)
             {
                 return Json(new { result = "error", message = RP.S("Member.Error.OrderNotFound") });
             }
             if(order.OrderStatus != OrderStatus.Accepted)
             {
                 return Json(new { result = "error", message = RP.S("Member.Error.YouMustAcceptOrderFirst") });
             }
             order.OrderStatus = OrderStatus.Sent;//ShoppingService.SentOrderStatus();//
             order.SentOn = DateTime.Now;
             _db.SaveChanges();
             return Json(new { result = "ok", message = RP.S("Member.OrderChanged.Sended") });
         }

         
             [HttpPost]
         public ActionResult OrderRefund(int ID)
         {
             if (CurrentShop == null) return Json(new { result = "error", message = RP.S("Member.Error.ShopNotFound") });
             var order = _db.Orders.FirstOrDefault(x => x.ID == ID && x.ShopID == CurrentShop.ID);
             if (order == null)
             {
                 return Json(new { result = "error", message = RP.S("Member.Error.OrderNotFound") });
             }
             if(order.OrderStatus != OrderStatus.Delivered)
             {
                 return Json(new { result = "error", message = RP.S("Member.Error.OrderNotDeliveredYet") });
             }
            // order.OrderStatus = OrderStatus.Refunded;
             order.RefundOn = DateTime.Now;
             order.RefundAmount = order.Total ;
             _db.SaveChanges();
                 if(order.PaymentMethod == PaymentMethod.Credit)
                 {
                     ShoppingService.RefundOrder(order);
                 }
                 return Json(new { result = "ok", message = RP.S("Member.OrderChanged.OrderRefounded") });
         }

             [HttpPost]
             public ActionResult OrderPartialRefund(int ID, decimal Amount)
             {

                 if (CurrentShop == null) return Json(new { result = "error", message = RP.S("Member.Error.ShopNotFound") });
                 if (Amount <= 0) return Json(new { result = "error", message = RP.S("Member.Error.AmountMustBeGreatenThanZero") });
                 var order = _db.Orders.FirstOrDefault(x => x.ID == ID && x.ShopID == CurrentShop.ID);
                 if (order == null)
                 {
                     return Json(new { result = "error", message = RP.S("Member.Error.OrderNotFOund") });
                 }
                 if (order.OrderStatus != OrderStatus.Delivered)
                 {
                     return Json(new { result = "error", message = RP.S("Member.Error.OrderNotDeliveredYet") });
                 }
                 if ((Amount + order.RefundAmount) > (order.Total ))
                     return Json(new { result = "error", message = RP.S("Member.Error.AmountMustBeLessThanOrderTotal") });
                // order.OrderStatus = OrderStatus.PartialRefunded;
                 order.RefundOn = DateTime.Now;
                 order.RefundAmount += Amount;
                 _db.SaveChanges();
                 if (order.PaymentMethod == PaymentMethod.Credit)
                 {
                     ShoppingService.PartialRefundOrder(order);
                 }
                 return Json(new { result = "ok", message = RP.S("Member.OrderChanged.PartialRefoundMaked") });
             }


         [HttpPost]
         public ActionResult OrderDelivered(int ID)
         {
             if (CurrentShop == null) return Json(new { result = "error", message = RP.S("Member.Error.ShopNotFound") });
             var order = _db.Orders.FirstOrDefault(x => x.ID == ID && x.ShopID == CurrentShop.ID);
             if (order == null)
             {
                 return Json(new { result = "error", message = RP.S("Member.Error.OrderNotFound") });
             }
             if (order.OrderStatus != OrderStatus.Sent)
             {
                 return Json(new { result = "error", message = RP.S("Member.Error.YouMustAcceptOrderFirst") });
             }
             order.OrderStatus = OrderStatus.Delivered;//ShoppingService.DeliveredOrderStatus();//canceled
             _db.SaveChanges();
             return Json(new { result = "ok", message = RP.S("Member.OrderChanged.OrderMarkedAsDelivered") });
         }

              
        #endregion

        #region lang skin

         [AcceptVerbs(HttpVerbs.Post)]
         public ActionResult ChangeShop(int ShopSelectList)
         {
             if (ShopSelectList > 0) Session["CurrentShopID"] = ShopSelectList;
             return Redirect(Request.UrlReferrer.ToString());
         }

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
    }
}

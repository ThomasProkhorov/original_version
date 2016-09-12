using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Uco.Infrastructure.Discounts;
using Uco.Infrastructure.Livecycle;
using Uco.Infrastructure.Repositories;
using Uco.Models;
using Uco.Models.Overview;

namespace Uco.Infrastructure.Services
{
    public class ShoppingCartService
    {
        public ShoppingCartService(Db _db = null)
        {
            _Context = new DBContextService(_db);
        }
        private DBContextService _Context = null;
        private Db _db
        {
            get
            {
                if (_Context == null) { return null; }
                return _Context.EntityContext;
            }
        }
        public static int GetFirstShopID()
        {
            var sci = LS.CurrentEntityContext.ShoppingCartItems.FirstOrDefault(x => x.UserID == LS.CurrentUser.ID);
            if (sci != null)
            {
                return sci.ShopID;
            }
            return 0;
        }
        public static void ClearCart(Guid UserID, int shopID
           )
        {
            var items = LS.CurrentEntityContext.ShoppingCartItems.Where(x => x.UserID == UserID && x.ShopID == shopID).ToList();
            LS.CurrentEntityContext.ShoppingCartItems.RemoveRange(items);
            LS.CurrentEntityContext.SaveChanges();
        }
        public static AddToCartModel AddToCart(Guid UserID, ShoppingCartItem item
            , decimal? Quantity = null
            , decimal? QuantityBit = null
            , bool? Delete = null
            , int? Attribute = null
            , int? qtype = null,
            bool isMobile = false)
        {

            List<string> Errors = new List<string>();
            if (item.ProductShopID < 1 && item.ID == 0)
            {
                Errors.Add(RP.S("ShoppingCart.AddToCart.Error.DataIssuse"));
                return new AddToCartModel()
                {
                    errors = Errors,
                    item = item
                };
                // return Json(new { result = "error", message = "Data issuse", data = item });
            }
            bool changeOriginalQuantity = true;
            //if (!LS.isLogined()) //Not actual here
            //{
            //    return Json(new { result = "error", action = "login", message = "You must login first", data = item });
            //}


            if (item.ID > 0)
            {
                item = LS.CurrentEntityContext.ShoppingCartItems.Where(x => x.ID == item.ID
             ).FirstOrDefault();
                // changeOriginalQuantity = false;
                if (item == null)
                {
                    Errors.Add(RP.S("ShoppingCart.AddToCart.Error.ItemNotFound"));
                    return new AddToCartModel()
                    {
                        errors = Errors,
                        item = item
                    };
                }
                if (Attribute.HasValue)
                {
                    item.ProductAttributeOptionID = Attribute.Value;
                }
            }




            //if (item.Quantity <= 0)
            //{
            //    Errors.Add(RP.S("ShoppingCart.AddToCart.Error.QuantityLessThan1"));
            //    return new AddToCartModel()
            //    {
            //        errors = Errors,
            //        item = item
            //    };
            //    //  return Json(new { result = "error", message = "Quantity can`t be less then 1", data = item });
            //}
            var maxQuantity = decimal.MaxValue;
            var productShop = LS.CurrentEntityContext.ProductShopMap.FirstOrDefault(x => x.ID == item.ProductShopID);
            if (productShop == null)
            {
                Errors.Add(RP.S("ShoppingCart.AddToCart.Error.ProductNotFound"));
                return new AddToCartModel()
                {
                    errors = Errors,
                    item = item
                };
                // return Json(new { result = "error", message = "product not found", data = item });

            }
            if (productShop.QuantityType == ProductQuantityType.CheckByProduct)
            {
                maxQuantity = productShop.Quantity;
            }
            if (item.ProductAttributeOptionID == 0)
            {
                if (LS.CurrentEntityContext.ProductAttributeOptions.Any(x => x.ProductShopID == item.ProductShopID))
                {
                    Errors.Add(RP.S("ShoppingCart.AddToCart.Error.OptionNotSelected"));
                    return new AddToCartModel()
                    {
                        errors = Errors,
                        item = item
                    };
                    //return Json(new { result = "error", message = "Need select option", data = item });
                }
                if (productShop.Price <= 0)
                {
                    Errors.Add(RP.S("ShoppingCart.AddToCart.Error.ZeroPrice"));
                    return new AddToCartModel()
                    {
                        errors = Errors,
                        item = item
                    };
                    // return Json(new { result = "error", message = "You can`t add product with zero price", data = item });
                }
            }
            else
            {
                var option = LS.CurrentEntityContext.ProductAttributeOptions.FirstOrDefault(x => x.ProductShopID == item.ProductShopID && x.ID == item.ProductAttributeOptionID);
                if (option == null)
                {
                    Errors.Add(RP.S("ShoppingCart.AddToCart.Error.OptionNotFoud"));
                    return new AddToCartModel()
                    {
                        errors = Errors,
                        item = item
                    };
                    //return Json(new { result = "error", message = "Option not exists", data = item });

                }

                if ((option.OverridenPrice.HasValue && option.OverridenPrice <= 0))
                {
                    Errors.Add(RP.S("ShoppingCart.AddToCart.Error.ZeroPrice"));
                    return new AddToCartModel()
                    {
                        errors = Errors,
                        item = item
                    };
                    //   return Json(new { result = "error", message = "You can`t add product with zero price", data = item });
                }
                if (!option.OverridenPrice.HasValue
                    && productShop.Price <= 0)
                {
                    Errors.Add(RP.S("ShoppingCart.AddToCart.Error.ZeroPrice"));
                    return new AddToCartModel()
                    {
                        errors = Errors,
                        item = item
                    };
                    //   return Json(new { result = "error", message = "You can`t add product with zero price", data = item });
                }
                if (productShop.QuantityType == ProductQuantityType.CheckByProductOptions)
                {
                    maxQuantity = option.Quantity;
                }
            }


            //check existing
            if (maxQuantity == 0)
            {
                Errors.Add(RP.S("ShoppingCart.AddToCart.Error.NoEnoughQuantity"));
                return new AddToCartModel()
                {
                    errors = Errors,
                    item = item
                };
                // return Json(new { result = "error", message = "No quantity enough", data = item });
            }
            ShoppingCartItem existing = null;
            if (item.ID > 0)
            {
                existing = item;
            }
            else
            {
                existing = LS.CurrentEntityContext.ShoppingCartItems.Where(x => x.UserID == UserID
                   && x.ProductShopID == item.ProductShopID
                   && x.ProductAttributeOptionID == item.ProductAttributeOptionID).FirstOrDefault();
            }

            if (existing != null)
            {

                if (QuantityBit.HasValue)
                {
                    if (existing.QuantityType == QuantityType.ByUnit)
                    {
                        if (QuantityBit.Value > 0)
                        {
                            QuantityBit = (decimal)Math.Ceiling(QuantityBit.Value);
                        }
                        else
                        {
                            QuantityBit = (decimal)Math.Floor(QuantityBit.Value);
                        }
                    }
                    existing.Quantity += QuantityBit.Value;
                }
                else
                    if (Quantity.HasValue)
                    {

                        if (existing.Quantity.ToString("0.000") == Quantity.Value.ToString("0.000") && !qtype.HasValue)
                        {
                            existing.Quantity += 1;
                        }
                        else
                        {
                            existing.Quantity = Quantity.Value;
                        }
                    }
                    else if (changeOriginalQuantity)
                    {

                        if (existing.Quantity.ToString("0.000") == item.Quantity.ToString("0.000") && !qtype.HasValue)
                        {
                            existing.Quantity += 1;
                        }
                        else
                        {
                            existing.Quantity = item.Quantity;
                        }


                    }


                if (existing.Quantity > maxQuantity)
                {
                    Errors.Add(RP.S("ShoppingCart.AddToCart.Error.NoQuantityEnought-StringParamMaxQuantity"));
                    return new AddToCartModel()
                    {
                        errors = Errors,
                        item = item
                    };
                    // return Json(new { result = "error", message = "No quantity enough, max is " + maxQuantity.ToString(), data = item });
                }
                if (qtype.HasValue)
                {
                    existing.QuantityType = (QuantityType)qtype.Value;
                    if (existing.QuantityType == QuantityType.ByUnit)
                    {
                        existing.Quantity = Math.Ceiling(existing.Quantity);
                    }
                }
                if ((Delete.HasValue && Delete.Value) || existing.Quantity <= 0)
                {
                    existing.Quantity = 0;
                    LS.CurrentEntityContext.ShoppingCartItems.Remove(existing);
                }
                else
                {
                    UserActivityService.InsertAddToCart(UserID, item.ProductShopID, item.Quantity
                        , item.ShopID, item.ProductAttributeOptionID
                     , HttpContext.Current.Request.RawUrl,
                     HttpContext.Current.Request.UrlReferrer != null ? HttpContext.Current.Request.UrlReferrer.OriginalString : null,
                     LS.GetUser_IP(HttpContext.Current.Request));
                }
                LS.CurrentEntityContext.SaveChanges();
                item = existing;
            }
            else
            {
                item.UserID = UserID;
                if (item.Quantity > maxQuantity)
                {
                    Errors.Add(RP.S("ShoppingCart.AddToCart.Error.NoQuantityEnought-StringParamMaxQuantity"));
                    return new AddToCartModel()
                    {
                        errors = Errors,
                        item = item
                    };
                    //return Json(new { result = "error", message = "No quantity enough, max is " + maxQuantity.ToString(), data = item });
                }
                UserActivityService.InsertAddToCart(UserID, item.ProductShopID, item.Quantity
                    , item.ShopID, item.ProductAttributeOptionID
                 , HttpContext.Current.Request.RawUrl,
                 HttpContext.Current.Request.UrlReferrer != null ? HttpContext.Current.Request.UrlReferrer.OriginalString : null
                 , LS.GetUser_IP(HttpContext.Current.Request));
                LS.CurrentEntityContext.ShoppingCartItems.Add(item);
                LS.CurrentEntityContext.SaveChanges();
            }
            if (productShop.PriceByUnit.HasValue && productShop.PriceByUnit.Value > 0)
            {
                item.MeasureUnit = "doNotUseIt";
            }
            return new AddToCartModel()
            {
                errors = Errors,
                item = item
            };


            //check existing - old version
            //var existing = LS.CurrentEntityContext.ShoppingCartItems.Where(x => x.UserID == UserID
            //    && x.ProductShopID == item.ProductShopID
            //    && x.ProductAttributeOptionID == item.ProductAttributeOptionID).FirstOrDefault();
            //if (existing != null)
            //{
            //    existing.Quantity += item.Quantity;
            //    LS.CurrentEntityContext.SaveChanges();
            //    item = existing;
            //}
            //else
            //{
            //    item.UserID = UserID;
            //    LS.CurrentEntityContext.ShoppingCartItems.Add(item);
            //    LS.CurrentEntityContext.SaveChanges();
            //}
        }
        public static ShoppingCartItemModel GetShoppingCartItemByID(int ID)
        {

            var item = (from sci in LS.CurrentEntityContext.ShoppingCartItems
                        join ps in LS.CurrentEntityContext.ProductShopMap
                        on sci.ProductShopID equals ps.ID
                        join p in LS.CurrentEntityContext.Products
                        on ps.ProductID equals p.ID
                        where sci.UserID == LS.CurrentUser.ID
                        && sci.ID == ID
                        //join psao in LS.CurrentEntityContext.ProductAttributeOptions
                        //on sci.ProductAttributeOptionID equals psao.ID
                        select new ShoppingCartItemModel()
                        {
                            ID = sci.ID,
                            Name = p.Name,
                            Price = ps.Price,
                            SKU = p.SKU,
                            Image = p.Image,
                            ProductShopID = ps.ID,
                            Quantity = sci.Quantity,
                            ShopID = ps.ShopID,
                            UnitPrice = ps.Price * sci.Quantity,
                            ProductAttributeOptionID = sci.ProductAttributeOptionID,
                            AttributeDescription = "",
                            CategoryID = p.CategoryID,
                            MeasureUnit = p.MeasureUnit,
                            MeasureUnitStep = p.MeasureUnitStep,
                        }).FirstOrDefault();
            if (item != null)
            {

                if (item.ProductAttributeOptionID > 0)
                {
                    var attribute = (from pao in LS.CurrentEntityContext.ProductAttributeOptions
                                     //  join pa in LS.CurrentEntityContext.ProductAttributes
                                     //on pao.ProductAttributeID equals pa.ID
                                     where pao.ID == item.ProductAttributeOptionID
                                     select new ProductAttributeOptionModel()
                                     {
                                         ID = pao.ID,
                                         Name = pao.Name,
                                         OverridenPrice = pao.OverridenPrice,
                                         OverridenSku = pao.OverridenSku,
                                         // ProductAttributeID = pao.ProductAttributeID,
                                         ProductShopID = pao.ProductShopID,
                                         Quantity = pao.Quantity,
                                         // ProductAttribute = pa.Name

                                     }).FirstOrDefault();
                    if (attribute != null)
                    {
                        item.AttributeDescription = attribute.ProductAttribute + ": " + attribute.Name;
                        if (!string.IsNullOrEmpty(attribute.OverridenSku))
                        {
                            item.SKU = attribute.OverridenSku;
                        }
                        if (attribute.OverridenPrice.HasValue)
                        {
                            item.Price = attribute.OverridenPrice.Value;
                            item.UnitPrice = item.Price * item.Quantity;
                        }
                    }
                }
                item.PriceStr = ShoppingService.FormatPrice(item.Price);
                item.UnitPriceStr = ShoppingService.FormatPrice(item.UnitPrice);
            }
            return item;
        }
        public static IList<ShoppingCartItemModel> GetShoppingCartItemsByList(int ShopID, IList<ShoppingCartItemModel> shoppingcartItems, bool loadAttributes = false)
        {
            var needed = shoppingcartItems.Select(x => x.ProductID);
            var parentDict = shoppingcartItems.ToList();
            var data = (from ps in LS.CurrentEntityContext.ProductShopMap
                        join p in LS.CurrentEntityContext.Products
                        on ps.ProductID equals p.ID
                        where ps.ShopID == ShopID
                        && needed.Contains(p.ID)
                        //join psao in LS.CurrentEntityContext.ProductAttributeOptions
                        //on sci.ProductAttributeOptionID equals psao.ID
                        select new ShoppingCartItemModel()
                        {
                            ID = 0,
                            Name = p.Name,
                            Price = ps.Price,
                            SKU = p.SKU,
                            Image = p.Image,
                            ProductID = p.ID,
                            ProductShopID = ps.ID,
                            QuantityResource = ps.Quantity,
                            QuantityType = ps.QuantityType,
                            // Quantity = 0,
                            ShopID = ps.ShopID,
                            // UnitPrice = ps.Price * 0,
                            // ProductAttributeOptionID = sci.ProductAttributeOptionID,
                            AttributeDescription = "",
                            CategoryID = p.CategoryID,
                            MeasureUnit = p.MeasureUnit,
                            MeasureUnitStep = p.MeasureUnitStep,
                        }).ToList();
            foreach (var pitem in parentDict)
            {
                var item = data.FirstOrDefault(x => x.ProductID == pitem.ProductID);
                if (item == null)
                {
                    item = new ShoppingCartItemModel();
                    item.AttributeDescription = pitem.AttributeDescription;
                    item.Image = pitem.Image;
                    item.Name = pitem.Name;
                    item.Price = pitem.Price;
                    item.ProductID = pitem.ProductID;
                    item.ProductShopID = pitem.ProductShopID;
                    item.QuantityType = pitem.QuantityType;
                    item.ShopID = pitem.ShopID;
                    item.SKU = pitem.SKU;
                    item.UnitPrice = pitem.UnitPrice;
                    item.IsNotAvaliable = true;//mark as not avaliable
                    data.Add(item);
                }
                item.Quantity = pitem.Quantity;
                if (pitem.ProductAttributeOptionID > 0)
                {
                    var attributes = (from pao in LS.CurrentEntityContext.ProductAttributeOptions
                                      //  join pa in LS.CurrentEntityContext.ProductAttributes
                                      // on pao.ProductAttributeID equals pa.ID
                                      where pao.ProductShopID == item.ProductShopID
                                      select new ProductAttributeOptionModel()
                                      {
                                          ID = pao.ID,
                                          Name = pao.Name,
                                          OverridenPrice = pao.OverridenPrice,
                                          OverridenSku = pao.OverridenSku,
                                          //  ProductAttributeID = pao.ProductAttributeID,
                                          ProductShopID = pao.ProductShopID,
                                          Quantity = pao.Quantity,
                                          // ProductAttribute = pa.Name

                                      }).ToList();
                    item.Attributes = attributes;

                    var attribute = attributes.FirstOrDefault(x => x.Name == pitem.AttributeDescription);
                    if (attribute != null)
                    {
                        item.AttributeDescription = attribute.ProductAttribute + ": " + attribute.Name;
                        item.ProductAttributeOptionID = attribute.ID;
                        if (item.QuantityType == ProductQuantityType.CheckByProductOptions)
                        {
                            item.QuantityResource = attribute.Quantity;
                        }
                        if (!string.IsNullOrEmpty(attribute.OverridenSku))
                        {
                            item.SKU = attribute.OverridenSku;
                        }
                        if (attribute.OverridenPrice.HasValue)
                        {
                            item.Price = attribute.OverridenPrice.Value;
                            item.UnitPrice = item.Price * item.Quantity;
                        }
                    }
                    else
                    {
                        item.SelectedAttributeNotAvaliable = true;//mark as not avaliable
                    }
                }
                if ((item.QuantityType == ProductQuantityType.CheckByProduct
                    || item.QuantityType == ProductQuantityType.CheckByProductOptions
                    ) && item.QuantityResource < item.Quantity)
                {
                    item.IsHaveNotQuantity = true;
                }
                item.UnitPrice = item.Price * item.Quantity;
                item.PriceStr = ShoppingService.FormatPrice(item.Price);
                item.UnitPriceStr = ShoppingService.FormatPrice(item.UnitPrice);
            }
            return data;
        }
        public ShoppingCartOverviewModel GetShoppingCartModel(int ShopID
            , bool loadattributes = true
            , bool withship = false
            , bool withdiscount = true
            , bool feutured = false
            , bool loadworktimes = false
            , bool nocache = false
            , bool checkQuantity = false
            , bool loadComments = false
            , Guid UserID = new Guid())
        {
            if (UserID == Guid.Empty && LS.isHaveID())
            {
                UserID = LS.CurrentUser.ID;
            }
            var model = new ShoppingCartOverviewModel();
            List<SpecificationOptionModel> specifications = null;
            model.ShopID = ShopID;
            var shop = LS.Get<Shop>().FirstOrDefault(x => x.ID == ShopID);
            if (shop == null) { return model; }
            model.Shop = shop;
            model.IsShipEnabled = shop.IsShipEnabled;
            model.InStorePickUpEnabled = shop.InStorePickUpEnabled;
            if (LS.isHaveID())
            {
                model.Items = this.GetShoppingCartItems(ShopID, loadattributes, withdiscount: withdiscount, checkQuantity: checkQuantity, UserID: UserID);

                model.IsLogined = true;
                if (withdiscount)
                {
                    // process discounts

                    var discountService = new DiscountService(_db, nocache);
                    discountService.ProcessItems(model, LS.CurrentUser);
                }
                //fix and format prices
                foreach (var item in model.Items)
                {
                    item.UnitPrice = item.Price * item.Quantity - item.TotalDiscountAmount;
                    if (item.UnitPrice < 0) { item.UnitPrice = 0; }
                    string ustep = "1";
                    if (item.MeasureUnitStep.HasValue)
                    {
                        //item.UnitPrice = item.UnitPrice / item.MeasureUnitStep.Value;
                        // ustep = item.MeasureUnitStep.Value.ToString();
                    }

                    item.PriceStr = ShoppingService.FormatPrice(item.Price) + (item.MeasureUnit != null ? " / " + item.MeasureUnit : "");
                    item.UnitPriceStr = ShoppingService.FormatPrice(item.UnitPrice);
                    //+ ( item.TotalDiscountAmount > 0
                    //   ? " (-" + ShoppingService.FormatPrice(item.TotalDiscountAmount) + ")"
                    // : "");
                    if (false && loadComments)
                    {
                        item.Comments = ShoppingService.GetUserNoteForProduct(item.ProductID, LS.CurrentUser.ID);
                    }
                }

                model.FreeShipFrom = shop.FreeShipFrom;
                model.ShopShipCost = shop.ShipCost;

                var deliveryZones = LS.Get<ShopDeliveryZone>().Where(x => x.ShopID == ShopID && x.Active).ToList();
                if (deliveryZones.Count > 0)
                {
                    //searching by couples zones
                    var checkoutData = ShoppingCartService.GetCheckoutData(UserID);
                    var address = "";
                    decimal longitude = 0;
                    decimal latitude = 0;
                    if (LS.CurrentUser.Latitude != 0)
                    {
                        latitude = LS.CurrentUser.Latitude;
                    }
                    if (LS.CurrentUser.Longitude != 0)
                    {
                        longitude = LS.CurrentUser.Longitude;
                    }
                    if (LS.CurrentHttpContext != null)
                    {
                        //if not regognized
                        if (longitude == 0)
                        {
                            if (LS.CurrentHttpContext.Session["longitude"] != null)
                            {
                                longitude = (decimal)LS.CurrentHttpContext.Session["longitude"];
                            }
                        }
                        if (latitude == 0)
                        {
                            if (LS.CurrentHttpContext.Session["latitude"] != null)
                            {
                                latitude = (decimal)LS.CurrentHttpContext.Session["latitude"];
                            }
                        }
                    }
                    if (LS.CurrentHttpContext != null && LS.CurrentHttpContext.Session["address"] != null)
                    {
                        address = (string)LS.CurrentHttpContext.Session["address"];
                    }
                    else
                    {
                        address = checkoutData.Address;

                    }
                    if (address == null)
                    {
                        address = "";
                    }
                    // find by address




                    var lowerCaseAddress = address.ToLower();

                    List<DeliveryZoneSmall> allPossible = new List<DeliveryZoneSmall>();
                    //add default shop zone
                    var clientDefaultradius = ShoppingService.distance((double)shop.RadiusLatitude, (double)shop.RadiusLongitude
                        , (double)latitude, (double)longitude, 'K');
                    if (clientDefaultradius <= (double)shop.ShipRadius)
                    {
                        allPossible.Add(new DeliveryZoneSmall()
                        {
                            ShipCost = shop.ShipCost
                            ,
                            FreeShipFrom = shop.FreeShipFrom,
                            Distance = clientDefaultradius
                        });
                    }
                    foreach (var dz in deliveryZones)
                    {
                        if (dz.DeliveryFroAllCity && !string.IsNullOrEmpty(dz.City) &&
                        (
                        lowerCaseAddress.Contains(", " + dz.City.ToLower() + ", ")
                        || lowerCaseAddress.StartsWith(dz.City.ToLower() + ", ")
                            // || lowerCaseAddress.EndsWith(", " + s.City.ToLower())
                            // || lowerCaseAddress.StartsWith(s.City.ToLower() + " ")
                        || lowerCaseAddress.EndsWith(", " + dz.City.ToLower())
                        )
                        )
                        {
                            allPossible.Add(new DeliveryZoneSmall() { ShipCost = dz.ShipCost, FreeShipFrom = dz.FreeShipFrom, Distance = 1 });
                        }
                        else if (!dz.DeliveryFroAllCity)
                        {

                            var clientradius = ShoppingService.distance((double)dz.RadiusLatitude, (double)dz.RadiusLongitude, (double)latitude, (double)longitude, 'K');
                            if (clientradius <= (double)dz.ShipRadius)
                            {
                                allPossible.Add(new DeliveryZoneSmall()
                                {
                                    ShipCost = dz.ShipCost
                                    ,
                                    FreeShipFrom = dz.FreeShipFrom,
                                    Distance = clientradius
                                });
                            }
                        }
                    }
                    if (allPossible.Count > 0)
                    {
                        var firstBetter = allPossible.OrderBy(x => x.Distance).FirstOrDefault();
                        model.FreeShipFrom = firstBetter.FreeShipFrom;
                        model.ShopShipCost = firstBetter.ShipCost;

                    }
                }

                if (model.Items.Count > 0)
                {
                    model.SubTotal = model.Items.Count > 0 ? model.Items.Sum(x => x.UnitPrice) : 0;
                    model.TotalWithoutShip = model.SubTotal;
                    if (withship && model.SubTotal < model.FreeShipFrom)
                    {
                        model.ShippingCost = model.ShopShipCost;

                    }
                    model.Total = model.SubTotal + model.ShippingCost + model.Fee;
                    model.Count = model.Items.Count;
                }
            }



            if (withdiscount)
            {
                // process total discounts

                var discountService = new DiscountService(_db, nocache);
                discountService.ProcessTotals(model, LS.CurrentUser);
            }
            //if (LS.isLogined())
            //{
            //    if (model.Items.Count > 0)
            //    {
            //        model.TotalWithoutShip = model.SubTotal;
            //        if (withship && model.SubTotal < shop.FreeShipFrom)
            //        {
            //            if (model.ShippingCost > 0)
            //            {
            //                model.ShippingCost = shop.ShipCost;
            //            }
            //        }


            //    }
            //}
            model.SubTotalStr = ShoppingService.FormatPrice(model.SubTotal);
            model.ShippingCostStr = ShoppingService.FormatPrice(model.ShippingCost);
            model.TotalWithoutShipStr = ShoppingService.FormatPrice(model.TotalWithoutShip);

            model.FeeStr = ShoppingService.FormatPrice(model.Fee);
            model.TotalStr = ShoppingService.FormatPrice(model.Total);
            model.TotalDiscountStr = ShoppingService.FormatPrice(model.TotalDiscount);
            //work and ship times
            if (loadworktimes)
            {
                var curdate = DateTime.Now.Date;
                var lastdate = curdate.AddDays(7);
                model.ShipTimes = _db.ShopShipTimes.Where(x => x.ShopID == ShopID && x.Active &&
                    (!x.IsSpecial || (x.Date >= curdate && x.Date <= lastdate)
                    )
                    )
                    .OrderBy(x => x.IsSpecial).ThenBy(x => x.Day).ThenBy(x => x.Date)
                   .Select(x => new ShipTimeModel()
                   {
                       Date = x.Date,
                       Day = x.Day,
                       TimeFromInt = x.TimeFrom,
                       TimeToInt = x.TimeTo,
                       IsSpecial = x.IsSpecial
                   })
                   .ToList();
                var culture = new System.Globalization.CultureInfo("he-IL");
                foreach (var t in model.ShipTimes)
                {
                    t.DayStr = t.DayStr = culture.DateTimeFormat.GetDayName(t.Day);
                    t.TimeFromeStr = TimeSpan.FromMinutes(t.TimeFromInt).ToString("hh':'mm");
                    t.TimeToStr = TimeSpan.FromMinutes(t.TimeToInt).ToString("hh':'mm");

                }

                model.WorkTimes = _db.ShopWorkTimes.Where(x => x.ShopID == ShopID && x.Active &&
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
                    //t.DayStr = t.Day.ToString();
                    //t.TimeFromeStr = IntToTime(t.TimeFromInt);
                    //t.TimeToStr = IntToTime(t.TimeToInt);
                    //t.DateStr = t.Date.ToString("dd/MM");
                    t.DayStr = culture.DateTimeFormat.GetDayName(t.Day);
                    t.TimeFromeStr = TimeSpan.FromMinutes(t.TimeFromInt).ToString("hh':'mm");
                    t.TimeToStr = TimeSpan.FromMinutes(t.TimeToInt).ToString("hh':'mm");
                    t.DateStr = t.Date.ToString("dd/MM");

                }
            }

            if (feutured)
            {
                var exceptProductShopIds = model.Items.Select(x => x.ProductShopID).ToList();
                model.FeaturedProducts = LS.SearchProducts(ShopID, out specifications, featuredLeft: true, excludeProductShopList: exceptProductShopIds, limit: 10).ToList();
                if (model.FeaturedProducts.Count < 10)
                {
                    int limitLeft = 10 - model.FeaturedProducts.Count;
                    var ordered = LS.SearchProducts(ShopID, out specifications, allOrderedProducts: true, excludeProductShopList: exceptProductShopIds, limit: limitLeft).ToList();
                    foreach (var p in ordered)
                    {
                        if (!model.FeaturedProducts.Any(x => x.ProductShopID == p.ProductShopID))
                        {
                            model.FeaturedProducts.Add(p);
                        }
                    }
                }
                if (model.FeaturedProducts.Count < 10)
                {
                    int limitLeft = 10 - model.FeaturedProducts.Count;
                    var catId = model.Items.Select(x => x.CategoryID).ToList();
                    if (catId.Count > 0)
                    {
                        var ordered = LS.SearchProducts(ShopID, out specifications, inCategories: catId, excludeProductShopList: exceptProductShopIds, limit: limitLeft).ToList();
                        foreach (var p in ordered)
                        {
                            if (!model.FeaturedProducts.Any(x => x.ProductShopID == p.ProductShopID))
                            {
                                model.FeaturedProducts.Add(p);
                            }
                        }
                    }
                }
            }
            return model;
        }
        public static int SwitchShoppingCart(Guid FromUserID, Guid ToUserID)
        {
            if (Guid.Empty != FromUserID && Guid.Empty != ToUserID)
            {
                int totalItems = 0;
                var cartItems = LS.CurrentEntityContext.ShoppingCartItems.Where(x => x.UserID == FromUserID).ToList();
                var toCartItems = LS.CurrentEntityContext.ShoppingCartItems.Where(x => x.UserID == ToUserID).ToList();
                foreach (var sci in cartItems)
                {
                    sci.UserID = ToUserID;

                    // totalItems++;
                }
                foreach (var sci in toCartItems)
                {
                    sci.UserID = FromUserID;

                    totalItems++;
                }
                LS.CurrentEntityContext.SaveChanges();
                return totalItems;
            }
            return -1;
        }
        public static int MigrateShoppingCart(Guid FromUserID, Guid ToUserID)
        {
            if (Guid.Empty != FromUserID && Guid.Empty != ToUserID)
            {
                int totalItems = 0;
                var cartItems = LS.CurrentEntityContext.ShoppingCartItems.Where(x => x.UserID == FromUserID).ToList();
                var toCartItems = LS.CurrentEntityContext.ShoppingCartItems.Where(x => x.UserID == ToUserID).ToList();
                foreach (var sci in cartItems)
                {
                    var update = toCartItems.FirstOrDefault(x => x.ShopID == sci.ShopID
                        && x.ProductShopID == sci.ProductShopID
                        && x.ProductAttributeOptionID == sci.ProductAttributeOptionID

                        );
                    if (update != null)
                    {
                        update.Quantity += sci.Quantity;

                        LS.CurrentEntityContext.ShoppingCartItems.Remove(sci);
                    }
                    else
                    {
                        sci.UserID = ToUserID;

                    }
                    totalItems++;
                }
                LS.CurrentEntityContext.SaveChanges();
                return totalItems;
            }
            return -1;
        }
        public IList<ShoppingCartItemModel> GetShoppingCartItems(int ShopID, bool loadAttributes = false
            , bool withdiscount = true
            , bool checkQuantity = false
            , Guid UserID = new Guid())
        {
            if (UserID == Guid.Empty && LS.isHaveID())
            {
                UserID = LS.CurrentUser.ID;
            }
            Dictionary<int, decimal> totalResources = new Dictionary<int, decimal>();
            var data = (from sci in _db.ShoppingCartItems
                        join ps in _db.ProductShopMap
                        on sci.ProductShopID equals ps.ID
                        join p in _db.Products
                        on ps.ProductID equals p.ID
                        where sci.UserID == UserID
                        && sci.ShopID == ShopID
                        //join psao in LS.CurrentEntityContext.ProductAttributeOptions
                        //on sci.ProductAttributeOptionID equals psao.ID
                        select new ShoppingCartItemModel()
                        {
                            ID = sci.ID,
                            Name = p.Name,
                            Price = ps.Price,
                            PriceByUnit = ps.PriceByUnit,
                            SKU = p.SKU,
                            Image = p.Image,
                            ProductID = p.ID,
                            //  Manufacturer = p.Manufacturer,
                            ProductManufacturerID = p.ProductManufacturerID,
                            ProductShopID = ps.ID,
                            Quantity = sci.Quantity,
                            QuantityResource = ps.Quantity,
                            QuantityType = ps.QuantityType,
                            QuantityPriceType = sci.QuantityType,
                            ShopID = ps.ShopID,
                            SoldByWeight = p.SoldByWeight,
                            Capacity = p.Capacity,
                            UnitPrice = ps.Price * sci.Quantity,
                            ProductAttributeOptionID = sci.ProductAttributeOptionID,
                            AttributeDescription = "",
                            CategoryID = p.CategoryID,
                            MeasureUnit = p.MeasureUnit,
                            MeasureUnitStep = p.MeasureUnitStep,
                        }).ToList();

            foreach (var item in data)
            {
                if (item.ProductManufacturerID > 0)
                {
                    var m = LS.Get<Manufacturer>().FirstOrDefault(x => x.ID == item.ProductManufacturerID);
                    item.Manufacturer = m != null ? m.Name : "";
                }
                if (item.QuantityPriceType == QuantityType.ByUnit && item.PriceByUnit.HasValue)
                {
                    item.Price = item.PriceByUnit.Value;
                    item.UnitPrice = item.Price * item.Quantity;
                    item.MeasureUnit = null;

                    item.MeasureUnitStep = null;
                    item.SoldByWeight = false;

                }
                item.Errors = new List<string>();
                if (false && loadAttributes)
                {
                    var attributes = (from pao in _db.ProductAttributeOptions
                                      //  join pa in LS.CurrentEntityContext.ProductAttributes
                                      // on pao.ProductAttributeID equals pa.ID
                                      where pao.ProductShopID == item.ProductShopID
                                      select new ProductAttributeOptionModel()
                                      {
                                          ID = pao.ID,
                                          Name = pao.Name,
                                          OverridenPrice = pao.OverridenPrice,
                                          OverridenSku = pao.OverridenSku,
                                          //  ProductAttributeID = pao.ProductAttributeID,
                                          ProductShopID = pao.ProductShopID,
                                          Quantity = pao.Quantity,
                                          //  ProductAttribute = pa.Name

                                      }).ToList();
                    item.Attributes = attributes;
                    var attribute = attributes.FirstOrDefault(x => x.ID == item.ProductAttributeOptionID);
                    if (attribute != null)
                    {
                        item.AttributeDescription = attribute.Name;
                        if (!string.IsNullOrEmpty(attribute.OverridenSku))
                        {
                            item.SKU = attribute.OverridenSku;
                        }
                        if (attribute.OverridenPrice.HasValue)
                        {
                            item.Price = attribute.OverridenPrice.Value;

                        }
                        if (item.QuantityType == ProductQuantityType.CheckByProductOptions)
                        {
                            item.QuantityResource = attribute.Quantity;
                        }
                    }
                    else if (checkQuantity)
                    {
                        item.Errors.Add(RP.S("ShoppingCart.AddToCart.Error.OptionNotFoud"));

                    }

                }
                else
                {
                    if (item.ProductAttributeOptionID > 0)
                    {
                        var attribute = (from pao in _db.ProductAttributeOptions
                                         // join pa in LS.CurrentEntityContext.ProductAttributes
                                         //on pao.ProductAttributeID equals pa.ID
                                         where pao.ID == item.ProductAttributeOptionID
                                         select new ProductAttributeOptionModel()
                                         {
                                             ID = pao.ID,
                                             Name = pao.Name,
                                             OverridenPrice = pao.OverridenPrice,
                                             OverridenSku = pao.OverridenSku,
                                             //  ProductAttributeID = pao.ProductAttributeID,
                                             ProductShopID = pao.ProductShopID,
                                             Quantity = pao.Quantity,
                                             // ProductAttribute = pa.Name

                                         }).FirstOrDefault();
                        if (attribute != null)
                        {
                            item.AttributeDescription = attribute.ProductAttribute + ": " + attribute.Name;
                            if (!string.IsNullOrEmpty(attribute.OverridenSku))
                            {
                                item.SKU = attribute.OverridenSku;
                            }
                            if (attribute.OverridenPrice.HasValue)
                            {
                                item.Price = attribute.OverridenPrice.Value;

                            }
                            if (item.QuantityType == ProductQuantityType.CheckByProductOptions)
                            {
                                item.QuantityResource = attribute.Quantity;
                            }
                        }
                        else if (checkQuantity)
                        {
                            item.Errors.Add(RP.S("ShoppingCart.AddToCart.Error.OptionNotFoud"));

                        }
                    }
                    else if (false && checkQuantity && LS.CurrentEntityContext.ProductAttributeOptions.Any(x => x.ProductShopID == item.ProductShopID))
                    {
                        item.Errors.Add(RP.S("ShoppingCart.AddToCart.Error.OptionNotSelected"));

                    }
                }
                // check total quantity if different attribute and check by product
                if (checkQuantity && item.QuantityType == ProductQuantityType.CheckByProduct)
                {
                    if (totalResources.ContainsKey(item.ProductShopID))
                    {
                        item.QuantityResource = totalResources[item.ProductShopID];
                        totalResources[item.ProductShopID] -= item.Quantity;
                    }
                    else
                    {
                        totalResources[item.ProductShopID] = item.QuantityResource - item.Quantity;
                    }
                }
                if (checkQuantity && item.QuantityType != ProductQuantityType.NotCheck && item.Quantity > item.QuantityResource)
                {
                    item.Errors.Add(RP.S("ShoppingCart.AddToCart.Error.NoEnoughQuantity"));
                }
                if (item.Price <= 0)
                {
                    item.Errors.Add(RP.S("ShoppingCart.AddToCart.Error.ZeroPrice"));
                }
                if (item.Errors.Count > 0)
                {
                    item.IsValid = false;
                }
            }


            return data;
        }
        public static CheckoutData GetCheckoutData(Guid UserID = new Guid())
        {
            if (LS.CurrentHttpContext != null && LS.CurrentHttpContext.Items["CheckoutData"] != null)
            {
                return LS.CurrentHttpContext.Items["CheckoutData"] as CheckoutData;
            }
            if (UserID == Guid.Empty && LS.isHaveID())
            {
                UserID = LS.CurrentUser.ID;
            }
            if (UserID == Guid.Empty)
            {
                return null;
            }
            var checkoutData = LS.CurrentEntityContext.CheckoutDatas.FirstOrDefault(x => x.UserID == UserID);
            if (checkoutData == null)
            {
                //add to db if not exists
                checkoutData = new CheckoutData()
                {
                    UserID = LS.CurrentUser.ID,
                    ShipTime = DateTime.Now,

                };
                LS.CurrentEntityContext.CheckoutDatas.Add(checkoutData);
                LS.CurrentEntityContext.SaveChanges();
            }
            if (LS.CurrentHttpContext != null)
            {
                LS.CurrentHttpContext.Items["CheckoutData"] = checkoutData;
            }
            return checkoutData;
        }

    }
}
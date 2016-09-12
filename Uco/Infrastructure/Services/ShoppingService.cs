using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Web;
using Uco.Infrastructure.Livecycle;
using Uco.Infrastructure.Repositories;
using Uco.Models;
using System.Linq;
using System.Web.WebPages.Html;
using Uco.Models.Overview;
using Uco.Infrastructure.Services;
using System.Linq.Expressions;
using Uco.Infrastructure.EntityExtensions;

namespace Uco.Infrastructure
{
    public static partial class ShoppingService
    {
        #region orderItems

        //public static int GetOrderItemStatus(string name)
        //{
        //   // var status = LS.Get<OrderItemStatus>().FirstOrDefault(x => x.Name == name);
        //   // if (status != null) return status.ID;
        //    return 0;
        //}


        //public static int NewItemStatus()
        //{
        //    return GetOrderItemStatus("New");
        //}
        //public static int AcceptedItemStatus()
        //{
        //    return GetOrderItemStatus("Accepted");
        //}
        //public static int OutOfStockItemStatus()
        //{
        //    return GetOrderItemStatus("Out of stock");
        //}
        //public static int ApprovedItemStatus()
        //{
        //    return GetOrderItemStatus("Approved by customer");
        //}
        //public static int ChangedItemStatus()
        //{
        //    return GetOrderItemStatus("Changed");
        //}
        //public static int CanceledItemStatus()
        //{
        //    return GetOrderItemStatus("Canceled by customer");
        //}

        #endregion
        public static Shop GetShopByID(int ID)
        {
            return LS.Get<Shop>().FirstOrDefault(x => x.ID == ID);
        }
        #region orders

        public static int GetOrderStatus(string name)
        {
            //var status = LS.Get<OrderStatus>().FirstOrDefault(x => x.Name == name);
            //if (status != null) return status.ID;
            return 0;
        }

        public static int NewOrderStatus()
        {
            return GetOrderStatus("New");
        }
        public static int PaidOrderStatus()
        {
            return GetOrderStatus("Paid");
        }
        public static int AcceptedOrderStatus()
        {
            return GetOrderStatus("Accepted");
        }
        public static int SentOrderStatus()
        {
            return GetOrderStatus("Sent");
        }
        public static int DeliveredOrderStatus()
        {
            return GetOrderStatus("Delivered");
        }
        public static int RejectedOrderStatus()
        {
            return GetOrderStatus("Rejected");
        }
        public static int CanceledOrderStatus()
        {
            return GetOrderStatus("Canceled");
        }
        #endregion

        public static void RefundOrder(Order order)
        {

        }
        public static void PartialRefundOrder(Order order)
        {

        }
        public static IEnumerable<ProductOverviewModel> GetFeaturedProducts(int ShopID = 0)
        {
            List<SpecificationOptionModel> specifications = null;
            return LS.SearchProducts(ShopID, out specifications, featuredLeft: true, limit: 5);
        }
        public static void AddLastSeenProduct(int productShopID)
        {
            var checkoutData = ShoppingCartService.GetCheckoutData();
            List<int> lastProductsIDs = new List<int>();
            if (checkoutData.LastSeenProducts != null)
            {
                lastProductsIDs = checkoutData.LastSeenProducts
                    .Split(new char[] { ' ', ',' }, StringSplitOptions.RemoveEmptyEntries)
                    .Select(x => int.Parse(x)).ToList();
            }

            if (!lastProductsIDs.Contains(productShopID))
            {
                lastProductsIDs.Add(productShopID);
            }
            if (lastProductsIDs.Count > 10)
            {
                lastProductsIDs.RemoveAt(0);
            }
            checkoutData.LastSeenProducts = string.Join(",", lastProductsIDs.Select(n => n.ToString()).ToArray());
            checkoutData.LastAction = DateTime.UtcNow;
            LS.CurrentEntityContext.SaveChanges();
        }
        public static string FormatPrice(decimal price)
        {
            return price.ToString("₪0.00");
        }
        public static string FormatPrice2(decimal? price)
        {
            if (price.HasValue) { return FormatPrice(price.Value); }
            return FormatPrice(0m);
        }
        public static List<ProductShortModel> GetProductAutocomplete(int ShopID, string text)
        {
            var result = new List<ProductShortModel>();

            var sett = RP.GetCurrentSettings();

            if (sett != null && sett.AutocompleteOptions != null)
            {
                result.AddRange(
                    sett.AutocompleteOptions.Split(new char[] { '\n' }).Where(x => x.ToLower().Contains(text.ToLower())).Select(x => new ProductShortModel()
                    {
                        Name = x,
                        ID = 0
                    })
                    );
            }

            #region Improving Search
            // "ת","ה","ות" - woman variants


            var source = LS.CurrentEntityContext.Products.AsNoTracking().AsQueryable();


            List<List<string>> searchAndOrList = new List<List<string>>();
            var words = text.Split(new char[] { ' ', ',', '\n' }, StringSplitOptions.RemoveEmptyEntries).ToList();
            var countWords = words.Count;
            for (var i = 0; i < countWords; i++)
            {
                var listor = new List<string>();
                listor.Add(words[i]);
                searchAndOrList.Add(listor);
            }

            for (var i = 0; i < countWords; i++)
            {
                var word = words[i];
                //1)
                #region woman
                if (word.EndsWith("ת"))
                {
                    var variant1 = word.Remove(word.Length - 1, 1) + "ה";
                    var variant2 = word.Remove(word.Length - 1, 1) + "ות";
                    searchAndOrList[i].Add(variant1);
                    searchAndOrList[i].Add(variant2);
                    continue;
                }
                else if (word.EndsWith("ה"))
                {
                    var variant1 = word.Remove(word.Length - 1, 1) + "ת";
                    var variant2 = word.Remove(word.Length - 1, 1) + "ות";
                    searchAndOrList[i].Add(variant1);
                    searchAndOrList[i].Add(variant2);
                    continue;
                }
                else if (word.EndsWith("ות"))
                {
                    var variant1 = word.Remove(word.Length - 1, 1) + "ה";
                    var variant2 = word.Remove(word.Length - 2, 2) + "ת";
                    searchAndOrList[i].Add(variant1);
                    searchAndOrList[i].Add(variant2);
                    continue;
                }
                #endregion

                #region man couple
                // "ים"
                if (word.EndsWith("ים"))
                {
                    var variant1 = word.Remove(word.Length - 2, 2);
                    //replace
                    //"פ" - "ף"
                    //"נ" - "ן"
                    //"מ" - "ם"
                    // "כ" - "ך"
                    // "צ" - "ץ"
                    if (variant1.EndsWith("פ"))
                    {
                        variant1 = variant1.Remove(variant1.Length - 1, 1) + "ף";
                    }
                    else if (variant1.EndsWith("נ"))
                    {
                        variant1 = variant1.Remove(variant1.Length - 1, 1) + "ן";
                    }
                    else if (variant1.EndsWith("מ"))
                    {
                        variant1 = variant1.Remove(variant1.Length - 1, 1) + "ם";
                    }
                    else if (variant1.EndsWith("כ"))
                    {
                        variant1 = variant1.Remove(variant1.Length - 1, 1) + "ך";
                    }
                    else if (variant1.EndsWith("צ"))
                    {
                        variant1 = variant1.Remove(variant1.Length - 1, 1) + "ץ";
                    }

                    searchAndOrList[i].Add(variant1);
                    continue;
                }
                #endregion

                #region man couple 2

                //"פ" - "ף"
                //"נ" - "ן"
                //"מ" - "ם"
                // "כ" - "ך"
                // "צ" - "ץ"
                if (word.EndsWith("ף"))
                {
                    var variant1 = word.Remove(word.Length - 1, 1) + "פ";
                    variant1 += "ים";
                    searchAndOrList[i].Add(variant1);
                    continue;
                }
                else if (word.EndsWith("ן"))
                {
                    var variant1 = word.Remove(word.Length - 1, 1) + "נ";
                    variant1 += "ים";
                    searchAndOrList[i].Add(variant1);
                    continue;
                }
                else if (word.EndsWith("ם"))
                {
                    var variant1 = word.Remove(word.Length - 1, 1) + "מ";
                    variant1 += "ים";
                    searchAndOrList[i].Add(variant1);
                    continue;
                }
                else if (word.EndsWith("ך"))
                {
                    var variant1 = word.Remove(word.Length - 1, 1) + "כ";
                    variant1 += "ים";
                    searchAndOrList[i].Add(variant1);
                    continue;
                }
                else if (word.EndsWith("ץ"))
                {
                    var variant1 = word.Remove(word.Length - 1, 1) + "צ";
                    variant1 += "ים";
                    searchAndOrList[i].Add(variant1);
                    continue;
                }

                #endregion
            }
            //   var orTextPredicate = words.Where(x => x.Length > 3).ToList(); // select only length > 3, keywords
            //if (orTextPredicate.Count > 0)
            int whatPosition = 0;
            foreach (var orTextPredicate in searchAndOrList)
            {
                Expression<Func<Product, bool>> predicate = null;
                foreach (var s in orTextPredicate)
                {
                    if (whatPosition == 0)
                    {
                        if (predicate != null)
                        {
                            predicate = predicate.MultiSearchOrSql(x => x.Name.StartsWith(s + " ")
                                || x.SKU.StartsWith(s + " ")
                                || x.ShortDescription.StartsWith(s + " ")
                                || x.FullDescription.StartsWith(s + " ")
                                //     || x.Manufacturer.Contains(s)
                                );
                        }
                        else
                        {
                            predicate = x => (x.Name.StartsWith(s)
                                 || x.SKU.StartsWith(s)
                                || x.ShortDescription.StartsWith(s)
                                || x.FullDescription.StartsWith(s)
                                //      || x.Manufacturer.Contains(s)
                                );
                        }
                    }
                    else
                    {
                        if (predicate != null)
                        {
                            predicate = predicate.MultiSearchOrSql(x => x.Name.Contains(s)
                                || x.SKU.Contains(s)
                                || x.ShortDescription.Contains(s)
                                || x.FullDescription.Contains(s)
                                //     || x.Manufacturer.Contains(s)
                                );
                        }
                        else
                        {
                            predicate = x => (x.Name.Contains(s)
                                 || x.SKU.Contains(s)
                                || x.ShortDescription.Contains(s)
                                || x.FullDescription.Contains(s)
                                //      || x.Manufacturer.Contains(s)
                                );
                        }
                    }
                }
                if (predicate != null)
                {
                    source = source.Where(predicate);
                }
                whatPosition++;
            }


            #endregion

            result.AddRange((from ps in LS.CurrentEntityContext.ProductShopMap
                             join p in source
                             on ps.ProductID equals p.ID
                             where ps.ShopID == ShopID
                             // && p.Name.Contains(text)
                             select new ProductShortModel
                             {
                                 Name = p.Name,
                                 ID = ps.ProductID
                             }).Take(20).ToList());
            if (result.Count == 0)
            {
                //no result
                UserActivityService.InsertSearchNotFound(text, ShopID, LS.CurrentUser.ID
                    , HttpContext.Current.Request.RawUrl,
                    HttpContext.Current.Request.UrlReferrer != null ? HttpContext.Current.Request.UrlReferrer.OriginalString : null
                     , LS.GetUser_IP(HttpContext.Current.Request));
            }
            return result;
        }

        public static double distance(double lat1, double lon1, double lat2, double lon2, char unit)
        {
            double theta = lon1 - lon2;
            double dist = Math.Sin(deg2rad(lat1)) * Math.Sin(deg2rad(lat2)) + Math.Cos(deg2rad(lat1)) * Math.Cos(deg2rad(lat2)) * Math.Cos(deg2rad(theta));
            dist = Math.Acos(dist);
            dist = rad2deg(dist);
            dist = dist * 60 * 1.1515;
            if (unit == 'K')
            {
                dist = dist * 1.609344;
            }
            else if (unit == 'N')
            {
                dist = dist * 0.8684;
            }
            return (dist);
        }

        //:::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::
        //::  This function converts decimal degrees to radians             :::
        //:::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::
        private static double deg2rad(double deg)
        {
            return (deg * Math.PI / 180.0);
        }

        //:::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::
        //::  This function converts radians to decimal degrees             :::
        //:::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::
        private static double rad2deg(double rad)
        {
            return (rad / Math.PI * 180.0);
        }

        public static IEnumerable<Shop> GetNearestShop(int shopType, decimal latitude, decimal longitude
            , string address, bool ignoreDistance = false)
        {
            var allShops = LS.Get<Shop>();
            var cacheSource = allShops.Where(x => x.ShopTypeIDs != null
                && (x.IsShipEnabled || x.InStorePickUpEnabled)
                && x.Active
                );
            var allAdditionalZones = LS.Get<ShopDeliveryZone>();
            if (shopType > 0)
            {
                cacheSource = cacheSource.Where(x => x.ShopTypeIDs.Contains("," + shopType.ToString() + ",")
                                        || x.ShopTypeIDs == shopType.ToString()
                                        || x.ShopTypeIDs.StartsWith(shopType.ToString() + ",")
                                        || x.ShopTypeIDs.EndsWith("," + shopType.ToString())
                                        );
            }

            var source = cacheSource.Select(x => new DistanceSearchItem()
            {
                ID = x.ID,
                RadiusLatitude = x.RadiusLatitude
                ,
                RadiusLongitude = x.RadiusLongitude
                ,
                ShipRadius = x.ShipRadius
                ,
                ShipCost = x.ShipCost,
                Rate = x.Rate,
                DisplayOrder = x.DisplayOrder
            }).ToList();

            source.AddRange(
                (from az in allAdditionalZones
                 join s in allShops
                 on az.ShopID equals s.ID
                 where az.Active && az.ShopID > 0
                 select new DistanceSearchItem()
                 {
                     ID = s.ID,
                     RadiusLatitude = az.RadiusLatitude
                     ,
                     RadiusLongitude = az.RadiusLongitude
                     ,
                     ShipRadius = az.ShipRadius
                     ,
                     ShipCost = az.ShipCost,
                     Rate = s.Rate,
                     DisplayOrder = s.DisplayOrder,
                     IsWholeCity = az.DeliveryFroAllCity,
                     City = az.City

                 }
                     )
                );
            List<IntDoublePair> distances = new List<IntDoublePair>();
            double maxRate = 0.1;
            double maxShip = 0.1;
            int maxOrders = 0;
            // 1) Display Order
            var lowerCaseAddress = address != null ? address.ToLower() : "";
            foreach (var s in source)
            {
                if (s.IsWholeCity)
                {
                    //check the city in address
                    if (!string.IsNullOrEmpty(s.City) &&
                        (
                        lowerCaseAddress.Contains(", " + s.City.ToLower() + ", ")
                        || lowerCaseAddress.StartsWith(s.City.ToLower() + ", ")
                        // || lowerCaseAddress.EndsWith(", " + s.City.ToLower())
                        // || lowerCaseAddress.StartsWith(s.City.ToLower() + " ")
                        || lowerCaseAddress.EndsWith(", " + s.City.ToLower())
                        )
                        )
                    {
                        //city founded
                        distances.Add(new IntDoublePair() { Int = s.ID, Double = 1 });
                    }
                    else if (ignoreDistance)
                    {
                        //not this city
                        distances.Add(new IntDoublePair() { Int = s.ID, Double = 10000 });
                    }

                }
                else if (!ignoreDistance)
                {
                    var clientradius = distance((double)s.RadiusLatitude, (double)s.RadiusLongitude, (double)latitude, (double)longitude, 'K');
                    if (clientradius <= (double)s.ShipRadius)
                        distances.Add(new IntDoublePair() { Int = s.ID, Double = clientradius });
                }
                else
                {
                    distances.Add(new IntDoublePair() { Int = s.ID, Double = 10000 });
                }
                if ((double)s.Rate > maxRate)
                {
                    maxRate = (double)s.Rate;
                }
                if ((double)s.ShipCost > maxShip)
                {
                    maxShip = (double)s.ShipCost;
                }
                int ShopID = s.ID;

                int ordersToday = LS.GetCachedFunc<int>(() =>
                {
                    var curDateOnly = DateTime.Now.Date;
                    var count = LS.CurrentEntityContext.Orders.Count(x => x.ShopID == ShopID && x.CreateOn >= curDateOnly);
                    return count;
                }, "shopOrdersToday_" + ShopID.ToString(), 10); //cache 10 min
                if (ordersToday > maxOrders)
                {
                    maxOrders = ordersToday;
                }
            }
            double max = 1;
            if (distances.Count > 0)
            {
                max = distances.Select(x => x.Double).Max();
            }
            if (max == 0)
            {
                max = 1;//prevent zero division
            }
            var curdate = DateTime.Now.Date;
            var curdateTime = DateTime.Now;
            var lastdate = curdate.AddDays(7);
            foreach (var s in source)
            {
                var groupDistances = distances.Where(x => x.Int == s.ID).ToList();
                if (groupDistances.Count > 0)
                {
                    groupDistances.ForEach((e) =>
                    {
                        e.Double = s.DisplayOrder // 70 %
                            + (max - e.Double) * 10 / max // 10 %
                            + (maxShip - (double)s.ShipCost) * 5 / maxShip      //
                            + (double)s.Rate * 10 / maxRate
                            ;
                        int ShopID = s.ID;
                        if (maxOrders > 0)
                        {
                            int ordersToday = LS.GetCachedFunc<int>(() =>
                            {
                                var curDateOnly = DateTime.Now.Date;
                                var count = LS.CurrentEntityContext.Orders.Count(x => x.ShopID == ShopID && x.CreateOn >= curDateOnly);
                                return count;
                            }, "shopOrdersToday_" + ShopID.ToString(), 10); //cache 10 min
                            e.Double += ordersToday * 5 / maxOrders;
                        }
                        //check shop closed
                        var WorkTimes =
                            LS.GetCachedFunc<List<ShopWorkTime>>(() =>
                            {



                                return LS.CurrentEntityContext.ShopWorkTimes.Where(x => x.ShopID == ShopID && x.Active &&
                               (!x.IsSpecial || (x.Date >= curdate && x.Date <= lastdate))
                               ).OrderBy(x => x.IsSpecial).ThenBy(x => x.Day).ThenBy(x => x.Date)
                               .ToList();
                            }, "shopWorkTime_" + ShopID.ToString(), 60);

                        var time = WorkTimes.FirstOrDefault(
                           x => (x.IsSpecial && x.Date.Date == curdate)
                               ||
                               (!x.IsSpecial && x.Day == curdate.DayOfWeek)

                           );
                        bool validTime = false;
                        if (time != null)
                        {
                            int curIntTime = curdateTime.Hour * 60 + curdateTime.Minute;
                            if (time.TimeFrom <= curIntTime && curIntTime <= time.TimeTo)
                            {
                                validTime = true;
                            }
                        }
                        if (!validTime)
                        {
                            e.Double -= 30;
                        }
                        //end shop closed check
                    });
                }
            }



            var shopIds = (from s in allShops
                           join d in distances
                           on s.ID equals d.Int
                           orderby d.Double descending
                           select s
                        );
            return shopIds;
        }
        public static IList<ShopType> GetShopTypes()
        {
            return LS.Get<ShopType>();
        }

        public static bool IsCanVoteForShop(int ShopID, Guid UserID)
        {
            if (UserID == Guid.Empty && LS.isHaveID())
            {
                UserID = LS.CurrentUser.ID;
            }
            if (UserID == Guid.Empty)
            {
                return false;
            }

            return !LS.CurrentEntityContext.ShopRates.Any(x => x.ShopID == ShopID && x.UserID == UserID);
        }
        /// <summary>
        /// return 0 if can`t add
        /// </summary>
        /// <param name="ShopID"></param>
        /// <param name="Rate"></param>
        /// <param name="UserID"></param>
        /// <returns>id of rate or 0 if cant add</returns>
        public static int AddShopRate(int ShopID, int Rate, Guid UserID = new Guid())
        {

            var rate = new ShopRate();
            if (UserID == Guid.Empty && LS.isHaveID())
            {
                UserID = LS.CurrentUser.ID;
            }
            if (UserID == Guid.Empty)
            {
                return 0;
            }
            if (LS.CurrentEntityContext.ShopRates.Any(x => x.ShopID == ShopID && x.UserID == UserID))
            {
                return 0;
            }
            rate.UserID = UserID;
            rate.Rate = Rate;
            rate.ShopID = ShopID;

            LS.CurrentEntityContext.ShopRates.Add(rate);
            LS.CurrentEntityContext.SaveChanges();

            var shop = LS.CurrentEntityContext.Shops.FirstOrDefault(x => x.ID == ShopID);
            if (shop != null)
            {
                shop.Rate = (shop.Rate * shop.RateCount + Rate) / (shop.RateCount + 1);
                shop.RateCount++;
                LS.CurrentEntityContext.SaveChanges();

            }
            return rate.ID;
        }

        public static void AddProductToFavorite(int ProductShopID, Guid UserID = new Guid())
        {
            if (UserID == Guid.Empty && LS.isHaveID())
            {
                UserID = LS.CurrentUser.ID;
            }
            if (UserID == Guid.Empty)
            {
                return;
            }
            var fav = LS.CurrentEntityContext.ProductFavorites.FirstOrDefault(x => x.ProductShopID == ProductShopID
                && x.UserID == UserID);
            if (fav == null)
            {
                UserActivityService.InsertFavoriteProduct(UserID, ProductShopID, false
                  , HttpContext.Current.Request.RawUrl,
                  HttpContext.Current.Request.UrlReferrer != null ? HttpContext.Current.Request.UrlReferrer.OriginalString : null
                   , LS.GetUser_IP(HttpContext.Current.Request));

                fav = new ProductFavorite()
                {
                    CreateDate = DateTime.Now,
                    ProductShopID = ProductShopID,
                    UserID = UserID
                };
                LS.CurrentEntityContext.ProductFavorites.Add(fav);
                LS.CurrentEntityContext.SaveChanges();
            }
        }
        public static void RemoveProductFromFavorite(int ProductShopID, Guid UserID = new Guid())
        {
            if (UserID == Guid.Empty && LS.isHaveID())
            {
                UserID = LS.CurrentUser.ID;
            }
            if (UserID == Guid.Empty)
            {
                return;
            }
            var fav = LS.CurrentEntityContext.ProductFavorites.FirstOrDefault(x => x.ProductShopID == ProductShopID
                && x.UserID == UserID);
            if (fav != null)
            {
                UserActivityService.InsertFavoriteProduct(UserID, ProductShopID, true
               , HttpContext.Current.Request.RawUrl,
               HttpContext.Current.Request.UrlReferrer != null ? HttpContext.Current.Request.UrlReferrer.OriginalString : null
                , LS.GetUser_IP(HttpContext.Current.Request));

                LS.CurrentEntityContext.ProductFavorites.Remove(fav);
                LS.CurrentEntityContext.SaveChanges();
            }
        }

        public static bool IsProductInFavorite(int ProductShopID, Guid UserID)
        {
            return LS.CurrentEntityContext.ProductFavorites.FirstOrDefault(x => x.ProductShopID == ProductShopID && x.UserID == UserID) == null ? false : true;
        }

        public static List<ShopRateModel> GetShopRates(int ShopID)
        {

            var rates = (from sc in LS.CurrentEntityContext.ShopRates
                         from u in LS.CurrentEntityContext.Users.Where(x => x.ID == sc.UserID).DefaultIfEmpty()
                         where sc.ShopID == ShopID
                         select new ShopRateModel()
                            {
                                ID = sc.ID,
                                ShopID = sc.ShopID,
                                Rate = sc.Rate,
                                UserID = sc.UserID,
                                UserName = u != null ? u.FirstName + " " + u.LastName : ""
                            }).ToList();


            return rates;
        }
        public static ShopComment AddShopComment(ShopComment commen, int ParentID = 0, Guid UserID = new Guid())
        {

            var c = new ShopComment();
            if (UserID == Guid.Empty && LS.isHaveID())
            {
                UserID = LS.CurrentUser.ID;
            }
            c.UserID = UserID;
            c.ParentID = ParentID;
            c.ShopID = commen.ShopID;
            c.Text = commen.Text;
            c.Title = commen.Title;
            c.UserName = commen.UserName;
            c.CreateTime = DateTime.Now;
            LS.CurrentEntityContext.ShopComments.Add(c);
            LS.CurrentEntityContext.SaveChanges();
            return c;

        }
        public static List<ShopCommentModel> GetShopComments(int ShopID, int ParentID = 0)
        {
            var comments = (from sc in LS.CurrentEntityContext.ShopComments
                            from u in LS.CurrentEntityContext.Users.Where(x => x.ID == sc.UserID).DefaultIfEmpty()
                            where sc.ShopID == ShopID && sc.ParentID == ParentID
                            && sc.Approved == true
                            orderby sc.CreateTime descending
                            select new ShopCommentModel()
                            {
                                ID = sc.ID,
                                Title = sc.Title,
                                ParentID = sc.ParentID,
                                ShopID = sc.ShopID,
                                CreateDate = sc.CreateTime,
                                Text = sc.Text,
                                UserID = sc.UserID,
                                UserName = u.ID != null ? u.FirstName + " " + u.LastName : sc.UserName,
                            }).OrderByDescending(x => x.CreateDate).ToList();
            foreach (var cm in comments)
            {
                if (cm.ID != cm.ParentID)
                    cm.ShopComments = GetShopComments(ShopID, cm.ID);
                else
                    cm.ShopComments = new List<ShopCommentModel>();
            }

            return comments;
        }

        #region Product

        public static ProductComment AddProductComment(ProductComment commen, int ParentID = 0, Guid UserID = new Guid())
        {

            var productComment = new ProductComment();
            if (UserID == Guid.Empty && LS.isHaveID())
            {
                UserID = LS.CurrentUser.ID;
            }
            productComment.UserID = UserID;
            productComment.ParentID = ParentID;
            productComment.ProductID = commen.ProductID;
            productComment.Text = commen.Text;
            productComment.Title = commen.Title;
            productComment.UserName = commen.UserName;
            productComment.CreateDate = DateTime.Now;

            LS.CurrentEntityContext.ProductComments.Add(productComment);
            LS.CurrentEntityContext.SaveChanges();
            return productComment;
        }

        public static List<ProductComment> GetProductComments(int productID, int ParentID = 0)
        {
            var comments = (from pc in LS.CurrentEntityContext.ProductComments
                            from u in LS.CurrentEntityContext.Users.Where(x => x.ID == pc.UserID).DefaultIfEmpty()
                            where pc.ProductID == productID && pc.ParentID == ParentID
                            && pc.Approved == true
                            orderby pc.CreateDate descending
                            select new
                            {
                                ID = pc.ID,
                                Title = pc.Title,
                                ParentID = pc.ParentID,
                                ProductID = pc.ProductID,
                                CreateDate = pc.CreateDate,
                                Text = pc.Text,
                                UserID = pc.UserID,
                                UserName = u.ID != null ? u.FirstName + " " + u.LastName : pc.UserName,
                            }).OrderByDescending(x => x.CreateDate).ToList().Select(x => new ProductComment()
                            {
                                ID = x.ID,
                                Title = x.Title,
                                ParentID = x.ParentID,
                                ProductID = x.ProductID,
                                CreateDate = x.CreateDate,
                                Text = x.Text,
                                UserID = x.UserID,
                                UserName = x.UserName

                            }).ToList();
            foreach (var cm in comments)
            {
                if (cm.ID != cm.ParentID)
                    cm.ProductComments = GetProductComments(productID, cm.ID);
                else
                    cm.ProductComments = new List<ProductComment>();
            }

            return comments;
        }

        public static ProductNote AddOrEditNoteForProduct(int productID, string text)
        {
            var currentUserID = LS.CurrentUser.ID;
            var productNote = new ProductNote();
            productNote = LS.CurrentEntityContext.ProductNotes.FirstOrDefault(x => x.UserID == currentUserID && x.ProductID == productID);

            if (productNote == null)
            {
                productNote = new ProductNote()
                {
                    CreateDate = DateTime.Now,
                    ProductID = productID,
                    Text = text,
                    UserID = currentUserID,
                };

                LS.CurrentEntityContext.ProductNotes.Add(productNote);

            }
            else
            {
                productNote.Text = text;
            }
            LS.CurrentEntityContext.SaveChanges();
            return productNote;
        }

        public static string GetUserNoteForProduct(int productID, Guid? userOID)
        {
            if (userOID == null)
                userOID = LS.CurrentUser.ID;
            var data = LS.CurrentEntityContext.ProductNotes.FirstOrDefault(x => x.UserID == userOID && x.ProductID == productID);
            return data == null ? "" : data.Text;
        }

        public static int AddProductRate(int productID, int Rate, Guid UserID = new Guid())
        {

            var rate = new ProductRate();
            if (UserID == Guid.Empty && LS.isHaveID())
            {
                UserID = LS.CurrentUser.ID;
            }
            if (UserID == Guid.Empty)
            {
                return 0;
            }
            rate = LS.CurrentEntityContext.ProductRates.FirstOrDefault(x => x.ProductID == productID && x.UserID == UserID);
            if (rate != null)
            {
                var product = LS.CurrentEntityContext.Products.FirstOrDefault(x => x.ID == productID);
                if (product != null)
                {
                    product.Rate = (product.Rate * product.RateCount - rate.Rate + Rate) / (product.RateCount);
                    // pr.RateCount++;
                    rate.Rate = Rate;
                    LS.CurrentEntityContext.SaveChanges();

                }
                return rate.ID;
            }
            rate = new ProductRate();
            rate.UserID = UserID;
            rate.Rate = Rate;
            rate.ProductID = productID;

            LS.CurrentEntityContext.ProductRates.Add(rate);
            LS.CurrentEntityContext.SaveChanges();

            var pr = LS.CurrentEntityContext.Products.FirstOrDefault(x => x.ID == productID);
            if (pr != null)
            {
                pr.Rate = (pr.Rate * pr.RateCount + Rate) / (pr.RateCount + 1);
                pr.RateCount++;
                LS.CurrentEntityContext.SaveChanges();

            }
            return rate.ID;

        }

        public static bool IsCanVoteForProduct(int productID, Guid userID)
        {
            if (userID == Guid.Empty && LS.isHaveID())
            {
                userID = LS.CurrentUser.ID;
            }
            if (userID == Guid.Empty)
            {
                return false;
            }

            return !LS.CurrentEntityContext.ProductRates.Any(x => x.ProductID == productID && x.UserID == userID);
        }

        public static bool IsProductInShoppingCart(int productShopID)
        {
            var currenUserOID = LS.CurrentUser.ID;
            return LS.CurrentEntityContext.ShoppingCartItems.Any(x => x.ProductShopID == productShopID && x.UserID == currenUserOID);
        }

        #endregion

        #region Contact
        public static void AddContact(Contact contact)
        {
            LS.CurrentEntityContext.Contacts.Add(contact);
            LS.CurrentEntityContext.SaveChanges();
        }
        #endregion
    }
    public class DistanceSearchItem
    {
        public int ID { get; set; }
        public decimal RadiusLatitude { get; set; }
        public decimal RadiusLongitude { get; set; }
        public decimal ShipRadius { get; set; }
        public decimal ShipCost { get; set; }
        public decimal Rate { get; set; }
        public bool IsWholeCity { get; set; }
        public string City { get; set; }
        public int DisplayOrder { get; set; }
    }
    public class IntDoublePair
    {
        public int Int { get; set; }
        public double Double { get; set; }
    }
}
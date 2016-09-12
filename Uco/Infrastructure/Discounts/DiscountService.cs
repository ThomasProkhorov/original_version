using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Web;
using Uco.Infrastructure.Livecycle;
using Uco.Infrastructure.Repositories;
using Uco.Infrastructure.Services;
using Uco.Models;
using Uco.Models.Overview;

namespace Uco.Infrastructure.Discounts
{
    public class DiscountService
    {
        #region constructor
        public DiscountService(Db _db = null, bool nocache = false)
        {
            _Context = new DBContextService(_db);
            if (nocache)
            {
                _UseCache = false;
            }
            //  this._init();
        }
        private DBContextService _Context = null;
        private bool _UseCache = true;
        private Db _db
        {
            get
            {
                if (_Context == null) { return null; }
                return _Context.EntityContext;
            }
        }

        private static string cachekey = "DiscountsInstances";
        private static Dictionary<string, IDiscountRule> rules;
        private static void _init()
        {
            if (!LS.IsExistInCache(cachekey))
            {
                var instances = (from t in Assembly.GetExecutingAssembly().GetTypes()
                                 where t.GetInterfaces().Contains(typeof(IDiscountRule))
                                          && t.GetConstructor(Type.EmptyTypes) != null
                                 select Activator.CreateInstance(t) as IDiscountRule).ToDictionary(x => x.GetType().Name);
                instances.SetMeToCache(cachekey);
            }
            rules = LS.GetFromCache<Dictionary<string, IDiscountRule>>(cachekey);
        }
        #endregion
        public static Dictionary<string, IDiscountRule> GetRules()
        {
            _init();
            return rules;
        }
        public string GetLocalizeRuleName(string systemName)
        {
            return RP.T("Discount.Name." + systemName).ToString();
        }

        public class DiscountDescriptor
        {
            public decimal Amount { get; set; }
            public bool InPercents { get; set; }
        }

        public DiscountDescriptor CalculateDiscountByCode(string code, ShoppingCartOverviewModel overview)
        {
            Discount discount = _db.Discounts.FirstOrDefault(_ => _.Active && _.DiscountCode == code);
            decimal itemsCount = overview.Items.Select(_ => _.Quantity).Sum();
            bool isPercent = discount.IsPercent;
            decimal discountAmount = discount.Amount;
            decimal discountValue = 0m;
            var diff = (isPercent ? overview.Total * (discountAmount / 100) : discountAmount);
            if (discount.DiscountCartItemType == DiscountCartItemType.ForItemPrice)
            {
                diff *= itemsCount;
            }

            discountValue = overview.Total - diff;

            return new DiscountDescriptor
            {
                Amount = discountValue,
                InPercents = isPercent
            };
        }

        public int GetCurrentLimitPosition(Discount discount, User user, bool cache = true
            , string userName = null, string Phone = null, string Address = null, string Email = null)
        {
            Func<int> getHistoryFunc = () =>
            {
                if (discount.LimitType == DiscountLimitType.PerAll)
                {
                    int sum = _db.DiscountUsages
                        .Where(x => x.DiscountID == discount.ID)
                        .Select(x => x.UsedTimes).DefaultIfEmpty(0).Sum();
                    return sum;
                }
                else if (discount.LimitType == DiscountLimitType.PerCustomer)
                {
                    var query = _db.DiscountUsages
                         .Where(x => x.DiscountID == discount.ID
                         && x.UserID == user.ID
                         );
                    if (!string.IsNullOrEmpty(userName)
                        || !string.IsNullOrEmpty(Phone)
                         || !string.IsNullOrEmpty(Address)
                        || !string.IsNullOrEmpty(Email)
                        )
                    {
                        query = _db.DiscountUsages
                         .Where(x => x.DiscountID == discount.ID
                         && (x.UserID == user.ID
                         || (x.UsageData != null && userName != null && x.UsageData.Contains(":\"" + userName))
                         || (x.UsageData != null && Phone != null && x.UsageData.Contains(":\"" + Phone))
                         || (x.UsageData != null && Address != null && x.UsageData.Contains(":\"" + Address))
                         || (x.UsageData != null && Email != null && x.UsageData.Contains(":\"" + Email))
                         )
                         );
                    }
                    int sum = query
                         .Select(x => x.UsedTimes).DefaultIfEmpty(0).Sum();
                    return sum;
                }
                return 0;
            };
            if (cache)
            {
                string key = "Discount_History_" + discount.ID + "_" + user.ID;
                return LS.GetCachedFunc<int>(getHistoryFunc, key, 30);
            }
            return getHistoryFunc();
        }
        public static void ClearHistoryCache(int discountID, Guid userID)
        {
            string key = "Discount_History_" + discountID + "_" + userID;
            LS.RemoveFromCache(key);
        }
        public static bool LimitCheck(int curNumber, int limit, DiscountLimitType limitType)
        {
            return (curNumber < limit || limitType == DiscountLimitType.NoLimit);
        }
        public static bool ExpiriedCheck(Discount discount)
        {
            if (discount.StartDate.HasValue || discount.EndDate.HasValue)
            {
                var curDate = DateTime.Now;
                if (discount.StartDate.HasValue && curDate < discount.StartDate.Value)
                {
                    return false;
                }
                if (discount.EndDate.HasValue && curDate > discount.EndDate.Value)
                {
                    return false;
                }
            }

            return true;
        }
        public void ProcessItems(ShoppingCartOverviewModel model, User user)
        {

            //discounts 

            if (model.Items.Count > 0)
            {
                var userdata = ShoppingCartService.GetCheckoutData();
                System.Web.Script.Serialization.JavaScriptSerializer js = new System.Web.Script.Serialization.JavaScriptSerializer();



                var discounts = LS.Get<Discount>(model.ShopID.ToString() + "_" + DiscountType.ForCartItem.ToString()); // search discounts for shop and type
                bool success = false;
                foreach (var curdiscount in discounts)
                {
                    #region process discount for cart items
                    bool breakmain = false;
                    if (curdiscount.IsCodeRequired)
                    {
                        if (string.IsNullOrEmpty(userdata.CouponCode) || userdata.CouponCode != curdiscount.DiscountCode)
                        {
                            continue; // code doesn`t match
                        }
                    }
                    if (!DiscountService.ExpiriedCheck(curdiscount))
                    {
                        continue;
                    }
                    int curUsesNumber = GetCurrentLimitPosition(curdiscount, user, _UseCache, userdata.FullName, userdata.Phone, userdata.Address, user.Email);
                    if (LimitCheck(curUsesNumber, curdiscount.Limit, curdiscount.LimitType))
                    {
                        success = true;
                        foreach (var d in GetRules().Where(x =>
                            // x.Value.Type == DiscountType.ForCartItem && 
                            curdiscount.RuleList.Select(y => y.Name).Contains(x.Key)
                            ))
                        {
                            var confitem = d.Value.GetConfigItem();

                            int i = 0;
                            foreach (var r in curdiscount.RuleList.Where(x => x.Name == d.Key))
                            {
                                i++;

                                //from cache :)
                                var o = curdiscount.GetRuleConfigObject(i.ToString() + r.Name, () =>
                                {
                                    return js.Deserialize(r.Value, confitem.GetType());
                                });


                                success = success && d.Value.Process(model, curdiscount, o, user, curUsesNumber);


                            }
                        }
                        if (success)
                        {

                            var productsList = curdiscount.GetProductsList();
                            if (curdiscount.IsCodeRequired && curdiscount.DiscountCode == userdata.CouponCode)
                            {
                                model.DiscountByCouponeCodeText = curdiscount.Name;
                            }
                            foreach (var item in model.Items.Where(x => productsList.Contains(x.ProductShopID.ToString())))
                            {
                                if (!DiscountService.LimitCheck(curUsesNumber, curdiscount.Limit, curdiscount.LimitType))
                                {
                                    break;
                                }
                                if (item.Price > 0 && item.TotalDiscountAmount < item.Price * item.Quantity)//only if actual
                                {
                                    if (curdiscount.DiscountCartItemType == DiscountCartItemType.ForItemPrice)
                                    {
                                        if (curdiscount.IsPercent)
                                        {
                                            item.Price = item.Price - (item.Price * curdiscount.Percent / 100);
                                        }
                                        else
                                        {
                                            item.Price = item.Price - curdiscount.Amount;
                                        }
                                        if (item.Price < 0) { item.Price = 0; }
                                    }
                                    else
                                    {
                                        if (curdiscount.IsPercent)
                                        {
                                            item.TotalDiscountAmount = item.Price * curdiscount.Percent * item.Quantity / 100;
                                        }
                                        else
                                        {
                                            item.TotalDiscountAmount = curdiscount.Amount;
                                        }

                                    }
                                    if (!string.IsNullOrEmpty(item.DiscountDescription))
                                    {
                                        item.DiscountDescription += ", " + curdiscount.Name;
                                    }
                                    else
                                    {
                                        item.DiscountDescription = curdiscount.Name;
                                    }
                                    item.DiscountIDs.Add(curdiscount.ID);
                                    if (curdiscount.LessShopFee)
                                    {
                                        model.IsLessMemberFee = true;
                                    }
                                    curUsesNumber++;
                                }
                            }

                            if (curdiscount.PreventNext)
                            {
                                breakmain = true;
                            }

                        }

                        if (breakmain)
                        {
                            break;
                        }
                    }
                    #endregion
                }
            }
        }
        private List<string> _TotalProcessKeys = new List<string>() 
        {
        DiscountType.ForCartSubTotal.ToString(),
        DiscountType.ForCartShip.ToString(),
       // DiscountType.ForCartFee.ToString(),
        DiscountType.ForCartTotal.ToString(),
        };
        public void ProcessTotals(ShoppingCartOverviewModel model, User user)
        {

            if (model.Items.Count > 0)
            {
                var userdata = ShoppingCartService.GetCheckoutData();
                System.Web.Script.Serialization.JavaScriptSerializer js = new System.Web.Script.Serialization.JavaScriptSerializer();


                foreach (var typDisc in _TotalProcessKeys)
                {
                    var discounts = LS.Get<Discount>(model.ShopID.ToString() + "_" + typDisc)

                        ; // search discounts for shop and type
                    discounts.AddRange(LS.Get<Discount>("0_" + typDisc));
                    bool success = false;


                    if (typDisc == DiscountType.ForCartTotal.ToString())
                    {
                        //fix total after previous discounts
                        model.TotalWithoutShip = model.SubTotal + model.Fee;
                        model.Total = model.SubTotal + model.ShippingCost + model.Fee;
                    }
                    if (discounts.Count > 0)
                    {
                        foreach (var curdiscount in discounts)
                        {
                            #region process discount for cart totals
                            bool breakmain = false;
                            if (curdiscount.IsCodeRequired)
                            {
                                if (string.IsNullOrEmpty(userdata.CouponCode) || userdata.CouponCode != curdiscount.DiscountCode)
                                {
                                    continue; // code doesn`t match
                                }
                            }
                            if (!DiscountService.ExpiriedCheck(curdiscount))
                            {
                                continue;
                            }
                            int curUsesNumber = GetCurrentLimitPosition(curdiscount, user, _UseCache);
                            if (LimitCheck(curUsesNumber, curdiscount.Limit, curdiscount.LimitType))
                            {
                                success = true;
                                foreach (var d in GetRules().Where(x =>
                                    // x.Value.Type == DiscountType.ForCartItem && 
                                    curdiscount.RuleList.Select(y => y.Name).Contains(x.Key)
                                    ))
                                {
                                    var confitem = d.Value.GetConfigItem();

                                    int i = 0;
                                    foreach (var r in curdiscount.RuleList.Where(x => x.Name == d.Key))
                                    {
                                        i++;

                                        //from cache :)
                                        var o = curdiscount.GetRuleConfigObject(i.ToString() + r.Name, () =>
                                        {
                                            return js.Deserialize(r.Value, confitem.GetType());
                                        });


                                        success = success && d.Value.Process(model, curdiscount, o, user, curUsesNumber);


                                    }
                                }
                                if (success)
                                {
                                    bool actual = false;
                                    #region subtotal
                                    if (curdiscount.DiscountType == DiscountType.ForCartSubTotal)
                                    {
                                        if (model.SubTotal > 0)
                                        {
                                            decimal minus = 0;
                                            if (curdiscount.IsPercent)
                                            {
                                                minus = (model.SubTotal * curdiscount.Percent / 100);
                                                model.SubTotal = model.SubTotal - minus;
                                            }
                                            else
                                            {
                                                minus = curdiscount.Amount;
                                                model.SubTotal = model.SubTotal - curdiscount.Amount;
                                            }
                                            if (model.SubTotal < 0) { model.SubTotal = 0; }
                                            model.TotalDiscount += minus;
                                            actual = true;
                                        }
                                    }
                                    #endregion
                                    #region ship
                                    else if (curdiscount.DiscountType == DiscountType.ForCartShip)
                                    {
                                        if (model.SubTotal < model.FreeShipFrom)
                                        {
                                            model.ShippingCost = model.ShopShipCost;
                                        }
                                        if (model.ShippingCost > 0)
                                        {
                                            decimal minus = 0;
                                            if (curdiscount.IsPercent)
                                            {
                                                minus = (model.ShippingCost * curdiscount.Percent / 100);
                                                model.ShippingCost = model.ShippingCost - minus;
                                            }
                                            else
                                            {
                                                minus = curdiscount.Amount;
                                                model.ShippingCost = model.ShippingCost - curdiscount.Amount;
                                            }
                                            if (model.ShippingCost < 0) { model.ShippingCost = 0; }
                                            model.TotalDiscount += minus;
                                            actual = true;
                                        }
                                    }
                                    #endregion
                                    #region payment fee
                                    //else if (curdiscount.DiscountType == DiscountType.ForCartFee)
                                    //{
                                    //    if (model.Fee > 0)
                                    //    {
                                    //        decimal minus = 0; 
                                    //        if (curdiscount.IsPercent)
                                    //        {
                                    //            minus = (model.Fee * curdiscount.Percent / 100);
                                    //            model.Fee = model.Fee - minus;
                                    //        }
                                    //        else
                                    //        {
                                    //            minus = curdiscount.Amount;
                                    //            model.Fee = model.Fee - curdiscount.Amount;
                                    //        }
                                    //        if (model.Fee < 0) { model.Fee = 0; }
                                    //        model.TotalDiscount += minus;
                                    //        actual = true;
                                    //    }
                                    //}
                                    #endregion
                                    #region total
                                    else if (curdiscount.DiscountType == DiscountType.ForCartTotal)
                                    {
                                        if (model.Total > 0)
                                        {
                                            decimal minus = 0;
                                            if (curdiscount.IsPercent)
                                            {
                                                minus = (model.Total * curdiscount.Percent / 100);
                                                model.Total = model.Total - minus;
                                            }
                                            else
                                            {
                                                minus = curdiscount.Amount;
                                                model.Total = model.Total - curdiscount.Amount;
                                            }
                                            if (model.Total < 0) { model.Total = 0; }
                                            model.TotalDiscount += minus;
                                            actual = true;
                                        }
                                    }
                                    #endregion
                                    if (actual)//if actual
                                    {
                                        if (curdiscount.IsCodeRequired && curdiscount.DiscountCode == userdata.CouponCode)
                                        {

                                            model.DiscountByCouponeCodeText = curdiscount.Name;
                                        }
                                        curUsesNumber++;

                                        if (!string.IsNullOrEmpty(model.DiscountDescription))
                                        {
                                            model.DiscountDescription += ", " + curdiscount.Name;
                                        }
                                        else
                                        {
                                            model.DiscountDescription = curdiscount.Name;
                                        }
                                        if (curdiscount.LessShopFee)
                                        {
                                            model.IsLessMemberFee = true;
                                        }
                                        model.DiscountIDs.Add(curdiscount.ID);//for history save
                                        if (curdiscount.PreventNext)
                                        {
                                            breakmain = true;
                                        }
                                    }
                                }


                            }

                            if (breakmain)
                            {
                                break;
                            }
                            #endregion
                        }
                    }
                }
            }

        }

    }
}
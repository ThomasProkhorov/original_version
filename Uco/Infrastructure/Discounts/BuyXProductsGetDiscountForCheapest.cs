using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;
using Uco.Infrastructure.Livecycle;
using Uco.Infrastructure.Services;
using Uco.Models;
using Uco.Models.Overview;

namespace Uco.Infrastructure.Discounts
{
    public class BuyXProductsGetDiscountForCheapest : IDiscountRule
    {



        public bool Process(ShoppingCartOverviewModel model, Discount curdiscount, object DiscountConfigItem, User user, int lim)
        {
            var rule = (BuyXProductsGetDiscountForCheapestConfig)DiscountConfigItem;
            
            var cartItemsForDiscount=model.Items.Where(x => rule.ProductShopsIds.Contains(x.ProductShopID.ToString()));
            if (cartItemsForDiscount.Count() == 0) return false;
            
            var totalQuantity = (int)Math.Truncate(cartItemsForDiscount.Select(x => x.Quantity).DefaultIfEmpty(0).Sum());
            if (totalQuantity < rule.MinTotalOfRequiredQuantity) return false;

            var cheapest = cartItemsForDiscount.OrderBy(x => x.Price).First();
            var newPrice = Math.Truncate((cheapest.Price - (cheapest.Price * rule.Percent / 100)) * 100)/100;
            var discount = cheapest.Price - newPrice;
            cheapest.TotalDiscountAmount = discount;
            cheapest.DiscountDescription += String.IsNullOrEmpty(cheapest.DiscountDescription) ?
                                          curdiscount.Name
                                        : cheapest.DiscountDescription + "," + curdiscount.Name;
            cheapest.DiscountIDs.Add(curdiscount.ID);
            var userdata = ShoppingCartService.GetCheckoutData();
            if (curdiscount.IsCodeRequired && curdiscount.DiscountCode == userdata.CouponCode)
            {
                model.DiscountByCouponeCodeText = curdiscount.Name;
            }

            //we must return true, but discountservice change all item for all products for discount,
            //so we return false
            return false;
        }

        public object GetConfigItem()
        {
            return new BuyXProductsGetDiscountForCheapestConfig();
        }
    }

    public class BuyXProductsGetDiscountForCheapestConfig
    {

        public int MinTotalOfRequiredQuantity { get; set; }
        public decimal Percent { get; set; }

        [Model(Show = false)]
        [UIHint("ProductShopSearch")]
        public string ProductShopsIds{ get; set; }
    }
}
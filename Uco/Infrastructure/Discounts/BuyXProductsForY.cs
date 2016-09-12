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
    public class BuyXProductsForYDiscount : IDiscountRule
    {

        class CartItemProduct
        {
            public decimal Price { get; set; }
            public int ID { get; set; }
        }

        public bool Process(ShoppingCartOverviewModel model, Discount curdiscount, object DiscountConfigItem, User user, int lim)
        {
            var rule = (BuyXProductsForYConfig)DiscountConfigItem;

            var cartItemsForDiscount = model.Items.Where(x => !String.IsNullOrEmpty(rule.ProductShopsIds) && rule.ProductShopsIds.Contains(x.ProductShopID.ToString()));
            if (cartItemsForDiscount.Count() == 0) return false;
            
            var totalQuantity = (int)Math.Truncate(cartItemsForDiscount.Select(x => x.Quantity).DefaultIfEmpty(0).Sum());
            if (totalQuantity < rule.MinTotalOfRequiredQuantity) return false;
            
            int multiplicator =  totalQuantity / rule.MinTotalOfRequiredQuantity;
            decimal newSoldPrice = multiplicator * rule.PayAmount;
            var quuantityForDiscount = multiplicator * rule.MinTotalOfRequiredQuantity;

            var cartItemProducts = GetFlattenCartItem(cartItemsForDiscount, quuantityForDiscount);

            decimal oldSoldPrice = cartItemProducts.Sum(x => x.Price);
            
            if (oldSoldPrice<=newSoldPrice) return false;

            var discountTotal = oldSoldPrice - newSoldPrice;

            var discountByProductId = CalculateDiscountByProductId(discountTotal, cartItemProducts);

            foreach (var item in model.Items)
            {
                if (!discountByProductId.ContainsKey(item.ProductID)) continue;
                item.TotalDiscountAmount = discountByProductId[item.ProductID];
                item.DiscountDescription += String.IsNullOrEmpty(item.DiscountDescription) ? 
                                              curdiscount.Name 
                                            : item.DiscountDescription + "," + curdiscount.Name;
                item.DiscountIDs.Add(curdiscount.ID);
            }

            var userdata = ShoppingCartService.GetCheckoutData();
            if (curdiscount.IsCodeRequired && curdiscount.DiscountCode == userdata.CouponCode)
            {
                model.DiscountByCouponeCodeText = curdiscount.Name;
            }

            //we must return true, but discountservice change all item for all products for discount,
            //so we return false
            return false;
        }

        private Dictionary<int,decimal> CalculateDiscountByProductId(decimal discountTotal, List<CartItemProduct> cartItemProducts)
        {
            var result = new Dictionary<int, decimal>();
            var allTotal=cartItemProducts.Sum(x =>x.Price);
            foreach (var productItems in cartItemProducts.GroupBy(x => x.ID))
            {
                var total = productItems.Sum(x => x.Price);
                var percent = 100 * total / allTotal;
                var discount=Math.Truncate(percent * discountTotal)/100;
                result.Add(productItems.Key, discount);
            }
            var diff = discountTotal - result.Sum(x => x.Value);
            if (diff > 0)
                result[cartItemProducts.First().ID] += diff;

            return result;
        }

        private List<CartItemProduct> GetFlattenCartItem(IEnumerable<ShoppingCartItemModel> cartItemsForDiscount, int quuantityForDiscount)
        {
            var cartItemProducts = new List<CartItemProduct>();
            foreach (var cartItem in cartItemsForDiscount.OrderBy(x => x.Price))
            {
                for (var i = 0; i < cartItem.Quantity; i++)
                {
                    if (cartItemProducts.Count >= quuantityForDiscount) break;

                    cartItemProducts.Add(new CartItemProduct
                    {
                        Price = cartItem.Price,
                        ID = cartItem.ProductID
                    });
                }
            }
            return cartItemProducts;
        }
        public object GetConfigItem()
        {
            return new BuyXProductsForYConfig();
        }
    }

    public class BuyXProductsForYConfig
    {

        public int MinTotalOfRequiredQuantity { get; set; }
        public decimal PayAmount { get; set; }

        [Model(Show = false)]
        [UIHint("ProductShopSearch")]
        public string ProductShopsIds{ get; set; }
    }
}
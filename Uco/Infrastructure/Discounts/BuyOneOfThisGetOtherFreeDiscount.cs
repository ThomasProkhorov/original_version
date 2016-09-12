using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;
using Uco.Infrastructure.Livecycle;
using Uco.Models;
using Uco.Models.Overview;

namespace Uco.Infrastructure.Discounts
{
    public class BuyOneOfThisGetOtherFreeDiscount : IDiscountRule
    {
        //  public string Title = "Percent or fixed amount discount";
        //public DiscountType Type { 
        //    get{
        //return DiscountType.ForCartItem;
        //} 
        //}
        public bool Process(ShoppingCartOverviewModel model, Discount curdiscount, object DiscountConfigItem, User user, int lim)
        {
            var rule = (BuyOneOfThisGetOtherFreeDiscountConfig)DiscountConfigItem;
            var productsList = curdiscount.GetProductsList();
            var totalQuantity = model.Items.Where(x => productsList.Contains(x.ProductShopID.ToString())).Select(x => x.Quantity)
                .DefaultIfEmpty(0).Sum();

            if (totalQuantity >= rule.MinTotalOfRequiredQuantity)
            {
                var items = model.Items.Where(x => rule.ProductShopForFreeIDs.Contains(x.ProductShopID.ToString()))
               .ToList();
                decimal restQuantity = rule.MaxFreeQuantity;
                if (items.Count > 0)
                {

                    foreach (var item in items)
                    {
                        if (!DiscountService.LimitCheck(lim, curdiscount.Limit, curdiscount.LimitType))
                        {
                            break;
                        }
                        if (item.Quantity <= restQuantity)
                        {
                            item.Price = 0;// show that price is zero
                        }
                        else
                        {
                            item.TotalDiscountAmount += item.Price * restQuantity;// can be only one free, so we change unit price
                        }
                        restQuantity -= item.Quantity;
                        lim++;

                    }

                    return true;
                }
            }

            return false;
        }
        public object GetConfigItem()
        {
            return new BuyOneOfThisGetOtherFreeDiscountConfig();
        }
    }

    public class BuyOneOfThisGetOtherFreeDiscountConfig
    {
        // public int Order { get; set; }

        //  public string Title { get; set; }

        //  [Model(Show = false, Edit = false)]
        //  public int ProductShopID { get; set; }

        //  [NotMapped]
        //   [Model(Show = false, Edit = true, AjaxEdit = false)]
        //   public virtual ProductShop ProductShop { get; set; }

        public int MinTotalOfRequiredQuantity { get; set; }

        // [Model(Show = false)]
        // [UIHint("ProductShopSearch")]
        //public string ProductShopIDs { get; set; }

        [Model(Show = false)]
        [UIHint("ProductShopSearch")]
        public string ProductShopForFreeIDs { get; set; }


        public int MaxFreeQuantity { get; set; }
    }
}
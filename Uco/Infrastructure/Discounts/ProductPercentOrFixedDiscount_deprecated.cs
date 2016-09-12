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
    //its default empty discount, can be skiped
    public class ProductPercentOrFixedDiscount//: IDiscountRule
    {
        //  public string Title = "Percent or fixed amount discount";
        public DiscountType Type
        {
            get
            {
                return DiscountType.ForCartItem;
            }
        }
        public bool Process(ShoppingCartOverviewModel model, Discount curdiscount, object DiscountConfigItem, User user, int lim)
        {
            var rule = (PercentFixedDicountConfig)DiscountConfigItem;
            var items = model.Items.Where(x => rule.ProductShopIDs.Contains("," + x.ProductShopID + ",")).ToList();

            if (items.Count > 0)
            {

                foreach (var item in items)
                {
                    if (!DiscountService.LimitCheck(lim, curdiscount.Limit, curdiscount.LimitType))
                    {
                        break;
                    }
                    if (curdiscount.IsPercent)
                    {
                        item.Price = item.Price - (item.Price * curdiscount.Percent / 100);
                    }
                    else
                    {
                        item.Price = item.Price - curdiscount.Amount;
                    }
                    if (item.Price < 0) { item.Price = 0; }
                    lim++;

                }

                return true;
            }

            return false;
        }
        public object GetConfigItem()
        {
            return new PercentFixedDicountConfig();
        }
    }

    public class PercentFixedDicountConfig
    {
        // public int Order { get; set; }

        //  public string Title { get; set; }

        //  [Model(Show = false, Edit = false)]
        //  public int ProductShopID { get; set; }

        //  [NotMapped]
        //   [Model(Show = false, Edit = true, AjaxEdit = false)]
        //   public virtual ProductShop ProductShop { get; set; }


        [Model(Show = false)]
        [UIHint("ProductShopSearch")]
        public string ProductShopIDs { get; set; }
    }
}
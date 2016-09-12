using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;
using Uco.Infrastructure.Livecycle;
using Uco.Models;
using Uco.Models.Overview;

namespace Uco.Infrastructure.Discounts
{
    public class CategoryDiscount : IDiscountRule
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
            var rule = (CategoryDiscountConfig)DiscountConfigItem;
            var items = model.Items.Where(x => x.CategoryID == rule.CategoryID).ToList();

            if (items.Count > 0 && items.Sum(x => x.Quantity) >= rule.MinQuantity)
            {

                //foreach (var item in items)
                //{
                //    if (!DiscountService.LimitCheck(lim, curdiscount.Limit, curdiscount.LimitType))
                //    {
                //        break;
                //    }
                //    if (rule.DiscountType == CategoryDiscountConfigType.ForItemPrice)
                //    {
                //        if (rule.IsPercent)
                //        {
                //            item.Price = item.Price - (item.Price * rule.Percent / 100);
                //        }
                //        else
                //        {
                //            item.Price = item.Price - rule.Amount;
                //        }
                //        if (item.Price < 0) { item.Price = 0; }
                //    }
                //    else
                //    {
                //        if (rule.IsPercent)
                //        {
                //            item.TotalDiscountAmount =  item.Price * rule.Percent * item.Quantity / 100;
                //        }
                //        else
                //        {
                //            item.TotalDiscountAmount = rule.Amount;
                //        }

                //    }
                //    lim++;

                //}

                return true;
            }

            return false;
        }
        public object GetConfigItem()
        {
            return new CategoryDiscountConfig();
        }
    }
    public class CategoryDiscountConfig
    {
        [Model(Show = false, Edit = false)]
        public int CategoryID { get; set; }

        [NotMapped]
        [Model(Show = false, Edit = true, AjaxEdit = false)]
        public virtual Category Category { get; set; }
        public int MinQuantity { get; set; }

        //public CategoryDiscountConfigType DiscountType { get; set; }
        //public bool IsPercent { get; set; }
        //public decimal Percent { get; set; }
        //public decimal Amount { get; set; }

    }

}
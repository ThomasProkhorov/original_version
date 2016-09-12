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
    public class CartSubTotalDiscount : IDiscountRule
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
            var rule = (CartSubTotalDicountConfig)DiscountConfigItem;


            if (model.Items.Select(x => x.Price * x.Quantity).DefaultIfEmpty(0).Sum() >= rule.MinAmount)
            {
                return true;
            }

            return false;
        }
        public object GetConfigItem()
        {
            return new CartSubTotalDicountConfig();
        }
    }
    public class CartSubTotalDicountConfig
    {
        public decimal MinAmount { get; set; }

    }
}
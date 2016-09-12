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
    public class ProductQuantityDiscount : IDiscountRule
    {
        //  public string Title = "Percent or fixed amount discount";
        //public DiscountType Type { 
        //    get{
        //return DiscountType.ForCartItem;
        //} 
        //}



        public bool Process(ShoppingCartOverviewModel model, Discount curdiscount, object DiscountConfigItem, User user, int lim)
        {

            var rule = (QuantityDicountConfig)DiscountConfigItem;
            var items = model.Items.Where(x => x.ProductShopID == rule.ProductShopID && x.Quantity >= rule.QuantityFrom).ToList();

            if (items.Count > 0)
            {

                return true;

            }

            return false;
        }
        public object GetConfigItem()
        {
            return new QuantityDicountConfig();
        }
    }
    public class QuantityDicountConfig
    {
        [Model(Show = false, Edit = false)]
        public int ProductShopID { get; set; }

        [NotMapped]
        [Model(Show = false, Edit = true, AjaxEdit = false)]
        public virtual ProductShop ProductShop { get; set; }
        public int QuantityFrom { get; set; }

    }
}
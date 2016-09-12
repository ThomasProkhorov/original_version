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
    public class MustHaveAllOfThisDiscount : IDiscountRule
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
            var rule = (MustHaveAllOfThisDiscountConfig)DiscountConfigItem;

            var musthave = new List<string>();
            if (rule.ProductShopIDs != null)
            {
                musthave = rule.ProductShopIDs.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
                .Select(x => x.Trim())
                .ToList();
            }


            foreach (var item in model.Items)
            {
                if (musthave.Contains(item.ProductShopID.ToString()))
                {
                    musthave.Remove(item.ProductShopID.ToString());
                }
                if (musthave.Count == 0)
                {
                    return true;
                }

            }
            if (musthave.Count == 0)
            {
                return true;
            }

            return false;
        }
        public object GetConfigItem()
        {
            return new MustHaveAllOfThisDiscountConfig();
        }
    }

    public class MustHaveAllOfThisDiscountConfig
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
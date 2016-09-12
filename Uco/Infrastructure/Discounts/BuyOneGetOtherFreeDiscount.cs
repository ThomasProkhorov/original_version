using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;
using Uco.Infrastructure.Livecycle;
using Uco.Infrastructure.Services;
using Uco.Models;
using Uco.Models.Overview;

namespace Uco.Infrastructure.Discounts
{
    public class BuyOneGetOtherFreeDiscount : IDiscountRule
    {
        //  public string Title = "Percent or fixed amount discount";
        //public DiscountType Type
        //{
        //    get
        //    {
        //        return DiscountType.ForCartTotal;
        //    }
        //}
        public bool Process(ShoppingCartOverviewModel model, Discount curdiscount, object DiscountConfigItem, User user, int lim)
        {
            var rule = (BuyOneGetOtherFreeDicountConfig)DiscountConfigItem;
            var productsList = curdiscount.GetProductsList();
            var totalQuantity = model.Items.Where(x => productsList.Contains(x.ProductShopID.ToString())).Select(x => x.Quantity)
                .DefaultIfEmpty(0).Sum();

            if (totalQuantity >= rule.QuantityFrom)
            {
                var items = model.Items.Where(x => rule.FreeProductShopID == x.ProductShopID)
                .ToList();
                if (items.Count == 0)
                {
                    if (rule.AutoAddToCart)
                    {
                        var cartItem = new ShoppingCartItem()
                        {
                            ProductShopID = rule.FreeProductShopID,
                            Quantity = 1,
                            ShopID = model.ShopID
                        };
                        ShoppingCartService.AddToCart(user.ID, cartItem);
                        var cartItemModel = ShoppingCartService.GetShoppingCartItemByID(cartItem.ID);
                        model.Items.Add(cartItemModel);
                        items.Add(cartItemModel);
                    }
                }
                if (items.Count > 0)
                {

                    foreach (var item in items)
                    {
                        if (!DiscountService.LimitCheck(lim, curdiscount.Limit, curdiscount.LimitType))
                        {
                            break;
                        }
                        if (item.Quantity <= rule.MaxFreeQuantity)
                        {
                            item.Price = 0;// show that price is zero
                        }
                        else
                        {
                            item.TotalDiscountAmount += item.Price * rule.MaxFreeQuantity;// can be only one free, so we change unit price
                        }
                        lim++;

                    }

                    return true;
                }
            }

            return false;
        }
        public object GetConfigItem()
        {
            return new BuyOneGetOtherFreeDicountConfig();
        }
    }
    public class BuyOneGetOtherFreeDicountConfig
    {
        //  [Model(Show = false, Edit = false)]
        //  public int ProductShopID { get; set; }

        //[NotMapped]
        //[Model(Show = false, Edit = true, AjaxEdit = false)]
        //public virtual ProductShop ProductShop { get; set; }
        public int QuantityFrom { get; set; }


        [Model(Show = false, Edit = false)]
        public int FreeProductShopID { get; set; }

        [NotMapped]
        [Model(Show = false, Edit = true, AjaxEdit = false)]
        public virtual ProductShop FreeProductShop { get; set; }

        public int MaxFreeQuantity { get; set; }

        public bool AutoAddToCart { get; set; }
    }
}
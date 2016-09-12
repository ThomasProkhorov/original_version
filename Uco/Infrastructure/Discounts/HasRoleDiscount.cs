using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web;
using Uco.Infrastructure.Livecycle;
using Uco.Models;
using Uco.Models.Overview;

namespace Uco.Infrastructure.Discounts
{
    public class HasRoleDiscount : IDiscountRule
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
            var rule = (HasRoleDiscountConfig)DiscountConfigItem;


            if (LS.CurrentUser.Roles != null && LS.CurrentUser.Roles.Contains(rule.Role))
            {
                return true;
            }

            return false;
        }
        public object GetConfigItem()
        {
            return new HasRoleDiscountConfig();
        }

    }
    public class HasRoleDiscountConfig
    {
        [UIHint("RoleDefault")]
        public string Role { get; set; }

    }
}
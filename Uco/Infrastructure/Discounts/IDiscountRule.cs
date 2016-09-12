using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Uco.Models;
using Uco.Models.Overview;

namespace Uco.Infrastructure.Discounts
{
    public interface IDiscountRule
    {
        //DiscountType Type { get;  }
        bool Process(ShoppingCartOverviewModel model, Discount discount, object DiscountConfigItem, User user, int limit);

        object GetConfigItem();
    }
}
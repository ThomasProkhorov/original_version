using System;
using System.Collections.Generic;
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
    public class CartWeigthDiscount : IDiscountRule
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
            var rule = (CartWeigthDiscountConfig)DiscountConfigItem;


            if (model.Items.Where(x => x.SoldByWeight && x.MeasureUnit != null)
                .Select(x => ExtractDecimal(x.Capacity) * x.Quantity).DefaultIfEmpty(0).Sum() >= rule.MinWeigthKg)
            {
                //  קג - Kg
                //  ALL in Kg
                return true;
            }

            return false;
        }
        public object GetConfigItem()
        {
            return new CartWeigthDiscountConfig();
        }
        private decimal ExtractDecimal(string text)
        {
            Regex regex = new Regex(@"^-?\d+(?:\.\d+)?");
            Match match = regex.Match(text);
            decimal weight = 0;
            if (match.Success)
            {
                weight = decimal.Parse(match.Value, CultureInfo.InvariantCulture);
            }
            return weight;
        }
    }
    public class CartWeigthDiscountConfig
    {
        public decimal MinWeigthKg { get; set; }

    }
}
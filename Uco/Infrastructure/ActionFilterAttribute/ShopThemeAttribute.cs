using System.Globalization;
using System.Web.Mvc;
using System.Linq;
using System;
using Uco.Infrastructure.Livecycle;
using Uco.Models;
using System.Web;

namespace Uco.Infrastructure
{
    public class ShopThemeAttribute : ActionFilterAttribute
    {

        string _nameParamShopId = "";
        bool _findTheme;

        public ShopThemeAttribute(string nameParamShopId, bool findTheme = true)
        {
            _nameParamShopId = nameParamShopId;
            _findTheme = findTheme;
        }

        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            if (!_findTheme)
                return;
            var httpContext = filterContext.HttpContext;

            if (httpContext.Request.HttpMethod == "GET")
            {
                if (httpContext.Request.QueryString.AllKeys.Contains(_nameParamShopId))
                {
                    int shopId = 0;
                    if (Int32.TryParse(httpContext.Request.QueryString[_nameParamShopId], out shopId))
                    {
                        SetShopTheme(shopId, httpContext);
                    } 
                }
            }

            if (httpContext.Request.HttpMethod == "POST")
            {
                if (httpContext.Request.Form.AllKeys.Contains(_nameParamShopId))
                {
                    int shopId = 0;
                    if (Int32.TryParse(httpContext.Request.Form[_nameParamShopId], out shopId))
                    {
                        SetShopTheme(shopId, httpContext);
                    }
                }
            }

            base.OnActionExecuting(filterContext);
        }

        private void SetShopTheme(int shopId,HttpContextBase httpContext)
        {
            var shop = LS.GetFirst<Shop>(x => x.ID == shopId);
            if (shop != null && !string.IsNullOrEmpty(shop.Theme))
            {
                httpContext.Items["ShopTheme"] = shop.Theme;
            }
        }
    }
}
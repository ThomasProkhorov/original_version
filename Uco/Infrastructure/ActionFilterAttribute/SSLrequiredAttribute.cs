using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Uco.Infrastructure
{
    public class SSLrequiredAttribute : ActionFilterAttribute
    {
        public bool MobileOnly { get; set; }

            public override void OnActionExecuting(ActionExecutingContext filterContext)
            {
                if (!filterContext.HttpContext.Request.IsSecureConnection 
                    && string.Equals(filterContext.HttpContext.Request.HttpMethod, "GET", StringComparison.OrdinalIgnoreCase))
                {
                    if (!MobileOnly || filterContext.HttpContext.Request.Browser.IsMobileDevice)
                    {
                        var url = filterContext.HttpContext.Request.Url.ToString().Replace("http:", "https:");
                        filterContext.Result = new RedirectResult(url);
                    }
                }
            }
        
    }
}
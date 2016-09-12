using System;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web.Mvc;
using System.Web.Routing;
using System.Web.Security;
using Uco.Infrastructure;
using Uco.Infrastructure.Livecycle;
using Uco.Infrastructure.Repositories;

namespace Uco.Models
{
    public class BaseActionController : BaseController
    {
        protected override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            if (CurrentSettings == null)
            {
                filterContext.Result = new RedirectResult("~/Error.html");
                return;
            }

            if (filterContext.RouteData.Values.Count(r => r.Key == "theme") == 0)
                filterContext.RouteData.Values.Add("theme", CurrentSettings.Themes);

            base.OnActionExecuting(filterContext);
        }
    }
}
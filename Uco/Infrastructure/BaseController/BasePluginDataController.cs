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
    public class BasePluginDataController : BaseController
    {
        protected override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            if (filterContext.RouteData.Values["plugin"] == null)
            {
                string Namespace = filterContext.Controller.GetType().Namespace;
                Namespace = Namespace.Replace(".Controllers", "");
                filterContext.RouteData.Values.Add("plugin", Namespace);
            }
            base.OnActionExecuting(filterContext);
        }

    }
}
using System;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using System.Web.Security;
using Uco.Infrastructure;
using Uco.Infrastructure.Livecycle;
using Uco.Infrastructure.Repositories;
using Uco.Models;

namespace Uco.Models
{
    public class BaseController : Controller
    {
        public Db _db;

        protected override void Initialize(System.Web.Routing.RequestContext requestContext)
        {
            base.Initialize(requestContext);
            _db = LS.CurrentEntityContext;
            CultureInfo culture = new CultureInfo("he-IL");
            culture.DateTimeFormat.ShortDatePattern = "dd/MM/yyyy";
            System.Threading.Thread.CurrentThread.CurrentCulture = culture;
            System.Threading.Thread.CurrentThread.CurrentUICulture = culture;
        }

        private Settings _CurrentSettings = null;
        public Settings CurrentSettings
        {
            get
            {
                if (_CurrentSettings != null) return _CurrentSettings;

                _CurrentSettings = RP.GetCurrentSettings();
                return _CurrentSettings;
            }
        }
        /// <summary>
        /// Render partial view to string
        /// </summary>
        /// <param name="viewName">View name</param>
        /// <param name="model">Model</param>
        /// <returns>Result</returns>
        public virtual string RenderPartialViewToString(string viewName, object model)
        {
            //Original source code: http://craftycodeblog.com/2010/05/15/asp-net-mvc-render-partial-view-to-string/
            if (string.IsNullOrEmpty(viewName))
                viewName = this.ControllerContext.RouteData.GetRequiredString("action");

            this.ViewData.Model = model;

            using (var sw = new StringWriter())
            {
                ViewEngineResult viewResult = System.Web.Mvc.ViewEngines.Engines.FindPartialView(this.ControllerContext, viewName);
                var viewContext = new ViewContext(this.ControllerContext, viewResult.View, this.ViewData, this.TempData, sw);
                viewContext.ViewData = ViewData;
                
                viewResult.View.Render(viewContext, sw);

                return sw.GetStringBuilder().ToString();
            }
        }

        protected  string GetAreaName(RouteData routeData)
        {
            object area;

            if (routeData.DataTokens.TryGetValue("area", out area))
            {
                return area as string;
            }

            return GetAreaName(routeData.Route);
        }
        protected  string GetAreaName(RouteBase route)
        {
            IRouteWithArea routeWithArea = route as IRouteWithArea;

            if (routeWithArea != null)
            {
                return routeWithArea.Area;
            }

            Route castRoute = route as Route;

            if (castRoute != null && castRoute.DataTokens != null)
            {
                return castRoute.DataTokens["area"] as string;
            }

            return null;
        }
    }

    
    
    /// <summary>
    /// Factoty for generic controller
    /// </summary>
    public class My_Controller_Factory : DefaultControllerFactory
    {
        /// <summary>
        /// Get area name
        /// </summary>
        /// <param name="routeData">RouteData</param>
        /// <returns>string</returns>
        private static string GetAreaName(RouteData routeData)
        {
            object area;

            if (routeData.DataTokens.TryGetValue("area", out area))
            {
                return area as string;
            }

            return GetAreaName(routeData.Route);
        }
        private static string GetAreaName(RouteBase route)
        {
            IRouteWithArea routeWithArea = route as IRouteWithArea;

            if (routeWithArea != null)
            {
                return routeWithArea.Area;
            }

            Route castRoute = route as Route;

            if (castRoute != null && castRoute.DataTokens != null)
            {
                return castRoute.DataTokens["area"] as string;
            }

            return null;
        }
        public override IController CreateController
        (System.Web.Routing.RequestContext requestContext, string controllerName)         {
            string controllername = requestContext.RouteData.Values
            ["controller"].ToString();

            //if controller == Generic : /{Manage}/{Model}/{Action}/{Id}
            if (controllerName == "Generic" && requestContext.RouteData.Values
           ["model"] != null)
            {
                
                //check if model exist in Uco.Models context
                string model = requestContext.RouteData.Values
           ["model"].ToString();
                var t = Type.GetType("Uco.Models." + model + ",Uco",false,true);
                if (t != null)
                {
                    //create controller GenericController<T>() and return
                    var area = GetAreaName(requestContext.RouteData);
                    if (area != null)
                    {
                        var gc = Type.GetType("Uco.Areas."+area+".Controllers.GenericController`1");

                        Type GcontrollerType = gc.MakeGenericType(t);

                        var c = Activator.CreateInstance(GcontrollerType) as IController;
                        HttpContext.Current.Items["controllerInstance"] = c;
                        return c;
                    }
                }
             //if not found set route value to default state /{controller}/{action}/{id}
                requestContext.RouteData.Values
            ["controller"] = model;
                controllerName = model;
            }

            //return default factory
            var defc = base.CreateController(requestContext, controllerName);
            HttpContext.Current.Items["controllerInstance"] = defc;
            return defc;
                  
    }
        public override void ReleaseController(IController controller)
        {
            IDisposable dispose = controller as IDisposable; if (dispose != null)
            {
                dispose.Dispose();
            }
        }
    } 
}
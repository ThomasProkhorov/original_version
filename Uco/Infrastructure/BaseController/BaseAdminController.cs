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

    public class BaseAdminController : BaseController
    {
        protected override void Initialize(System.Web.Routing.RequestContext requestContext)
        {
            base.Initialize(requestContext);
            if (requestContext.HttpContext.Session["LangSelectList"] != null)
            {
                string LangSelectList = requestContext.HttpContext.Session["LangSelectList"] as string;
                if (!string.IsNullOrEmpty(LangSelectList))
                {
                    System.Globalization.CultureInfo culture = new System.Globalization.CultureInfo(LangSelectList);
                    System.Threading.Thread.CurrentThread.CurrentCulture = culture;
                    System.Threading.Thread.CurrentThread.CurrentUICulture = culture;
                }
            }
        }

        public User CurrentUser
        {
            get
            {
                return LS.CurrentUser;
            }
        }

        protected override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            if (AdminCurrentSettingsRepository == null)
            {
                filterContext.Result = new RedirectResult("~/Error.html");
                return;
            }

            string Plugin = string.Empty;
            if (filterContext.RouteData.DataTokens["Namespaces"] != null)
            {
                string[] Namespaces = filterContext.RouteData.DataTokens["Namespaces"] as string[];
                string Namespace = Namespaces[0];
                if (!string.IsNullOrEmpty(Namespace))
                {
                    //Uco.LongTailArticles.Areas.Admin.Controllers
                    string pattern = "Uco.*.Areas.*.Controllers";

                    if (Regex.IsMatch(Namespace, pattern))
                    {
                        int RemoveStart = Namespace.IndexOf(".Areas");
                        int RemoveCount = Namespace.Count() - RemoveStart;

                        Namespace = Namespace.Remove(RemoveStart, RemoveCount);
                        filterContext.RouteData.Values.Add("plugin", Namespace);
                        Plugin = Namespace;
                    }
                }
            }
            //sett Role for permission
            ViewBag.CurrentRole = "Admin";
            base.OnActionExecuting(filterContext);
        }

        private Settings _AdminCurrentSettingsRepository = null;
        public Settings AdminCurrentSettingsRepository
        {
            get
            {
                if (_AdminCurrentSettingsRepository != null) return _AdminCurrentSettingsRepository;
                _AdminCurrentSettingsRepository = RP.GetAdminCurrentSettingsRepository();
                return _AdminCurrentSettingsRepository;
            }
        }
    }

    public static partial class ControllerHelper
    {
        public static void Initialize(System.Web.Routing.RequestContext requestContext)
        {
            
            if (requestContext.HttpContext.Session["LangSelectList"] != null)
            {
                string LangSelectList = requestContext.HttpContext.Session["LangSelectList"] as string;
                if (!string.IsNullOrEmpty(LangSelectList))
                {
                    System.Globalization.CultureInfo culture = new System.Globalization.CultureInfo(LangSelectList);
                    System.Threading.Thread.CurrentThread.CurrentCulture = culture;
                    System.Threading.Thread.CurrentThread.CurrentUICulture = culture;
                }
            }
        }
        public static void OnActionExecuting( BaseController controller ,ActionExecutingContext filterContext, dynamic ViewBag)
        {
           
            string Plugin = string.Empty;
            if (filterContext.RouteData.DataTokens["Namespaces"] != null)
            {
                string[] Namespaces = filterContext.RouteData.DataTokens["Namespaces"] as string[];
                string Namespace = Namespaces[0];
                if (!string.IsNullOrEmpty(Namespace))
                {
                    //Uco.LongTailArticles.Areas.Admin.Controllers
                    string pattern = "Uco.*.Areas.*.Controllers";

                    if (Regex.IsMatch(Namespace, pattern))
                    {
                        int RemoveStart = Namespace.IndexOf(".Areas");
                        int RemoveCount = Namespace.Count() - RemoveStart;

                        Namespace = Namespace.Remove(RemoveStart, RemoveCount);
                        filterContext.RouteData.Values.Add("plugin", Namespace);
                        Plugin = Namespace;
                    }
                }
            }
            //sett Role for permission
            ViewBag.CurrentRole = "Member";

            var attr = (AuthorizeAttribute)controller.GetType().GetCustomAttributes(typeof(AuthorizeAttribute), true).FirstOrDefault();
            if (attr != null)
            {
                ViewBag.CurrentRole = attr.Roles;
            }
           
        }
    }

    public class BaseGenericAdminController<T> : BaseGenericController<T> where T: class
    {
        protected override void Initialize(System.Web.Routing.RequestContext requestContext)
        {
            base.Initialize(requestContext);
            ControllerHelper.Initialize(requestContext);
        }

        public User CurrentUser
        {
            get
            {
                return LS.CurrentUser;
            }
        }

        protected override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            if (AdminCurrentSettingsRepository == null)
            {
                filterContext.Result = new RedirectResult("~/Error.html");
                return;
            }

            ControllerHelper.OnActionExecuting(this,filterContext, ViewBag);
            base.OnActionExecuting(filterContext);
        }

        private Settings _AdminCurrentSettingsRepository = null;
        public Settings AdminCurrentSettingsRepository
        {
            get
            {
                if (_AdminCurrentSettingsRepository != null) return _AdminCurrentSettingsRepository;
                _AdminCurrentSettingsRepository = RP.GetAdminCurrentSettingsRepository();
                return _AdminCurrentSettingsRepository;
            }
        }
    }

    public class BaseMemberGemericController<T> : BaseGenericController<T> where T: class
    {
        protected override void Initialize(System.Web.Routing.RequestContext requestContext)
        {
            base.Initialize(requestContext);
            ControllerHelper.Initialize(requestContext);
        }

        public User CurrentUser
        {
            get
            {
                return LS.CurrentUser;
            }
        }
       public Shop CurrentShop
        {
            get
            {
                return LS.CurrentShop;
            }
        }
        protected override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            if (CurrentSettingsRepository == null)
            {
                filterContext.Result = new RedirectResult("~/Error.html");
                return;
            }

            ControllerHelper.OnActionExecuting(this, filterContext, ViewBag);
            ViewBag.AdditionalFilterField = "ShopID";
            ViewBag.AdditionalFilterID = 0;
            if(LS.CurrentShop!= null)
            {

                ViewBag.AdditionalFilterID = LS.CurrentShop.ID;
            }
            base.OnActionExecuting(filterContext);
        }

        private Settings _CurrentSettingsRepository = null;
        public Settings CurrentSettingsRepository
        {
            get
            {
                if (_CurrentSettingsRepository != null) return _CurrentSettingsRepository;
                _CurrentSettingsRepository = RP.GetCurrentSettings();
                return _CurrentSettingsRepository;
            }
        }
    }
    public class BaseMemberController : BaseController
    {
        protected override void Initialize(System.Web.Routing.RequestContext requestContext)
        {
            base.Initialize(requestContext);
            ControllerHelper.Initialize(requestContext);
        }

        public User CurrentUser
        {
            get
            {
                return LS.CurrentUser;
            }
        }
        private Shop _CurrentShop = null;
        protected void ResetCurrentShop()
        {
            _CurrentShop = null;
        }
        public Shop CurrentShop
        {
            get
            {
                object requestedShopID = TempData["_SwitchedShop"];
                if (requestedShopID != null)
                {
                    _CurrentShop = _db.Shops.Find(requestedShopID);
                }
                if (_CurrentShop == null)
                {
                    var curid = LS.CurrentShopID;
                        Shop ms = _db.Shops.FirstOrDefault(r => r.UserID == CurrentUser.ID
                            && r.ID == curid);
                        if (ms != null)
                        {
                            _CurrentShop = ms;
                            return _CurrentShop;
                        }
                    Shop m = _db.Shops.FirstOrDefault(r => r.UserID == CurrentUser.ID);
                    if (m == null) return null;
                    _CurrentShop = m;
                }
                return _CurrentShop;
            }
        }
        protected override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            if (CurrentSettingsRepository == null)
            {
                filterContext.Result = new RedirectResult("~/Error.html");
                return;
            }
            ViewBag.AdditionalFilterField = "ShopID";
            ViewBag.AdditionalFilterID = 0;
            if (LS.CurrentShop != null)
            {

                ViewBag.AdditionalFilterID = LS.CurrentShop.ID;
            }
            ControllerHelper.OnActionExecuting(this, filterContext, ViewBag);
          
            base.OnActionExecuting(filterContext);
        }

        private Settings _CurrentSettingsRepository = null;
        public Settings CurrentSettingsRepository
        {
            get
            {
                if (_CurrentSettingsRepository != null) return _CurrentSettingsRepository;
                _CurrentSettingsRepository = RP.GetCurrentSettings();
                return _CurrentSettingsRepository;
            }
        }
    }
}
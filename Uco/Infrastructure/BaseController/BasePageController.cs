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
    public class BasePageController : BaseController
    {
        protected override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            if (CurrentSettings == null)
            {
                filterContext.Result = new RedirectResult("~/DomainError.html");
                return;
            }
            if (filterContext.RouteData.Values["lang"] != null && !string.IsNullOrWhiteSpace(filterContext.RouteData.Values["lang"].ToString()))
            {
                CultureInfo culture = new CultureInfo(filterContext.RouteData.Values["lang"].ToString());
                System.Threading.Thread.CurrentThread.CurrentCulture = culture;
                System.Threading.Thread.CurrentThread.CurrentUICulture = culture;
            }
            else
            {
                CultureInfo culture = new CultureInfo(SF.GetLangCodeWebconfig());
                System.Threading.Thread.CurrentThread.CurrentCulture = culture;
                System.Threading.Thread.CurrentThread.CurrentUICulture = culture;
            }

            filterContext.RouteData.Values.Add("theme", CurrentSettings.Themes);

            string Plugin = string.Empty;
            if (filterContext.RouteData.DataTokens["Namespaces"] != null)
            {
                string[] Namespaces = filterContext.RouteData.DataTokens["Namespaces"] as string[];
                string Namespace = Namespaces[0];
                if (!string.IsNullOrEmpty(Namespace) && Namespace.StartsWith("Uco.") && Namespace.EndsWith(".Controllers") && Namespace != "Uco.Controllers")
                {
                    Namespace = Namespace.Replace(".Controllers", "");
                    filterContext.RouteData.Values.Add("plugin", Namespace);
                    Plugin = Namespace;
                }
            }

            if (CurrentPage != null)
            {
                ViewBag.Title = CurrentPage.SeoTitle != null && CurrentPage.SeoTitle != "" ? CurrentPage.SeoTitle : CurrentPage.Title;
                ViewBag.H1 = CurrentPage.SeoH1 != null && CurrentPage.SeoH1 != "" ? CurrentPage.SeoH1 : CurrentPage.Title;
                ViewBag.Keywords = CurrentPage.SeoKywords;
                ViewBag.Description = CurrentPage.SeoDescription;
                ViewBag.ID = CurrentPage.ID;
                ViewBag.Url = CurrentPage.Url;
                ViewBag.LanguageRTL = CurrentSettings.LanguageRTL;
                string theme = CurrentSettings.Themes;
                if (string.IsNullOrEmpty(theme)) ViewBag.Layout = "~/Views/Shared/" + CurrentPage.Layout;
                else ViewBag.Layout = "~/Themes/" + theme + "/Views/Shared/" + CurrentPage.Layout;

            }
            else
            {
                filterContext.Result = RedirectToAction("Error404", "Error");// RedirectResult("~/Error/Error404");
                return;
            }

            if (SF.UsePermissions() && !SF.UserCanView(CurrentUser, CurrentPage))
            {
                TempData["Error"] = "You don't have permissions to see this page";
                filterContext.Result = new RedirectResult("~/Account/LogOn?returnUrl=" + CurrentPage.Url);
                return;
            }

            base.OnActionExecuting(filterContext);
        }

        private AbstractPage _CurrentPage = null;
        public AbstractPage CurrentPage
        {
            get
            {
                if (_CurrentPage != null) return _CurrentPage;
                if (RouteData.Values["name"] == null) return null;
                string Url = (string)RouteData.Values["name"];
                if (Url == "root")
                {
                    _CurrentPage = _db.AbstractPages.FirstOrDefault(r => r.RouteUrl == "d" && r.Visible == true && r.DomainID == CurrentSettings.ID);
                    return _CurrentPage;
                }
                _CurrentPage = _db.AbstractPages.FirstOrDefault(r => r.SeoUrlName == Url && r.Visible == true && r.DomainID == CurrentSettings.ID);
                return _CurrentPage;
            }
        }

        public User CurrentUser
        {
            get
            {
                return LS.CurrentUser;
            }
        }
    }
}
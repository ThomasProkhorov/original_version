using System.Globalization;
using System.Web.Mvc;

namespace Uco.Infrastructure
{
    public class LocalizationAttribute : ActionFilterAttribute
    {
        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            if (filterContext.RouteData.Values["lang"] != null && !string.IsNullOrWhiteSpace(filterContext.RouteData.Values["lang"].ToString()))
            {
                CultureInfo culture = new CultureInfo(filterContext.RouteData.Values["lang"].ToString());
                culture.DateTimeFormat.ShortDatePattern = "dd/MM/yyyy";
                System.Threading.Thread.CurrentThread.CurrentCulture = culture;
                System.Threading.Thread.CurrentThread.CurrentUICulture = culture;
            
            }
            if (filterContext.HttpContext.Session["LangSelectList"] != null)
            {
                string LangSelectList = filterContext.HttpContext.Session["LangSelectList"] as string;
                if (!string.IsNullOrEmpty(LangSelectList))
                {
                    System.Globalization.CultureInfo culture = new System.Globalization.CultureInfo(LangSelectList);
                    culture.DateTimeFormat.ShortDatePattern = "dd/MM/yyyy";
                    System.Threading.Thread.CurrentThread.CurrentCulture = culture;
                    System.Threading.Thread.CurrentThread.CurrentUICulture = culture;
                }
            }
            base.OnActionExecuting(filterContext);
        }
    }
}
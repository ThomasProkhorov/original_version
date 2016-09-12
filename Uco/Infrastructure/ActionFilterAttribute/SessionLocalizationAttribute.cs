using System.Globalization;
using System.Web.Mvc;

namespace Uco.Infrastructure
{
    public class SessionLocalizationAttribute : ActionFilterAttribute
    {
        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            if (filterContext.HttpContext.Session["LangSelectList"] != null)
            {
                string LangSelectList = filterContext.HttpContext.Session["LangSelectList"] as string;
                if (!string.IsNullOrEmpty(LangSelectList))
                {
                    CultureInfo culture = new CultureInfo(LangSelectList);
                    System.Threading.Thread.CurrentThread.CurrentCulture = culture;
                    System.Threading.Thread.CurrentThread.CurrentUICulture = culture;
                }
            }
            base.OnActionExecuting(filterContext);
        }
    }
}
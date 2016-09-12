using System.Web.Mvc;

namespace Uco.Areas.Admin
{
    public class AdminAreaRegistration : AreaRegistration
    {
        public override string AreaName
        {
            get
            {
                return "Admin";
            }
        }

        public override void RegisterArea(AreaRegistrationContext context)
        {
            context.MapRoute(
                "Admin_generic",
                "Admin/Manage/{model}/{action}/{id}",
                new { controller = "Generic", model = "Main", action = "Index", id = UrlParameter.Optional }
            );
            //this will not calling never
            context.MapRoute(
                "Admin_default",
                "Admin/{controller}/{action}/{id}",
                new { controller = "Main", action = "Index", id = UrlParameter.Optional }
            );
        }
    }
}

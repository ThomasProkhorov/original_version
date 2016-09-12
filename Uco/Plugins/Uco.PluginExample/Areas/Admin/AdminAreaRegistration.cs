using System.Web.Mvc;

namespace Uco.LongTailArticles.Areas.Admin
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
               name: "Admin_Uco.PluginExample",
               url: "PluginExample/Admin/{controller}/{action}/{id}",
               defaults: new { controller = "Main", action = "Index", id = UrlParameter.Optional },
               namespaces: new string[] { "Uco.PluginExample.Areas.Admin.Controllers" }
            );
        }
    }
}

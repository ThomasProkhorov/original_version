using System.Web.Mvc;

namespace Uco.Areas.Member
{
    public class MemberAreaRegistration : AreaRegistration
    {
        public override string AreaName
        {
            get
            {
                return "Member";
            }
        }

        public override void RegisterArea(AreaRegistrationContext context)
        {
            context.MapRoute(
                "Member_generic",
                "Member/Manage/{model}/{action}/{id}",
                new { controller = "Generic", model = "Main", action = "Index", id = UrlParameter.Optional }
            );
            context.MapRoute(
                "Member_default",
                "Member/{controller}/{action}/{id}",
                new { controller = "Main", action = "Index", id = UrlParameter.Optional }
            );
        }
    }
}

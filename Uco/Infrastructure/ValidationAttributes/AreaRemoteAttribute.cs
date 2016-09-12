using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;

namespace Uco.Infrastructure
{
    public class AreaRemoteAttribute : RemoteAttribute
    {
        public AreaRemoteAttribute(string action, string controller, string area)
            : base(action, controller, area)
        {
            this.RouteData["area"] = area;
        }
    }
}
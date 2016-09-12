using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using Uco.Infrastructure.Livecycle;
using Uco.Models;

namespace Uco.Infrastructure
{
    public class AuthorizeAdminAttribute : AuthorizeAttribute
    {
        // Custom property
        //public string AccessLevel { get; set; }
        private Type _controlerType;
        private Type ControlerType
        {
            get { 
                return _controlerType; 
            }
            set { 
                _controlerType = value; 
            }
        }

        protected override bool AuthorizeCore(HttpContextBase httpContext)
        {
            var isAuthorized = base.AuthorizeCore(httpContext);
            if (!isAuthorized)
            {
                return false;
            }

            if (SF.UsePermissions())
            {
                if (!LS.CurrentUser.Roles.Contains("Admin") && SF.CheckAdminControlersPermissions(ControlerType) == false) return false;
                else return true;
            }
            else
            {
                if (!LS.CurrentUser.Roles.Contains("Admin")) return false;
                else return true;
            }
        }

        public override void OnAuthorization(AuthorizationContext filterContext)
        {
            ControlerType = filterContext.Controller.GetType();

            base.OnAuthorization(filterContext);
        }

        protected override void HandleUnauthorizedRequest(AuthorizationContext filterContext)
        {
            filterContext.Controller.TempData["Error"] = "You don't have permissions to see this page";

            filterContext.Result = new RedirectToRouteResult(
                new RouteValueDictionary(
                    new
                    {
                        controller = "Account",
                        action = "LogOn",
                        returnUrl = "/Admin",
                        Area = ""
                    })
                );
        }
    }
}
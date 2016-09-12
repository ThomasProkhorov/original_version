using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Kendo.Mvc.UI;
using System.IO;
using Uco.Infrastructure;
using Uco.Infrastructure.Livecycle;

namespace Uco.Areas.Admin.Controllers
{
    [AuthorizeAdmin]
    [SessionLocalization]
    public class ImageBrowserController : EditorImageBrowserController
    {
        private string contentFolderRoot = string.Format("~/Content/UserFiles/{0}/", LS.CurrentUser.RoleDefault);
        public override string ContentPath
        {
            get
            {
                return contentFolderRoot;
            }
        }

        protected override void Execute(System.Web.Routing.RequestContext requestContext)
        {
            if (LS.CurrentUser.UserName == "Anonymous")
            {
                TempData["Error"] = "You don't have permissions to see this page";
                requestContext.HttpContext.Response.Redirect("~/Account/LogOn?returnUrl=/Admin");
            }
            base.Execute(requestContext);
        }
    }
}

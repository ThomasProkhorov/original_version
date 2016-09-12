using System.Collections.Generic;
using System.IO;
using System.Web;
using System.Web.Mvc;
using Kendo.Mvc.UI;
using Uco.Infrastructure;
using Uco.Models;
using Uco.Infrastructure.Livecycle;

namespace Uco.Areas.Admin.Controllers
{
    [AuthorizeAdmin]
    public class UploadController : BaseAdminController
    {
        public ActionResult Save(IEnumerable<HttpPostedFileBase> attachments)
        {
            foreach (var file in attachments)
            {
                var fileName = Path.GetFileName(file.FileName);
                var physicalPath = Path.Combine(Server.MapPath(string.Format("~/Content/UserFiles/{0}/Upload", LS.CurrentUser.RoleDefault)), fileName);
                file.SaveAs(physicalPath);
            }
            return Content("");
        }
        public ActionResult Remove(string[] fileNames)
        {
            foreach (var fullName in fileNames)
            {
                var fileName = Path.GetFileName(fullName);
                var physicalPath = Path.Combine(Server.MapPath(string.Format("~/Content/UserFiles/{0}/Upload", LS.CurrentUser.RoleDefault)), fileName);
                if (System.IO.File.Exists(physicalPath) && !physicalPath.Contains("/Content/DesignFiles"))
                {
                    System.IO.File.Delete(physicalPath);
                }
            }
            return Content("");
        }

        public ActionResult SaveApp_Data(IEnumerable<HttpPostedFileBase> attachments)
        {
            string WorkFolder = "~/App_Data/temp/";
            string WorkFolderPath = Server.MapPath(WorkFolder);
            foreach (var file in attachments)
            {
                var fileName = Path.GetFileName(file.FileName);
                var physicalPath = Path.Combine(WorkFolderPath, fileName);
                file.SaveAs(physicalPath);
            }
            return Content("");
        }

        public ActionResult RemoveApp_Data(string[] fileNames)
        {
            string WorkFolder = "~/App_Data/temp/";
            string WorkFolderPath = Server.MapPath(WorkFolder);
            foreach (var fullName in fileNames)
            {
                var fileName = Path.GetFileName(fullName);
                var physicalPath = Path.Combine(WorkFolderPath, fileName);
                if (System.IO.File.Exists(physicalPath))
                {
                    System.IO.File.Delete(physicalPath);
                }
            }
            return Content("");
        }
    }
}
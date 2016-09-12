using System.Collections.Generic;
using System.IO;
using System.Web;
using System.Web.Mvc;

namespace Uco.Controllers
{
    //[Authorize]
    public partial class UploadUserController : Controller
    {
        public ActionResult Save(IEnumerable<HttpPostedFileBase> attachments)
        {
            string UserName = Uco.Infrastructure.SF.CleanUrl(User.Identity.Name);
            string UserFolder = "~/Content/UserFiles/Users/" + UserName;
            string UserFolderPath = Server.MapPath(UserFolder);
            CreateUserFolder(UserFolderPath);
            foreach (var file in attachments)
            {
                var fileName = Path.GetFileName(file.FileName);
                var physicalPath = Path.Combine(UserFolderPath, fileName);
                file.SaveAs(physicalPath);
            }
            return Content("");
        }

        public ActionResult Remove(string[] fileNames)
        {
            string UserName = Uco.Infrastructure.SF.CleanUrl(User.Identity.Name);
            string UserFolder = "~/Content/UserFiles/Users/" + UserName;
            string UserFolderPath = Server.MapPath(UserFolder);
            CreateUserFolder(UserFolderPath);
            foreach (var fullName in fileNames)
            {
                var fileName = Path.GetFileName(fullName);
                var physicalPath = Path.Combine(UserFolderPath, fileName);
                if (System.IO.File.Exists(physicalPath))
                {
                    System.IO.File.Delete(physicalPath);
                }
            }
            return Content("");
        }

        public ActionResult SaveAnonim(IEnumerable<HttpPostedFileBase> attachments)
        {
            if (System.Configuration.ConfigurationManager.AppSettings["EnableAnonimUpload"] != "true") return Content("Enable EnableAnonimUpload");
            string UserFolder = "~/Content/UserFiles/Users/";
            string UserFolderPath = Server.MapPath(UserFolder);
            CreateUserFolder(UserFolderPath);
            foreach (var file in attachments)
            {
                var fileName = Path.GetFileName(file.FileName);
                var physicalPath = Path.Combine(UserFolderPath, fileName);
                file.SaveAs(physicalPath);
            }
            return Content("");
        }

        public ActionResult RemoveAnonim(string[] fileNames)
        {
            string UserFolder = "~/Content/UserFiles/Users/";
            string UserFolderPath = Server.MapPath(UserFolder);
            CreateUserFolder(UserFolderPath);
            foreach (var fullName in fileNames)
            {
                var fileName = Path.GetFileName(fullName);
                var physicalPath = Path.Combine(UserFolderPath, fileName);
                if (System.IO.File.Exists(physicalPath))
                {
                    System.IO.File.Delete(physicalPath);
                }
            }
            return Content("");
        }

        private void CreateUserFolder(string UserFolderPath)
        {
            if (Directory.Exists(UserFolderPath)) return;
            DirectoryInfo di = Directory.CreateDirectory(UserFolderPath);
        }
    }
}
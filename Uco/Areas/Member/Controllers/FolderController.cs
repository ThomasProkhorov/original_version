using System.Collections.Generic;
using System.IO;
using System.Web;
using System.Web.Mvc;
using Kendo.Mvc.UI;
using Uco.Infrastructure;
using Uco.Models;
using Uco.Infrastructure.Livecycle;

namespace Uco.Areas.Shop.Controllers
{
    public partial class FolderController : BaseAdminController
    {
        public ActionResult Save(IEnumerable<HttpPostedFileBase> attachments, string name)
        {
            string UserFolder = string.Format("~/Content/UserFiles/{0}/{1}", LS.CurrentUser.RoleDefault, name);

            Folder folder = new Folder(name);
            string UserFolderPath = Server.MapPath(UserFolder);
            CreateUserFolder(UserFolderPath);
            foreach (var file in attachments)
            {
                if (!folder.AllowedFileExtentions.Contains(folder.GetFileExtentionNoDot(file.FileName))) continue;
                var fileName = Path.GetFileName(file.FileName);
                var physicalPath = Path.Combine(UserFolderPath, fileName);
                file.SaveAs(physicalPath);
            }
            return Content("");
        }

        public ActionResult Remove(string[] fileNames, string name)
        {
            string UserFolder = string.Format("~/Content/UserFiles/{0}/{1}", LS.CurrentUser.RoleDefault, name);
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

        public ActionResult RemoveFile(string FolderName, string FileName)
        {
            FileName = Path.GetFileName(FileName);
            string UserFolder = string.Format("~/Content/UserFiles/{0}/{1}", LS.CurrentUser.RoleDefault, FolderName);
            string UserFolderPath = Server.MapPath(UserFolder);
            CreateUserFolder(UserFolderPath);
            FileName = Server.UrlDecode(FileName);
            var physicalPath = Path.Combine(UserFolderPath, FileName);
            if (System.IO.File.Exists(physicalPath))
            {
                System.IO.File.Delete(physicalPath);
                return Content("");
            }
            else throw new HttpException(500, "error");
        }

        private void CreateUserFolder(string UserFolderPath)
        {
            if (Directory.Exists(UserFolderPath)) return;
            DirectoryInfo di = Directory.CreateDirectory(UserFolderPath);
        }

        public ActionResult _UpdateFolderDitails(string folder)
        {
            string UserFolder = string.Format("~/Content/UserFiles/{0}/{1}", LS.CurrentUser.RoleDefault, folder);

            Folder f = new Folder(folder);

            List<Uco.Models.Folder.FileItem> allImages = f.Folder2List();
            foreach (Uco.Models.Folder.FileItem item in allImages)
            {
                string order = Request["folder_div_order_" + Url.Encode(item.FileName)];
                string title = Request["folder_div_title_" + Url.Encode(item.FileName)];

                item.Title = title;

                if (!string.IsNullOrEmpty(order))
                {
                    int tempInt = 0;
                    int.TryParse(order, out tempInt);
                    if (tempInt != 0) item.Order = tempInt;
                }
            }

            bool done = f.List2Folder(allImages);

            return Content((done ? "Done" : "Error"));
        }
    }
}
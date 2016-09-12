using System.Collections.Generic;
using System.IO;
using System.Web;
using System.Web.Mvc;
using Uco.Infrastructure;
using Uco.Models;

namespace Uco.Controllers
{
    [Authorize]
    public partial class GalleryUserController : Controller
    {
        public ActionResult Save(IEnumerable<HttpPostedFileBase> attachments, string name)
        {
            string UserFolder = "~/Content/UserFiles/Users/" + name + "/";
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

        public ActionResult Remove(string[] fileNames, string name)
        {
            string UserFolder = "~/Content/UserFiles/Users/" + name + "/"; ;
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

        public ActionResult RemoveFile(string fullName, string name)
        {
            if (name.StartsWith("/")) name = name.Remove(0, 1);
            string folder = "/image?img=%2fcontent%2fuserfiles%2fusers%2f" + Server.UrlEncode(name + "/");
            folder = folder.ToLower();
            fullName = fullName.ToLower();
            if (!fullName.Contains(folder))
                throw new HttpException(500, "error");
            string file = fullName.Replace(folder, "");
            if (!file.Contains("&w="))
                throw new HttpException(500, "error");
            file = file.Remove(file.IndexOf("&w="));
            string UserFolder = "~/Content/UserFiles/Users/" + name + "/";
            string UserFolderPath = Server.MapPath(UserFolder);
            CreateUserFolder(UserFolderPath);
            file = Server.UrlDecode(file);
            var physicalPath = Path.Combine(UserFolderPath, file);
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

        public ActionResult _UpdateGalleryDitails(string folder)
        {
            string GalleryPath = Server.MapPath("~/Content/UserFiles/Users/" + folder + "/");
            List<ImageGalleryItem> allImages = SF.GalleryFile2List(GalleryPath, 120, 120, false, false);
            foreach (ImageGalleryItem item in allImages)
            {
                string order = Request["gallery_div_order_" + Url.Encode(item.FileName)];
                string title = Request["gallery_div_title_" + Url.Encode(item.FileName)];

                item.Title = title;

                if (!string.IsNullOrEmpty(order))
                {
                    int tempInt = 0;
                    int.TryParse(order, out tempInt);
                    if (tempInt != 0) item.Order = tempInt;
                }
            }

            bool done = SF.GalleryList2File(allImages, GalleryPath);

            return Content((done ? "בוצע" : "שגיאה"));
        }
    }
}
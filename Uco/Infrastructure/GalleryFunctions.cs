using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using Uco.Infrastructure.Livecycle;
using Uco.Models;

namespace Uco.Infrastructure
{
    public static partial class SF
    {
        public static bool GalleryList2File(List<ImageGalleryItem> allImages, string GalleryPath)
        {
            //create directory if not exist
            if (!Directory.Exists(GalleryPath)) Directory.CreateDirectory(GalleryPath);

            //create string
            string allImagesString = "";
            foreach (ImageGalleryItem item in allImages)
            {
                allImagesString = allImagesString + item.Order + "^" + item.FileName + "^^" + item.Title + "\n";
            }

            //create file
            string GalleryFilePath = GalleryPath + "info.txt";
            try
            {
                File.WriteAllText(GalleryFilePath, allImagesString);
                return true;
            }
            catch
            {
                return false;
            }
        }

        public static List<ImageGalleryItem> GalleryFile2List(string GalleryPath, int Width, int Height, bool ToCacheFile, bool ExactSize)
        {
            List<Uco.Models.ImageGalleryItem> allImages = new List<ImageGalleryItem>();

            //create directory if not exist
            if (!Directory.Exists(GalleryPath)) Directory.CreateDirectory(GalleryPath);

            //get all files in directory
            string RootPath = LS.CurrentHttpContext.Server.MapPath("~");
            string[] AllFiles = Directory.GetFiles(LS.CurrentHttpContext.Server.UrlDecode(GalleryPath));

            //add images from directory to allImages
            for (int i = 0; i < AllFiles.Length; i++)
            {
                AllFiles[i] = AllFiles[i].Replace(RootPath, "");
                AllFiles[i] = AllFiles[i].Replace("\\", "/").ToLower();
                if (AllFiles[i].EndsWith(".jpg") || AllFiles[i].EndsWith(".png") || AllFiles[i].EndsWith(".gif") || AllFiles[i].EndsWith(".bmp"))
                {
                    Uco.Models.ImageGalleryItem newImage = new Uco.Models.ImageGalleryItem();
                    newImage.BigImageUrl = "/" + AllFiles[i];
                    newImage.SmallImageUrl = SF.GetImageUrl("/" + AllFiles[i], Width, Height, ExactSize, ToCacheFile);
                    allImages.Add(newImage);
                }
            }

            //create info.txt in directory if not exist
            string GalleryFilePath = GalleryPath + "info.txt";
            if (!File.Exists(GalleryFilePath)) File.CreateText(GalleryFilePath).Close();

            //read info.txt to allImages
            string GalleryFile = File.ReadAllText(GalleryFilePath);
            if (!string.IsNullOrEmpty(GalleryFile))
            {
                foreach (string item in GalleryFile.Split('\n'))
                {
                    if (string.IsNullOrEmpty(item)) { continue; }
                    string[] t = item.Split('^');
                    if (t.Length == 4)
                    {
                        Uco.Models.ImageGalleryItem n = allImages.FirstOrDefault(r => r.FileName == t[1]);
                        if (n != null)
                        {
                            n.Title = t[3];
                            int tempInt = 0;
                            int.TryParse(t[0], out tempInt);
                            if (tempInt == 0) { tempInt = 100; }
                            n.Order = tempInt;
                        }
                    }
                }
            }

            return allImages.OrderBy(r => r.Order).ToList();
        }
    }
}
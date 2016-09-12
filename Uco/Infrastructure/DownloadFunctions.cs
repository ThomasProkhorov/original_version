using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web;
using Uco.Infrastructure.Livecycle;
using Uco.Infrastructure.Repositories;
using Uco.Models;

namespace Uco.Infrastructure
{
    public static partial class SF
    {
        #region Download Pic

        public static string DownloadPic(string Domain, string PicUrlNoDomain)
        {
            string PicPath = LS.CurrentHttpContext.Server.MapPath(PicUrlNoDomain);
            if (System.IO.File.Exists(PicPath)) return PicUrlNoDomain;

            using (System.Net.WebClient client = new System.Net.WebClient())
            {
                client.DownloadFile("http://" + Domain + PicUrlNoDomain, PicPath);
            }

            return PicUrlNoDomain;
        }

        public static string DownloadGallery(string Domain, string GalleryFilesUrlNoDomain, int ID)
        {
            string GalleryFiles = "";
            using (System.Net.WebClient client = new System.Net.WebClient())
            {
                GalleryFiles = new System.Net.WebClient().DownloadString("http://" + Domain + "/Images/GetGallery?title=gallery_" + ID);
            }

            string GalleryPath = LS.CurrentHttpContext.Server.MapPath("/Content/UserFiles/" + GalleryFilesUrlNoDomain);
            if (!System.IO.Directory.Exists(GalleryPath))
            {
                System.IO.Directory.CreateDirectory(GalleryPath);
            }

            if (string.IsNullOrEmpty(GalleryFiles)) return GalleryFilesUrlNoDomain;
            GalleryFiles = GalleryFiles.Replace("<br />", "^");

            foreach (string item in GalleryFiles.Split('^'))
            {
                if (string.IsNullOrEmpty(item)) continue;
                DownloadPic(Domain, item);
            }

            return GalleryFilesUrlNoDomain;
        }

        #endregion
    }
}
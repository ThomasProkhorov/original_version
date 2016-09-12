using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Web;
using System.Web.Mvc;
using Uco.Infrastructure.Repositories;

namespace Uco.Controllers
{
    public class ImagesController : Controller
    {
        public FileResult GetImage(string img, int w, int h, int t, int c)
        {
            img = Server.UrlDecode(img);
            string path = Server.MapPath("~" + img);
            if (!System.IO.File.Exists(path)) throw new HttpException(404, "Not found");

            var lastModServer = System.IO.File.GetLastWriteTimeUtc(path);
            string ifModifiedSince = HttpContext.Request.Headers["If-Modified-Since"];

            if (ifModifiedSince != null && lastModServer <= DateTime.Parse(ifModifiedSince))
            {
                Response.StatusCode = 304;
                Response.StatusDescription = "Not Modified";
                Response.AddHeader("Content-Length", "0");
                return null;
            }
            HttpContext.Response.Cache.SetLastModified(lastModServer);

            string thumbnail = Server.MapPath("~" + GetThumbnailUrl(img, w, h));
            if (System.IO.File.Exists(thumbnail)) return base.File(thumbnail, GetContentType(thumbnail));
            else
            {
                using (FileStream fs = new FileStream(path, FileMode.Open, FileAccess.Read))
                {
                    using (Image photoImg = Image.FromStream(fs))
                    {
                        if (t == 0)
                        {
                            double resizeRation = GetResizeRatio(photoImg.Width, photoImg.Height, w, h);
                            w = (int)Math.Round(photoImg.Width * resizeRation);
                            h = (int)Math.Round(photoImg.Height * resizeRation);
                            if (w == 0) w = 1;
                            if (h == 0) h = 1;
                        }

                        Image photoImgNew = RezizeImage(photoImg, w, h);

                        if (c == 1)
                        {
                            photoImgNew.Save(thumbnail);
                            return base.File(thumbnail, GetContentType(thumbnail));
                        }
                        else
                        {
                            using (System.IO.MemoryStream ms = new System.IO.MemoryStream())
                            {
                                photoImgNew.Save(ms, System.Drawing.Imaging.ImageFormat.Jpeg);
                                return base.File(ms.ToArray(), "image/jpeg");
                            }
                        }
                    }
                }
            }
        }

        private Image RezizeImage(Image img, int maxWidth, int maxHeight)
        {
            if (img.Height < maxHeight && img.Width < maxWidth) return img;
            using (img)
            {
                Bitmap cpy = new Bitmap(maxWidth, maxHeight, PixelFormat.Format32bppArgb);
                using (Graphics gr = Graphics.FromImage(cpy))
                {
                    gr.Clear(Color.Transparent);

                    // This is said to give best quality when resizing images
                    gr.InterpolationMode = InterpolationMode.HighQualityBicubic;

                    gr.DrawImage(img,
                        new Rectangle(0, 0, maxWidth, maxHeight),
                        new Rectangle(0, 0, img.Width, img.Height),
                        GraphicsUnit.Pixel);
                }
                return cpy;
            }
        }

        private string GetContentType(string filename)
        {
            FileInfo file = new FileInfo(filename);
            switch (file.Extension.ToUpper())
            {
                case ".PNG": return "image/png";
                case ".JPG": return "image/jpeg";
                case ".JPEG": return "image/jpeg";
                case ".GIF": return "image/gif";
                case ".BMP": return "image/bmp";
                case ".TIFF": return "image/tiff";
                default: throw new NotSupportedException("The Specified File Type Is Not Supported");
            }
        }

        private double GetResizeRatio(int CurentWidth, int CurentHeight, int MaxWidth, int MaxHeight)
        {
            double ratioY = ((double)MaxHeight) / ((double)CurentHeight);
            double ratioX = ((double)MaxWidth) / ((double)CurentWidth);

            double ratio = Math.Min(ratioX, ratioY);
            if (ratio == 0) ratio = Math.Max(ratioX, ratioY);
            if (ratio <= 0 || ratio > 1) ratio = 1;
            return ratio;
        }

        private string GetThumbnailUrl(string img, int w, int h)
        {
            string ext = Path.GetExtension(img);
            string imageUrlNoExt = img.Remove(img.Length - ext.Length);
            imageUrlNoExt = imageUrlNoExt.Replace("/", "_");
            return "/App_Data/cache/images/" + RP.GetCurrentSettings().ID + "/" + imageUrlNoExt + "_" + w + "_" + h + ext;
        }
    }
}

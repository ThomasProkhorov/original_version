using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.Hosting;
using System.Web.Mvc;
using Uco.Infrastructure.Livecycle;
using Uco.Infrastructure.Repositories;
using Uco.Models;

namespace Uco.Infrastructure
{
    public static partial class SF
    {
        public static object _lock = new object();
        public static string GetImageUrl(string Pic, int Width, int Height, bool ExactSize, bool ToCacheFile)
        {
            var key = Pic+"_"+Width.ToString()+"_"+Height.ToString()+"_" + (ExactSize ? "1":"0" );
           if(LS.Cache["ImageCache"] == null)
            {
                LS.Cache["ImageCache"] = new Dictionary<string, string>();
            }
            Dictionary<string, string> imgCache = (Dictionary<string, string>)LS.Cache["ImageCache"];
            lock (_lock)
            {
                if (!imgCache.ContainsKey(key))
                {
                     var returnUrl = GetImage(Pic,Width,Height,ExactSize,ToCacheFile);

                     imgCache.Add(key, returnUrl);
                }
            }
            return imgCache[key];
            return "~/Image?img=" + HttpUtility.UrlEncode(Pic) + "&w=" + Width + "&h=" + Height + "&t=" + (ExactSize ? "1" : "0") + "&c=" + (ToCacheFile ? "1" : "0");
        }
        public static string GetImage(string img, int w, int h, bool t, bool c,int noimagesize=0)
        {
            if(string.IsNullOrEmpty(img))
            {
                img = "";
            }
            img = HttpUtility.UrlDecode(img);
            string path = HostingEnvironment.MapPath("~" + img);
            if (!System.IO.File.Exists(path))
            {
                img = "/Content/DesignFiles/noimage.jpg"; //default.jpg";
                path = HostingEnvironment.MapPath("~" + img);
                if(noimagesize > 0)
                {
                    w = noimagesize;
                    h = noimagesize;
                }
            }
            if (!System.IO.File.Exists(path))
            {
                return "/Content/DesignFiles/noimage.jpg"; //default.jpg"; // throw new HttpException(404, "Not found");
            }
            //var lastModServer = System.IO.File.GetLastWriteTimeUtc(path);
            //string ifModifiedSince = HttpContext.Request.Headers["If-Modified-Since"];

            //if (ifModifiedSince != null && lastModServer <= DateTime.Parse(ifModifiedSince))
            //{
            //    Response.StatusCode = 304;
            //    Response.StatusDescription = "Not Modified";
            //    Response.AddHeader("Content-Length", "0");
            //    return null;
            //}
            //HttpContext.Response.Cache.SetLastModified(lastModServer);
            try
            {
                var ImageUrl = GetThumbnailUrl(img, w, h);
                string thumbnail = HostingEnvironment.MapPath("~" + ImageUrl);
                if (System.IO.File.Exists(thumbnail))
                {
                    var less = File.GetLastWriteTimeUtc(path);
                    var bigger = File.GetLastWriteTimeUtc(thumbnail);
                    if (less < bigger)
                    {
                        return ImageUrl;
                    }
                }
               
               
                    using (FileStream fs = new FileStream(path, FileMode.Open, FileAccess.Read))
                    {
                        using (Image photoImg = Image.FromStream(fs))
                        {
                            if (t)
                            {
                                double resizeRation = GetResizeRatio(photoImg.Width, photoImg.Height, w, h);
                                w = (int)Math.Round(photoImg.Width * resizeRation);
                                h = (int)Math.Round(photoImg.Height * resizeRation);
                                if (w == 0) w = 1;
                                if (h == 0) h = 1;
                            }

                            Image photoImgNew = RezizeImage(photoImg, w, h);

                            // if (c == 1)
                            // {
                            photoImgNew.Save(thumbnail);
                            return ImageUrl;
                            //    return base.File(thumbnail, GetContentType(thumbnail));
                            // }
                            // else
                            //  {
                            //    using (System.IO.MemoryStream ms = new System.IO.MemoryStream())
                            //     {
                            //     photoImgNew.Save(ms, System.Drawing.Imaging.ImageFormat.Jpeg);
                            //     return base.File(ms.ToArray(), "image/jpeg");
                            //   }
                            // }
                        }
                    }
                
            }
            catch
            {
                return  "/Content/DesignFiles/noimage.jpg";;
            }
        }
        private static Image RezizeImage(Image img, int maxWidth, int maxHeight)
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

        private static string GetContentType(string filename)
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

        private static double GetResizeRatio(int CurentWidth, int CurentHeight, int MaxWidth, int MaxHeight)
        {
            double ratioY = ((double)MaxHeight) / ((double)CurentHeight);
            double ratioX = ((double)MaxWidth) / ((double)CurentWidth);

            double ratio = Math.Min(ratioX, ratioY);
            if (ratio == 0) ratio = Math.Max(ratioX, ratioY);
            if (ratio <= 0 || ratio > 1) ratio = 1;
            return ratio;
        }
        private static string GetThumbnailUrl(string img, int w, int h)
        {

            string ext = Path.GetExtension(img);
            string imageUrlNoExt = img.Remove(img.Length - ext.Length);
            imageUrlNoExt = imageUrlNoExt.Replace("/", "_");
            string folder = HostingEnvironment.MapPath("~" + "/Cache/Images/" + RP.GetCurrentSettings().ID);
            if (!Directory.Exists(folder))
            {
                System.IO.Directory.CreateDirectory(folder);
            }
            return "/Cache/Images/" + RP.GetCurrentSettings().ID + "/" + imageUrlNoExt + "_" + w + "_" + h + ext;
        }
        /// <summary>
        /// Sets a value in an object, used to hide all the logic that goes into
        ///     handling this sort of thing, so that is works elegantly in a single line.
        /// </summary>
        /// <param name="target"></param>
        /// <param name="propertyName"></param>
        /// <param name="propertyValue"></param>
        public static void SetValueFromString(this object target, string propertyName, string propertyValue)
        {
            PropertyInfo oProp = target.GetType().GetProperty(propertyName);
            Type tProp = oProp.PropertyType;

            //Nullable properties have to be treated differently, since we 
            //  use their underlying property to set the value in the object

            if (oProp.PropertyType.BaseType.Name == "Enum")
            {
                var val = Enum.ToObject(oProp.PropertyType, Convert.ToInt32(propertyValue));
                oProp.SetValue(target, val, null);
                return;
            }else
            if (tProp.IsGenericType
                && tProp.GetGenericTypeDefinition().Equals(typeof(Nullable<>)))
            {
                //if it's null, just set the value from the reserved word null, and return
                if (propertyValue == null)
                {
                    oProp.SetValue(target, null, null);
                    return;
                }

                //Get the underlying type property instead of the nullable generic
                tProp = new NullableConverter(oProp.PropertyType).UnderlyingType;
            }

            //use the converter to get the correct value
            oProp.SetValue(target, Convert.ChangeType(propertyValue, tProp), null);
        }
        public static string CleanUrl(string UrlToClean)
        {
            if (UrlToClean == null) return "";
            UrlToClean = UrlToClean.Trim();
            UrlToClean = UrlToClean.Replace(" ", "-");
            UrlToClean = UrlToClean.Replace(".", "-");
            UrlToClean = UrlToClean.Replace(",", "-");
            UrlToClean = UrlToClean.Replace("\"", "-");
            UrlToClean = UrlToClean.Replace("'", "-");
            UrlToClean = UrlToClean.Replace(",", "-");
            UrlToClean = UrlToClean.Replace("/", "-");
            UrlToClean = UrlToClean.Replace("\\", "-");
            UrlToClean = UrlToClean.Replace("!", "-");
            UrlToClean = UrlToClean.Replace("@", "-");
            UrlToClean = UrlToClean.Replace("#", "-");
            UrlToClean = UrlToClean.Replace("%", "-");
            UrlToClean = UrlToClean.Replace("^", "-");
            UrlToClean = UrlToClean.Replace("&", "-");
            UrlToClean = UrlToClean.Replace("*", "-");
            UrlToClean = UrlToClean.Replace("(", "-");
            UrlToClean = UrlToClean.Replace(")", "-");
            UrlToClean = UrlToClean.Replace("-", "-");
            UrlToClean = UrlToClean.Replace("+", "-");
            UrlToClean = UrlToClean.Replace("=", "-");
            UrlToClean = UrlToClean.Replace("?", "-");
            UrlToClean = UrlToClean.Replace("<", "-");
            UrlToClean = UrlToClean.Replace(">", "-");
            UrlToClean = UrlToClean.Replace("₪", "-");
            UrlToClean = UrlToClean.Replace("|", "-");

            UrlToClean = UrlToClean.Replace("-----", "-");
            UrlToClean = UrlToClean.Replace("----", "-");
            UrlToClean = UrlToClean.Replace("---", "-");
            UrlToClean = UrlToClean.Replace("--", "-");

            return UrlToClean;
        }

        public static int RandomNumber(int min, int max)
        {
            Random random = new Random();
            return random.Next(min, max);
        }
        public static List<string> GetTokensByTemplateName(string systemName)
        {
            return MessageService.GetAvaliableTokensForTemplate(systemName);
          
        }
        public static string getValidEmailsString(string emails)
        {
            string res = "";
            string dlm = "";
            if (!string.IsNullOrEmpty(emails))
            {
                string[] arr = emails.Split(new char[] { ' ', ',' }, StringSplitOptions.RemoveEmptyEntries);
                foreach (var inputEmail in arr)
                {
                    if (isEmail(inputEmail))
                    {
                        res += dlm+inputEmail;
                        dlm = ", ";
                    }
                        
                   
                }
            }
            return res;
        }
        public static bool isEmail(string inputEmail)
        {
            inputEmail = inputEmail ?? string.Empty;
            string strRegex = @"^([a-zA-Z0-9_\-\.]+)@((\[[0-9]{1,3}" +
                  @"\.[0-9]{1,3}\.[0-9]{1,3}\.)|(([a-zA-Z0-9\-]+\" +
                  @".)+))([a-zA-Z]{2,4}|[0-9]{1,3})(\]?)$";
            Regex re = new Regex(strRegex);
            if (re.IsMatch(inputEmail))
                return (true);
            else
                return (false);
        }
    }
}
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;
using System.Web;
using System.Web.Mvc;

namespace Uco.Infrastructure
{
    public static class CssBundleHelper
    {
        private static Dictionary<string, MvcHtmlString> _loaded = new Dictionary<string, MvcHtmlString>();
        private static object _lock = new object();
        private static string _filePath = "/Cache/css/";
        private static string _filePrefix = "auto.gen.";
        public static HtmlString CssBundle(this HtmlHelper htmlHelper, params string[] Files)
        {



            
            StringBuilder hash = new StringBuilder();
            foreach (var src in Files)
            {
                hash.Append(src);
            }
            var key = Math.Abs(hash.ToString().GetHashCode()).ToString();
            if (!_loaded.ContainsKey(key))
            {
                string[] replacment = _filePath.Split(new char[] { '/' }, StringSplitOptions.RemoveEmptyEntries);

                string newContentPrefix = "";
                string NotMerged = "";
                for (int u = 0; u < replacment.Length; u++)
                {
                    newContentPrefix += "../";
                }
                StringBuilder sb = new StringBuilder();
                foreach (var src in Files)
                {
                    string scriptPath = Path.Combine(System.Web.HttpContext.Current.Server.MapPath(src));
                    NotMerged += "<link href=\"" + src.Replace("~", "") + "\" rel=\"stylesheet\" type=\"text/css\">";

                    if (File.Exists(scriptPath))
                    {
                        var content = File.ReadAllText(scriptPath);
                        var pathPart = src.Split('/');
                        string newUrlPath = "";
                        int i = 1;
                        foreach (var c in pathPart)
                        {
                            if (!string.IsNullOrEmpty(c) && c != "~" && i != pathPart.Length)
                            {
                                newUrlPath += c + "/";
                            }
                            i++;
                        }
                        // IMportant, need replace all url, image must be found!!!!!!!!!!!!
                        var index = content.IndexOf("url(\"");
                        while (index > 0)
                        {
                            if (content[index + 5] != '/')
                            {
                                content = content.Insert(index + 5, newContentPrefix + newUrlPath);

                                index += 5;
                            }

                            index = content.IndexOf("url(\"", index + 1);
                        }
                        index = content.IndexOf("url('");
                        while (index > 0)
                        {
                            if (content[index + 5] != '/')
                            {
                                content = content.Insert(index + 5, newContentPrefix + newUrlPath);

                                index += 5;
                            }

                            index = content.IndexOf("url('", index + 1);
                        }

                        index = content.IndexOf("url(");
                        while (index > 0)
                        {
                            if (content[index + 4] != '/' && content[index + 4] != '\'' && content[index + 4] != '"')
                            {
                                content = content.Insert(index + 4, newContentPrefix + newUrlPath);

                                index += 4;
                            }

                            index = content.IndexOf("url(", index + 1);
                        }

                        // content = content.Replace("url('", "url('../" + newUrlPath);
                        // content = content.Replace("url(\"", "url(\"../" + newUrlPath);
                        // content = content.Replace("//", "/");
                        sb.AppendLine(content);

                    }

                }
                string resPath = "~" + _filePath + _filePrefix + key + ".css";
                string scriptResPath = Path.Combine(System.Web.HttpContext.Current.Server.MapPath(resPath));
                lock (_lock)
                {
                    if (!_loaded.ContainsKey(key))
                    {
                        File.WriteAllText(scriptResPath, sb.ToString());
                        var _footerMerged = "<link href=\"" + resPath.Replace("~", "") + "\" rel=\"stylesheet\" type=\"text/css\">";
                        // for future use RouteTable, create virtual file, if can be faster
                        _loaded.Add(key, MvcHtmlString.Create(_footerMerged));
                        _loaded[key + "_notmerged"] = MvcHtmlString.Create(NotMerged);
                    }
                }
            }
            //return _loaded[key + "_notmerged"]; // if not merged, debug model
             return _loaded[key]; // production mode





            // old code
            //string OutString = string.Empty;
            //foreach (string item in Files)
            //{
            //    OutString = string.Format("{0}\r\n<link href=\"{1}\" rel=\"stylesheet\" type=\"text/css\" />", OutString, item.Replace("~/", "/"));
            //}
            //return new HtmlString(OutString);
        }
    }
}

using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;

namespace Uco.Infrastructure.HtmlHelpers
{
    
    public class StyleHelper
    {
        private Dictionary<string, List<string>> _zones { get; set; }
        private static Dictionary<string, MvcHtmlString> _loaded = new Dictionary<string, MvcHtmlString>();
        private static object _lock = new object();
        private static string _filePath = "/Cache/css/";
        private static string _filePrefix = "auto.gen.";
        private static bool? _combine;
        public static bool IsCombine {
            get {
                if(!_combine.HasValue)
                {
                    _combine = ConfigurationManager.AppSettings["CombineCss"] == "true";
                }
                return _combine.Value;
            }
        }
        public StyleHelper()
        {
            _zones = new Dictionary<string, List<string>>();
            _zones.Add("footer",new List<string>());
            _zones.Add("head", new List<string>());
        }
        public static void ClearCache()
        {
            _loaded.Clear();
        }
        public void Init()
        {
            _zones = new Dictionary<string, List<string>>();
            _zones.Add("footer", new List<string>());
            _zones.Add("head", new List<string>());
        }
        public void AddHead(string path)
        {
            if (!string.IsNullOrEmpty(path) && !_zones["head"].Contains(path))
            {
                _zones["head"].Add(path);
            }
        }
        private MvcHtmlString _RenderZone(string zone)
        {
            if (!_zones.ContainsKey(zone) || _zones[zone].Count == 0)
            {
                return MvcHtmlString.Create("");
            }
            StringBuilder hash = new StringBuilder();
            foreach (var src in _zones[zone])
            {
                hash.Append(src);
            }
            var key = Math.Abs( hash.ToString().GetHashCode()).ToString();
            if (!_loaded.ContainsKey(key))
            {
                string[] replacment = _filePath.Split(new char[]{'/'} ,StringSplitOptions.RemoveEmptyEntries);

                string newContentPrefix = "";
                string NotMerged = "";
                for (int u = 0; u < replacment.Length;u++ )
                {
                    newContentPrefix += "../";
                }
                StringBuilder sb = new StringBuilder();
                foreach (var src in _zones[zone])
                {
                    var pathServer = src;
                    int queryParamPositiion = pathServer.IndexOf("?");
                    if (queryParamPositiion > 0)
                        pathServer = pathServer.Substring(0, queryParamPositiion);
                    string scriptPath = Path.Combine(System.Web.HttpContext.Current.Server.MapPath(pathServer));
                    NotMerged += "<link href=\"" + src.Replace("~", "") + "\" rel=\"stylesheet\" type=\"text/css\">";
                   
                    if (File.Exists(scriptPath))
                    {
                        var content = File.ReadAllText(scriptPath);
                        var pathPart = pathServer.Split('/');
                        string newUrlPath = "";
                        int i = 1;
                        foreach(var c in pathPart)
                        {
                            if (!string.IsNullOrEmpty(c) && c != "~" && i != pathPart.Length) {
                                newUrlPath +=  c +"/";
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

                            index = content.IndexOf("url(\"",index+1);
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
                lock(_lock)
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
            if (IsCombine)
            {
                return _loaded[key];
               
            }
            return _loaded[key + "_notmerged"];
        }
        public MvcHtmlString RenderHead()
        {
            return _RenderZone("head");
          
        }
        public void AddFooter(string path)
        {
            if (!string.IsNullOrEmpty(path) && !_zones["footer"].Contains(path))
            {
                _zones["footer"].Add(path);
            }
        }
        public MvcHtmlString RenderFooter()
        {
            return _RenderZone("footer");
           
        }

        
      
    }
}
namespace Uco.Infrastructure.ViewEngine
{
    using System;
    using System.Collections.Generic;
    using System.Web.Mvc;

    public class CustomRazorViewEngine : CustomBuildManagerViewEngine
    {
        public CustomRazorViewEngine()
        {
            var cache = new DefaultViewLocationCache(TimeSpan.FromDays(365));
            base.ViewLocationCache = cache;

            List<string> AreaViewLocationFormatsList = new List<string>();
            if(SF.UsePlugins())
            {
                AreaViewLocationFormatsList.Add("~/Plugins/{4}/Areas/{2}/Views/{1}/{0}.cshtml");
                AreaViewLocationFormatsList.Add("~/Plugins/{4}/Areas/{2}/Views/Shared/{0}.cshtml");
            }
            AreaViewLocationFormatsList.Add("~/Areas/{2}/Views/{1}/{0}.cshtml");
            AreaViewLocationFormatsList.Add("~/Areas/{2}/Views/Shared/{0}.cshtml");
            AreaViewLocationFormats = AreaViewLocationFormatsList.ToArray();

            List<string> AreaMasterLocationFormatsList = new List<string>();
            if (SF.UsePlugins())
            {
                AreaMasterLocationFormatsList.Add("~/Plugins/{4}/Areas/{2}/Views/{1}/{0}.cshtml");
                AreaMasterLocationFormatsList.Add("~/Plugins/{4}/Areas/{2}/Views/Shared/{0}.cshtml");
            }
            AreaMasterLocationFormatsList.Add("~/Areas/{2}/Views/{1}/{0}.cshtml");
            AreaMasterLocationFormatsList.Add("~/Areas/{2}/Views/Shared/{0}.cshtml");
            AreaMasterLocationFormats = AreaMasterLocationFormatsList.ToArray();

            List<string> AreaPartialViewLocationFormatsList = new List<string>();
            if (SF.UsePlugins())
            {
                AreaPartialViewLocationFormatsList.Add("~/Plugins/{4}/Areas/{2}/Views/{1}/{0}.cshtml");
                AreaPartialViewLocationFormatsList.Add("~/Plugins/{4}/Areas/{2}/Views/Shared/{0}.cshtml");
            }
            AreaPartialViewLocationFormatsList.Add("~/Areas/{2}/Views/{1}/{0}.cshtml");
            AreaPartialViewLocationFormatsList.Add("~/Areas/{2}/Views/Shared/{0}.cshtml");
            AreaPartialViewLocationFormats = AreaMasterLocationFormatsList.ToArray();

            List<string> ViewLocationFormatsList = new List<string>();
            if (SF.UseThemes())
            {
                ViewLocationFormatsList.Add("~/Themes/{2}/Views/{1}/{0}.cshtml");
                ViewLocationFormatsList.Add("~/Themes/{2}/Views/Shared/{0}.cshtml");
            }
            if (SF.UsePlugins())
            {
                ViewLocationFormatsList.Add("~/Plugins/{3}/Views/{1}/{0}.cshtml");
                ViewLocationFormatsList.Add("~/Plugins/{3}/Views/Shared/{0}.cshtml");
            }
            ViewLocationFormatsList.Add("~/Views/{1}/{0}.cshtml");
            ViewLocationFormatsList.Add("~/Views/Shared/{0}.cshtml");
            ViewLocationFormats = ViewLocationFormatsList.ToArray();

            List<string> MasterLocationFormatsList = new List<string>();
            if (SF.UseThemes())
            {
                MasterLocationFormatsList.Add("~/Themes/{2}/Views/{1}/{0}.cshtml");
                MasterLocationFormatsList.Add("~/Themes/{2}/Views/Shared/{0}.cshtml");
            }
            if (SF.UsePlugins())
            {
                MasterLocationFormatsList.Add("~/Plugins/{3}/Views/{1}/{0}.cshtml");
                MasterLocationFormatsList.Add("~/Plugins/{3}/Views/Shared/{0}.cshtml");
            }
            MasterLocationFormatsList.Add("~/Views/{1}/{0}.cshtml");
            MasterLocationFormatsList.Add("~/Views/Shared/{0}.cshtml");
            MasterLocationFormats = MasterLocationFormatsList.ToArray();

            List<string> PartialViewLocationFormatsList = new List<string>();
            if (SF.UseThemes())
            {
                PartialViewLocationFormatsList.Add("~/Themes/{2}/Views/{1}/{0}.cshtml");
                PartialViewLocationFormatsList.Add("~/Themes/{2}/Views/Shared/{0}.cshtml");
            }
            if (SF.UsePlugins())
            {
                PartialViewLocationFormatsList.Add("~/Plugins/{3}/Views/{1}/{0}.cshtml");
                PartialViewLocationFormatsList.Add("~/Plugins/{3}/Views/Shared/{0}.cshtml");
            }
            PartialViewLocationFormatsList.Add("~/Views/{1}/{0}.cshtml");
            PartialViewLocationFormatsList.Add("~/Views/Shared/{0}.cshtml");
            PartialViewLocationFormats = PartialViewLocationFormatsList.ToArray();

            ViewStartFileExtensions = new[] { "cshtml" };
        }

        public string[] ViewStartFileExtensions { get; set; }

        protected override IView CreatePartialView(ControllerContext controllerContext, string partialPath)
        {
            return new RazorView(controllerContext, partialPath, null, false, ViewStartFileExtensions);
        }

        protected override IView CreateView(ControllerContext controllerContext, string viewPath, string masterPath)
        {
            return new RazorView(controllerContext, viewPath, masterPath, true, ViewStartFileExtensions);
        }

        protected override bool IsValidCompiledType(ControllerContext controllerContext, string virtualPath, Type compiledType)
        {
            return typeof(WebViewPage).IsAssignableFrom(compiledType);
        }
        public bool DoesViewExist(ControllerContext controllerContext, string name)
        {
            string viewName = name;
            
            ViewEngineResult viewResult = ViewEngines.Engines.FindPartialView(controllerContext, viewName);
            return (viewResult.View != null);
        }
    }
}
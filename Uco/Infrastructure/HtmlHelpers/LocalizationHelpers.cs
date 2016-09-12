using System;
using System.Globalization;
using System.Web;
using System.Web.Mvc;

namespace Uco.Infrastructure
{
    public static class LocalizationHelpers
    {
        #region in controler

        public static string GetGlobalResource(string FileName, string Culture, string ResourceKey)
        {
            System.Globalization.CultureInfo ci = new System.Globalization.CultureInfo(Culture);
            if (HttpContext.GetGlobalResourceObject(FileName, ResourceKey, ci) == null) return "";
            return (string)HttpContext.GetLocalResourceObject(FileName, ResourceKey, ci);
        }

        public static string GetGlobalResource(string FileName, string ResourceKey)
        {
            if (HttpContext.GetGlobalResourceObject(FileName, ResourceKey) == null) return "";
            return (string)HttpContext.GetLocalResourceObject(FileName, ResourceKey);
        }

        public static string GetLocalResource(string FilePath, string Culture, string ResourceKey)
        {
            
            System.Globalization.CultureInfo ci = new System.Globalization.CultureInfo(Culture);
            if (HttpContext.GetLocalResourceObject(FilePath, ResourceKey, ci) == null) return "";
            return (string)HttpContext.GetLocalResourceObject(FilePath, ResourceKey, ci);
        }

        public static string GetLocalResource(string FilePath, string ResourceKey)
        {
            if (HttpContext.GetLocalResourceObject(FilePath, ResourceKey) == null) return "";
            return (string)HttpContext.GetLocalResourceObject(FilePath, ResourceKey);
        }

        #endregion

        #region in view

        public static object RG(this HtmlHelper htmlHelper, string classKey, string resourceKey)
        {
            return htmlHelper.ViewContext.HttpContext.GetGlobalResourceObject(classKey, resourceKey);
        }

        public static string R(this HtmlHelper htmlHelper, string resourceKey)
        {
            string vp = GetVirtualPath(htmlHelper);
            try
            {
                var t = htmlHelper.ViewContext.HttpContext.GetLocalResourceObject(vp, resourceKey);
                return t != null ? t.ToString() : "";
            }
            catch
            {
                throw new NotImplementedException("No resource for: " + vp + " - " + resourceKey);
            }
        }
        public static bool IsPartialExist(this HtmlHelper htmlHelper, string view)
        {
            var controllerContext = htmlHelper.ViewContext.Controller.ControllerContext;
            var result = ViewEngines.Engines.FindPartialView(controllerContext, view);
           
            return result.View != null;
        }
        //public static bool IsPartialExist(this HtmlHelper htmlHelper, string view,string controller)
        //{
        //    var controllerContext = htmlHelper.ViewContext.Controller.ControllerContext;
        //    var viewfolder = controllerContext.RouteData.Values["controller"] ;
            
        //    var result = ViewEngines.Engines.FindPartialView(controllerContext, view);

        //    return result.View != null;
        //}
        public static string R(this HtmlHelper htmlHelper, string classKey, string resourceKey)
        {
            string vp = GetVirtualPath(htmlHelper);
            try
            {
                var t = htmlHelper.ViewContext.HttpContext.GetLocalResourceObject(classKey, resourceKey);
                return t != null ? t.ToString() : "";
            }
            catch
            {
                throw new NotImplementedException("No resource for: " + vp + " - " + resourceKey);
            }
        }

        public static string R(this HtmlHelper htmlHelper, string classKey, string resourceKey, CultureInfo culture)
        {
            string vp = GetVirtualPath(htmlHelper);
            try
            {
                var t = htmlHelper.ViewContext.HttpContext.GetLocalResourceObject(classKey, resourceKey, culture);
                return t != null ? t.ToString() : ""; 
            }
            catch
            {
                throw new NotImplementedException("No resource for: " + vp + " - " + resourceKey);
            }
        }

        private static string GetVirtualPath(HtmlHelper htmlhelper)
        {
            RazorView view = htmlhelper.ViewContext.View as RazorView;

            if (view != null)
                return view.ViewPath;

            return null;
        }
        
        #endregion
    }
}
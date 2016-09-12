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
        private static Db _db
        {
            get { return LS.CurrentEntityContext; }
        }

        public static AbstractPage GetPageByUrl(string Url)
        {
            string[] UrlSplited = Url.Split('/');
            int UrlSplitedLength = UrlSplited.Length;
            if (UrlSplitedLength < 2) return null;
            string SeoUrlName = UrlSplited[UrlSplitedLength - 1].ToLower();
            string RouteUrl = UrlSplited[UrlSplitedLength - 2].ToLower();

            return LS.CurrentEntityContext.AbstractPages.FirstOrDefault(r => r.SeoUrlName == SeoUrlName && r.RouteUrl == RouteUrl);
        }

        public static string GetTypeRouteUrl(Type t)
        {
            object[] attributes = t.GetCustomAttributes(typeof(RouteUrlAttribute), false);
            foreach (object item in attributes)
            {
                RouteUrlAttribute at = (RouteUrlAttribute)item;
                return at.RouteUrl;
            }
            return string.Empty;
        }

        public static List<string> GetTypeRestrictParentsAttribute(Type t)
        {
            object[] attributes = t.GetCustomAttributes(typeof(RestrictParentsAttribute), false);
            foreach (object item in attributes)
            {
                RestrictParentsAttribute at = (RestrictParentsAttribute)item;
                return at.Parents.ToList();
            }
            return new List<string>();
        }

        public static string GetPageIcon(string RouteUrl)
        {
            Dictionary<string, Type> dictionary = RP.GetPageTypesDictionaryReprository();

            if (!dictionary.ContainsKey(RouteUrl)) return string.Empty;
            Type t = dictionary[RouteUrl];

            System.Attribute[] attrs = System.Attribute.GetCustomAttributes(t);
            foreach (System.Attribute attr in attrs)
            {
                if (attr is PageIconAttribute)
                {
                    PageIconAttribute a = (PageIconAttribute)attr;
                    return a.PageIcon;
                }
            }

            return string.Empty;
        }

        public static string GetPageName(string RouteUrl)
        {
            Dictionary<string, Type> dictionary = RP.GetPageTypesDictionaryReprository();

            if (!dictionary.ContainsKey(RouteUrl)) return string.Empty;
            Type t = dictionary[RouteUrl];
            return GetPageName(t);
        }

        public static string GetPageName(Type t)
        {
            System.Attribute[] attrs = System.Attribute.GetCustomAttributes(t);
            foreach (System.Attribute attr in attrs)
            {
                if (attr is PageNameAttribute)
                {
                    PageNameAttribute a = (PageNameAttribute)attr;
                    if (a.ResourceType == null) return a.PageName;
                    return GetLocalizedString("PageName", a.PageName, a.ResourceType);

                }
            }
            return string.Empty;
        }
    }
}
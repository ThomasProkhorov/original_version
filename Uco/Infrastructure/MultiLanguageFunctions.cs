using System.Collections.Generic;
using System.Configuration;
using System.Web;
using Uco.Infrastructure.Repositories;
using Uco.Models;
using System.Linq;
using System;

namespace Uco.Infrastructure
{
    public static partial class SF
    {
        public static string GetLangCodeThreading()
        {
            return System.Threading.Thread.CurrentThread.CurrentCulture.Name;
        }
        public static List<string> GetMenuGroups()
        {
            var set = RP.GetCurrentSettings();
            if (!string.IsNullOrEmpty(set.MenuGroups))
            {
                return set.MenuGroups.Split(',').ToList();
               
            }
            return new List<string>();
        }
        public static string GetFirstMenuGroup()
        {
            var set = RP.GetCurrentSettings();
            if (!string.IsNullOrEmpty(set.MenuGroups))
            {
                var spl = set.MenuGroups.Split(',');
                if (spl.Length > 0) {
                    return spl[0];
                }
            }
            return null;
        }
        public static string GetLangCodeWebconfig()
        {
            return ((System.Web.Configuration.GlobalizationSection)ConfigurationManager.GetSection("system.web/globalization")).UICulture;
        }

        public static string GetLangCodeCurrentSettings()
        {
            return RP.GetCurrentSettings().LanguageCode;
        }

        public static string GetLangRoute(string LanguageCode)
        {
            if (string.IsNullOrEmpty(LanguageCode)) return string.Empty;
            if (GetLangCodeWebconfig().ToLower() == LanguageCode.ToLower()) return string.Empty;
            else return LanguageCode + "/";
        }

        public static void SetLanguageCode(AbstractPage ap, int MaxTimes, string LanguageCode)
        {
            List<int> Pages = new List<int>();
            Pages.Add(ap.ID);
            SF.SetLanguageCodeRecurcive(ap, 100, Pages);
            string FormatedPagesIDs = string.Join(",", Pages);
            _db.Database.ExecuteSqlCommand(string.Format("Update AbstractPages Set LanguageCode = '{0}' Where DomainID = '{1}' And ID IN ({2})", LanguageCode, ap.DomainID, FormatedPagesIDs));

        }

        private static List<int> SetLanguageCodeRecurcive(AbstractPage ap, int MaxTimes, List<int> Pages)
        {
            MaxTimes = MaxTimes - 1;
            if (MaxTimes <= 0) return Pages;

            foreach (AbstractPage item in _db.AbstractPages.Where(r => r.ParentID == ap.ID).ToList())
            {
                if (item.RouteUrl == "l") continue;

                Pages.Add(item.ID);

                SetLanguageCodeRecurcive(item, MaxTimes, Pages);
            }

            return Pages;
        }

        //public static string GetLangJS(string KendoUrl)
        //{
        //    return "<script src=\"" + VirtualPathUtility.ToAbsolute(KendoUrl + "/cultures/kendo.culture." + SF.GetLangCodeThreading() + ".min.js") + "\"></script><script> kendo.culture(\"" + SF.GetLangCodeThreading() + "\");</script>";
        //}

        public static bool AdminIsRTL()
        {
            string LangCode = SF.GetLangCodeThreading();
            List<string> rltList = new List<string>() { "he-IL" };
            if (rltList.Contains(LangCode)) { return true; }
            else { return false; }
        }

        private static string GetLocalizedString(string propertyName, string key, Type ResourceType)
        {
            // If we don't have a resource or a key, go ahead and fall back on the key
            if (ResourceType == null || key == null)
                return key;

            var property = ResourceType.GetProperty(key);

            // Strings are only valid if they are public static strings
            var isValid = false;
            if (ResourceType.IsVisible && property != null && property.PropertyType == typeof(string))
            {
                var getter = property.GetGetMethod();

                // Gotta have a public static getter on the property
                if (getter != null && getter.IsStatic && getter.IsPublic)
                {
                    isValid = true;
                }
            }

            // If it's not valid, go ahead and throw an InvalidOperationException
            if (!isValid)
            {
                var message = string.Format(localization_failed_message, propertyName, ResourceType.ToString(), key);
                throw new InvalidOperationException(message);
            }

            return (string)property.GetValue(null, null);

        }

        const string localization_failed_message = "Cannot retrieve property '{0}' because localization failed. Type '{1} is not public or does not contain a public static string property with the name '{2}'.";
    }
}
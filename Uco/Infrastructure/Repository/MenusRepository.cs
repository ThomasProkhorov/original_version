using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Uco.Models;
using System.Data.Entity.Validation;
using System.Diagnostics;
using System.Net.Mail;
using System.Configuration;
using System.Text;
using System.Reflection;
using System.Globalization;
using System.Web.Configuration;
using Uco.Infrastructure.Livecycle;

namespace Uco.Infrastructure.Repositories
{
    public static partial class RP
    {
        #region Get/Clean Repository

        public static void CleanMenuRepository(int SettingsID)
        {
            LS.Cache.Remove("MenuItems_" + SettingsID);
        }

        public static List<CustomMenuItem> GetMenuRepository(Settings s)
        {
            string Token = "MenuItems_" + s.ID;
            if (LS.Cache[Token] == null)
            {
                List<CustomMenuItem> pages = GetMenuAll(s.ID);
                LS.Cache[Token] = pages;
                return pages;
            }
            else
                return (List<CustomMenuItem>)LS.Cache[Token];
        }

        public static List<CustomMenuItem> GetDisplayMenuRepository()
        {
            Settings s = GetCurrentSettings();
            return GetMenuRepository(s);
        }

        public static List<CustomMenuItem> GetAdminMenuRepository()
        {
            Settings s = GetAdminCurrentSettingsRepository();
            return GetMenuRepository(s);
        }

        #endregion

        #region Menu

        public static string GetMenuFormated(int MaxLevel)
        {
            return GetMenuFormated(MaxLevel, 0, "", "");

        }

        public static string GetMenuFormated(int MaxLevel, int CurrentPageID)
        {
            return GetMenuFormated(MaxLevel, CurrentPageID, "", "");

        }

        public static string GetMenuFormated(int MaxLevel, int CurrentPageID, string UlID, string UlClass)
        {
            List<CustomMenuItem> pages = GetDisplayMenuRepository();
            if (SF.GetLangCodeThreading() == SF.GetLangCodeWebconfig())
            {
                return WriteChildMenu(pages.Where(r => r.Visible == true && r.LangCode == SF.GetLangCodeThreading() && r.ShowInMenu == true && r.Level <= MaxLevel).ToList(), null, MaxLevel, 1, CurrentPageID, UlID, UlClass);
            }
            else
            {
                MaxLevel = MaxLevel + 1;
                return WriteChildMenu(pages.Where(r => r.Visible == true && r.LangCode == SF.GetLangCodeThreading() && r.ShowInMenu == true && r.Level <= MaxLevel).ToList(), null, MaxLevel, 2, CurrentPageID, UlID, UlClass);
            }
        }

        #endregion

        #region SubMenu

        public static string GetSubMenuFormated(int startLevel, int MaxLevel, int MainPageID)
        {
            List<CustomMenuItem> pages = GetDisplayMenuRepository();
            CustomMenuItem StartItem = pages.FirstOrDefault(r => r.ID == MainPageID);

            List<int> backWay = new List<int>();
            int backWayLevel = 0;
            do
            {
                backWay.Add(StartItem.ID);
                backWayLevel = backWayLevel + 1;
                StartItem = pages.FirstOrDefault(r => r.ID == StartItem.ParentID);
            }
            while (StartItem != null || backWayLevel > 10);

            try { MainPageID = backWay[backWayLevel - startLevel]; }
            catch { return ""; }
            StartItem = pages.FirstOrDefault(r => r.ID == MainPageID);

            return WriteChildMenu(pages.Where(r => r.Visible == true && r.ShowInMenu == true && r.Level <= MaxLevel).ToList(), StartItem, MaxLevel, startLevel);

        }

        public static string GetSubMenuFormated(int startLevel, int MaxLevel, int MainPageID, int CurentPageID)
        {
            List<CustomMenuItem> pages = GetDisplayMenuRepository();
            CustomMenuItem StartItem = pages.FirstOrDefault(r => r.ID == MainPageID);

            List<int> backWay = new List<int>();
            int backWayLevel = 0;
            do
            {
                backWay.Add(StartItem.ID);
                backWayLevel = backWayLevel + 1;
                StartItem = pages.FirstOrDefault(r => r.ID == StartItem.ParentID);
            }
            while (StartItem != null || backWayLevel > 10);

            try { MainPageID = backWay[backWayLevel - startLevel]; }
            catch { return ""; }
            StartItem = pages.FirstOrDefault(r => r.ID == MainPageID);

            return WriteChildMenu(pages.Where(r => r.Visible == true && r.ShowInMenu == true && r.Level <= MaxLevel).ToList(), StartItem, MaxLevel, startLevel, CurentPageID);
        }

        public static string GetSubMenuFormated(int startLevel, int MaxLevel, int CurentPageID, out string H)
        {
            List<CustomMenuItem> pages = GetDisplayMenuRepository();
            CustomMenuItem StartItem = pages.FirstOrDefault(r => r.ID == CurentPageID);

            List<int> backWay = new List<int>();
            int backWayLevel = 0;
            do
            {
                backWay.Add(StartItem.ID);
                backWayLevel = backWayLevel + 1;
                StartItem = pages.FirstOrDefault(r => r.ID == StartItem.ParentID);
                if (StartItem != null && StartItem.RouteUrl == "d")
                {
                    backWay.Add(StartItem.ID);
                    backWayLevel = backWayLevel + 1;
                    break;
                }
            }
            while (StartItem != null || backWayLevel > 10);

            try { CurentPageID = backWay[backWayLevel - startLevel]; }
            catch { H = ""; return ""; }
            StartItem = pages.FirstOrDefault(r => r.ID == CurentPageID);

            H = StartItem.Title;
            return WriteChildMenu(pages.Where(r => r.Visible == true && r.Level <= MaxLevel).ToList(), StartItem, MaxLevel, startLevel);
        }

        public static string GetBreadcrumbs(int CurentPageID)
        {
            string outValue = "";
            CustomMenuItem item1 = RP.GetDisplayMenuRepository().FirstOrDefault(r => r.ID == CurentPageID);
            if (item1 == null) return outValue;
            outValue = "<ul>";
            outValue = outValue + "<li><a href='" + item1.Url.Replace("~", "") + "'>" + item1.Title + "</a><span>/ </span></li>";
            CustomMenuItem item2 = RP.GetDisplayMenuRepository().FirstOrDefault(r => r.ID == item1.ParentID);
            if (item2 == null) return outValue + "</ul>";
            outValue = outValue + "<li><a href='" + item2.Url.Replace("~", "") + "'>" + item2.Title + "</a><span>/ </span></li>";
            CustomMenuItem item3 = RP.GetDisplayMenuRepository().FirstOrDefault(r => r.ID == item2.ParentID);
            if (item3 == null) return outValue + "</ul>";
            outValue = outValue + "<li><a href='" + item3.Url.Replace("~", "") + "'>" + item3.Title + "</a><span>/ </span></li>";
            CustomMenuItem item4 = RP.GetDisplayMenuRepository().FirstOrDefault(r => r.ID == item3.ParentID);
            if (item4 == null) return outValue + "</ul>";
            outValue = outValue + "<li><a href='" + item4.Url.Replace("~", "") + "'>" + item4.Title + "</a><span>/ </span></li>";

            return outValue;
        }


        #endregion

        #region Sitemap

        public static List<CustomMenuItem> GetSiteMap()
        {
            Settings s = GetCurrentSettings();
            List<CustomMenuItem> pages = GetMenuAllIncludeNoneAdminMenu(s.ID, SF.GetLangCodeThreading());
            return pages;
        }

        public static string GetSiteMapFormated()
        {
            Settings s = GetCurrentSettings();
            List<CustomMenuItem> pages = GetMenuAllIncludeNoneAdminMenu(s.ID, SF.GetLangCodeThreading());
            return WriteChildMenu(pages.Where(r => r.Visible == true && r.ShowInSitemap == true).ToList(), null, 10, 1);
        }

        public static string GetSiteMapFormatedXML(int skip = 0, int take = 20000)
        {
            Settings s = GetCurrentSettings();
            List<CustomMenuItem> pages = new List<CustomMenuItem>();
            if (skip == 0)
            {
               pages = GetMenuAllIncludeNoneAdminMenu(s.ID, SF.GetLangCodeThreading());
            }
          pages =  pages.Where(r => r.Visible == true && r.ShowInSitemap == true).ToList();
            var categoriesFormat = "/{0}#!/category/{1}/{2}";
            var productsFormat = "/{0}#!/{1}/{2}";
            var shopFormat = "/{0}";
            DateTime now = DateTime.Now;
            var today =  new DateTime(now.Year, now.Month, now.Day);
            var allShops = _db.Shops.ToList();
            var allShopCategories = _db.ShopCategories.ToList();
            var allCategories = _db.Categories.ToList();
            if (skip == 0)
            {
                foreach (Shop shop in allShops)
                {
                    var menuItem = new CustomMenuItem
                    {
                        RedirectTo = string.Format(shopFormat, shop.SeoUrl),
                        ChangeTime = today,
                        Visible = true,
                        ShowInSitemap = true
                    };
                    pages.Add(menuItem);
                }
                foreach (Shop shop in allShops)
                {
                    var shopCategories = allShopCategories.Where(_ => _.ShopID == shop.ID).ToList();

                    foreach (ShopCategory shopCategory in shopCategories)
                    {
                        var categories = allCategories.Where(_ => _.ID == shopCategory.CategoryID).ToList();
                        foreach (Category category in categories)
                        {
                            var menuItem = new CustomMenuItem
                            {
                                RedirectTo = string.Format(categoriesFormat, shop.SeoUrl, shop.ID, category.ID),
                                ChangeTime = today,
                                Visible = true,
                                ShowInSitemap = true
                            };
                            pages.Add(menuItem);
                        }
                    }


                }
            }
            var usedCount = pages.Count;
            var canTakeCount = take - usedCount;
            if(canTakeCount < 0)
            {
                canTakeCount = 0;
            }
            var allProductShops = (from ps in _db.ProductShopMap
                                   join p in _db.Products
                                   on ps.ProductID equals p.ID
                                   select new { ps.ID, p.SKU, ProductID = p.ID, ps.ShopID }
                                      ).Distinct().OrderBy(x=>x.ID).Skip(skip).Take(canTakeCount).ToList();
            foreach (Shop shop in allShops)
            {
               var productShops = allProductShops.Where(_ => _.ShopID == shop.ID);
                foreach (var productShop in productShops)
                {
                    var menuItem = new CustomMenuItem
                    {
                        RedirectTo = string.Format(productsFormat, shop.SeoUrl, productShop.SKU ,productShop.ID),
                        ChangeTime = today,
                        Visible = true,
                        ShowInSitemap = true
                    };
                    pages.Add(menuItem);
                }
            }


            return WriteChildMenuXML(pages);
        }

        #endregion

        #region Other

        public static string GetSEONameByUrl(string Url)
        {
            if (Url == null) return string.Empty;
            string[] UrlSplit = Url.Split('/');
            if (UrlSplit.Length == 3) return UrlSplit[2];
            else return string.Empty;
        }

        public static string GetMenuUrl(int ID)
        {
            if (ID == 1) return "/";
            CustomMenuItem m = GetDisplayMenuRepository().FirstOrDefault(r => r.ID == ID);
            if (m != null) return m.Url;
            else return string.Empty;
        }

        public static int GetPageIDByUrl(string Url)
        {
            CustomMenuItem cmi = GetDisplayMenuRepository().FirstOrDefault(r => r.Url == "~" + Url);
            if (cmi != null) return cmi.ID;
            else return 0;
        }

        #endregion

        #region Private

        private static string WriteChildMenu(List<CustomMenuItem> pages1, CustomMenuItem Current, int MaxLevel, int CurrentLevel)
        {
            return WriteChildMenu(pages1, Current, MaxLevel, CurrentLevel, 0, "", "");
        }

        private static string WriteChildMenu(List<CustomMenuItem> pages1, CustomMenuItem Current, int MaxLevel, int CurrentLevel, int CurentPageID)
        {
            return WriteChildMenu( pages1, Current, MaxLevel, CurrentLevel, CurentPageID, "" ,"");
        }

        private static string WriteChildMenu(List<CustomMenuItem> pages1, CustomMenuItem Current, int MaxLevel, int CurrentLevel, int CurentPageID, string UlID, string UlClass)
        {
            string outStr = "";
            if (pages1.Count(r => r.Level == CurrentLevel && (Current == null ? true : (r.ParentID == Current.ID))) != 0)
            {
                outStr = outStr + (CurrentLevel == 1 ? "<ul id='" + UlID + "' class='" + UlClass + "'>" : "<ul>"); 
                foreach (CustomMenuItem item in pages1.Where(r => r.Level == CurrentLevel && (Current == null ? true : (r.ParentID == Current.ID))))
                {
                    outStr = outStr + "<li" + (CurentPageID == item.ID ? " class=\"current\"" : "") + (IsParent(item.ID, CurentPageID) ? " class=\"parent\"" : "") + "><a href=\"" + item.Url + "\">" + item.Title + "</a>";
                    if (MaxLevel != CurrentLevel)
                    {
                        outStr = outStr + WriteChildMenu(pages1, item, MaxLevel, CurrentLevel + 1);
                    }
                    outStr = outStr + "</li>";
                }
                outStr = outStr + "</ul>";
            }
            return outStr;
        }

        private static string WriteChildMenuXML(List<CustomMenuItem> pages)
        {
            var output = new StringBuilder("<?xml version=\"1.0\" encoding=\"UTF-8\"?><urlset xmlns=\"http://www.sitemaps.org/schemas/sitemap/0.9\">");
            foreach (CustomMenuItem item in pages)
            {
                output.Append(System.String.Format("<url><loc>http://{0}{1}</loc><lastmod>{2}</lastmod><changefreq>weekly</changefreq><priority>0</priority></url>", LS.CurrentHttpContext.Request.ServerVariables["HTTP_HOST"]
                    , System.Security.SecurityElement.Escape(item.Url.Replace(((char)2).ToString(),""))
                    , String.Format("{0:yyyy-MM-dd}", item.ChangeTime)));
            }
            output.Append("</urlset>");

            return output.ToString();
        }

        private static bool IsParent(int ParentID, int CurrentID)
        {
            int MaxLoop = 5;
            return IsParent(ParentID, CurrentID, MaxLoop);
        }

        private static bool IsParent(int ParentID, int CurrentID, int MaxLoop)
        {
            List<CustomMenuItem> mi = GetDisplayMenuRepository();
            CustomMenuItem CurrentItem = mi.FirstOrDefault(r => r.ID == CurrentID);
            CustomMenuItem ParentItem = mi.FirstOrDefault(r => r.ID == ParentID);

            if (CurrentItem == null || ParentItem == null || CurrentItem == ParentItem) return false;
            if (CurrentItem.ParentID == ParentItem.ID)
                return true;

            for (int i = 1; i <= MaxLoop; i++)
            {
                CurrentItem = mi.FirstOrDefault(r => r.ID == CurrentItem.ParentID);

                if (CurrentItem == null || ParentItem == null || CurrentItem == ParentItem) return false;
                if (CurrentItem.ParentID == ParentItem.ID)
                    return true;
            }
            return false;
        }

        private static List<CustomMenuItem> GetMenuAll(int SettingsID)
        {
            List<CustomMenuItem> pages = _db.AbstractPages.Where(r => r.DomainID == SettingsID && r.ShowInAdminMenu == true).OrderBy(r => r.Order).ThenBy(r => r.CreateTime).Select(item => new CustomMenuItem()
            {
                Title = item.Title,
                SeoUrlName = (item.RouteUrl == "r" ? item.Text2 : item.SeoUrlName),
                RouteUrl = item.RouteUrl,
                RedirectTo = item.RedirectTo,
                ID = item.ID,
                Visible = item.Visible,
                ShowInMenu = item.ShowInMenu,
                ShowInAdminMenu = item.ShowInAdminMenu,
                ShowInSitemap = item.ShowInSitemap,
                ParentID = item.ParentID,
                Level = ((item.RouteUrl == "d") ? 0 : -1),
                IsLangRoot = ((item.RouteUrl == "d") ? true : false),
                LangCode = item.LanguageCode,
                ChangeTime = item.ChangeTime
            }
            ).ToList();

            CustomMenuItem domainPage = pages.FirstOrDefault(r => r.Level == 0);
            if (domainPage != null)
            {
                domainPage.Level = 0;
                SetMenuLevel(domainPage, pages);
            }

            pages.RemoveAll(r => r.Level == -1);

            return pages;
        }

        private static List<CustomMenuItem> GetMenuAllIncludeNoneAdminMenu(int SettingsID, string LanguageCode)
        {
            List<CustomMenuItem> pages = _db.AbstractPages.Where(r => r.Visible && r.DomainID == SettingsID && r.ShowInAdminMenu && r.LanguageCode == LanguageCode).OrderBy(r => r.Order).ThenBy(r => r.CreateTime).Select(item => new CustomMenuItem()
            {
                Title = item.Title,
                SeoUrlName = item.SeoUrlName,
                RouteUrl = item.RouteUrl,
                RedirectTo = item.RedirectTo,
                ID = item.ID,
                Visible = item.Visible,
                ShowInMenu = item.ShowInMenu,
                ShowInAdminMenu = item.ShowInAdminMenu,
                ShowInSitemap = item.ShowInSitemap,
                ParentID = (item.ParentID == null ? 0 : (int)item.ParentID),
                Level = ((item.RouteUrl == "d" || item.RouteUrl == "l") ? 0 : -1),
                IsLangRoot = ((item.RouteUrl == "d" || item.RouteUrl == "l") ? true : false),
                LangCode = item.LanguageCode,
                ChangeTime = item.ChangeTime
            }
            ).ToList();

            CustomMenuItem domainPage = pages.FirstOrDefault(r => r.Level == 0);
            if (domainPage != null)
            {
                domainPage.Level = 0;
                SetMenuLevel(domainPage, pages);
            }

            pages.RemoveAll(r => r.Level == -1);

            return pages;
        }

        private static void SetMenuLevel(CustomMenuItem item1, List<CustomMenuItem> pages)
        {
            foreach (CustomMenuItem item2 in pages.Where(r => r.ParentID == item1.ID).OrderBy(r => r.ID))
            {
                item2.Level = item1.Level + 1;
                SetMenuLevel(item2, pages);
            }
        }

        #endregion
    }
}
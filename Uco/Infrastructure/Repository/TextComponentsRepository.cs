using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Uco.Infrastructure.Livecycle;
using Uco.Models;

namespace Uco.Infrastructure.Repositories
{
    public static partial class RP
    {
        public static List<Banner> GetBannersReprository(string BannerGroup)
        {
            string lang = SF.GetLangCodeThreading();
            string key = string.Format("BannersReprository_{0}_{1}_{2}", lang, BannerGroup, RP.GetCurrentSettings().ID.ToString());
            if (LS.Cache[key] == null)
            {
                using (Db _db = new Db())
                {
                    int did = RP.GetAdminCurrentSettingsRepository().ID;
                    List<Banner> l = _db.Banners.Where(r => r.BannerGroup == BannerGroup && r.DomainID == did
                        && r.LangCode == lang
                        && r.ShowDateMax > DateTime.Now).ToList();


                    //Output
                    foreach (Banner item in l)
                    {
                        if (item.BannerTypeName == Banner.BannerType.Text) item.Output = "<div class='banner'>" + item.Text + "</div>";
                        else if (item.BannerTypeName == Banner.BannerType.Html) item.Output = "<div class='banner'>" + item.Html + "</div>";
                        else if (item.BannerTypeName == Banner.BannerType.Image)
                        {
                            if (!string.IsNullOrEmpty(item.Link)) item.Output = "<div class='banner'>" + "<a target='_blank' href='" + item.Link + "'><img alt='" + item.Title + "' src='" + item.MainFile + "'></a>" + "</div>";
                            else if (string.IsNullOrEmpty(item.Link)) item.Output = "<div class='banner'>" + "<img alt='" + item.Title + "' src='" + item.MainFile + "'>" + "</div>";
                        }
                        else if (item.BannerTypeName == Banner.BannerType.Flash) item.Output =
                               "<div class='banner banner_holder' style='width:" + item.Width + "px;height:" + item.Height + "px;position:relative;'>"
                                    + "<div class='banner_bottom' style='width:" + item.Width + "px;height:" + item.Height + "px;left:0;position:absolute;top:0;z-index:100;'>"
                                        + "<object height=\"" + item.Height + "\" width=\"" + item.Width + "\" codebase=\"http://active.macromedia.com/flash5/cabs/swflash.cab#version=5,0,0,0' classid='clsid:D27CDB6E-AE6D-11cf-96B8-444553540000\">"
                                            + "<param name=\"movie\" value=\"" + item.MainFile + "\" />"
                                            + "<param name=\"play\" value=\"true\" />"
                                            + "<param name=\"loop\" value=\"true\" />"
                                            + "<param name=\"wmode\" value=\"transparent\" />"
                                            + "<param name=\"quality\" value=\"high\" />"
                                            + "<embed height=\"" + item.Height + "\" width=\"" + item.Width + "\" pluginspage=\"http://www.macromedia.com/shockwave/download/index.cgi?P1_Prod_Version=ShockwaveFlash\" quality=\"high\" wmode=\"transparent\" loop=\"true\" play=\"true\" src=\"" + item.MainFile + "\">"
                                        + "</object>"
                                    + "</div>"
                                    + (string.IsNullOrEmpty(item.Link) ? "" : ("<a target='_blank' href='" + item.Link + "' class='banner_top' style='width:" + item.Width + "px;height:" + item.Height + "px;display:block;left:0;position:absolute;top:0;z-index: 1000;'></a>"))
                                + "</div>";
                        else if (item.BannerTypeName == Banner.BannerType.FlashAndBackground) item.Output =
                                "<div class='banner banner_holder' style='width:" + item.Width + "px;height:" + item.Height + "px;position:relative;'>"
                                    + "<div syle='width:" + item.Width + "px;height:" + item.Height + "px;display:block;left:0;position:absolute;top:0;z-index: 0;'><img alt='" + item.Title + "' src='" + item.OtherFile + "' style='border-width: 0px; width:" + item.Width + "px; height:" + item.Height + "px;'></div>"
                                    + "<div class='banner_bottom' style='width:" + item.Width + "px;height:" + item.Height + "px;left:0;position:absolute;top:0;z-index:100;'>"
                                        + "<object height=\"" + item.Height + "\" width=\"" + item.Width + "\" codebase=\"http://active.macromedia.com/flash5/cabs/swflash.cab#version=5,0,0,0' classid='clsid:D27CDB6E-AE6D-11cf-96B8-444553540000\">"
                                            + "<param name=\"movie\" value=\"" + item.MainFile + "\" />"
                                            + "<param name=\"play\" value=\"true\" />"
                                            + "<param name=\"loop\" value=\"true\" />"
                                            + "<param name=\"wmode\" value=\"transparent\" />"
                                            + "<param name=\"quality\" value=\"high\" />"
                                            + "<embed height=\"" + item.Height + "\" width=\"" + item.Width + "\" pluginspage=\"http://www.macromedia.com/shockwave/download/index.cgi?P1_Prod_Version=ShockwaveFlash\" quality=\"high\" wmode=\"transparent\" loop=\"true\" play=\"true\" src=\"" + item.MainFile + "\">"
                                        + "</object>"
                                    + "</div>"
                                    + (string.IsNullOrEmpty(item.Link) ? "" : ("<a target='_blank' href='" + item.Link + "' class='banner_top' style='width:" + item.Width + "px;height:" + item.Height + "px;display:block;left:0;position:absolute;top:0;z-index: 1000;'></a>"))
                                + "</div>";
                    }
                    LS.Cache[key] = l;
                    return l;
                }

            }
            else return LS.Cache[key] as List<Banner>;
        }
        #region Get/Clean Repository

        public static void CleanTextComponentRepository()
        {
            foreach (Settings item in RP.GetSettingsRepositoryList())
            {
                foreach (string item2 in System.Configuration.ConfigurationManager.AppSettings["Languages"].Split(','))
                {
                    LS.Cache.Remove(string.Format("TextComponent_{0}_{1}", item.ID, item2));
                }
            }
        }

        private static List<TextComponent> GetTextComponentReprository()
        {
            return LS.Get<TextComponent>();
            string LanguageCode = RP.GetCurrentSettings().LanguageCode;
            int DomainID = RP.GetCurrentSettings().ID;
            string Token = "TextComponent_" + DomainID + "_" + LanguageCode;

            if (LS.Cache[Token] == null)
            {
                List<TextComponent> l = new List<TextComponent>();
                l = _db.TextComponents.Where(r => r.DomainID == DomainID && r.LangCode == LanguageCode).ToList();

                LS.Cache[Token] = l;
                return l;
            }
            else return LS.Cache[Token] as List<TextComponent>;
        }

        #endregion

        #region Get/Set single

        public static string GetTextComponent(string SystemName)
        {
            TextComponent tc = GetTextComponentReprository().FirstOrDefault(r => r.SystemName == SystemName);
            if (tc != null) return tc.Text;
            else return string.Empty;
        }
		
		public static IHtmlString Text(string SystemName)
        {
            TextComponent tc = GetTextComponentReprository().FirstOrDefault(r => r.SystemName == SystemName);
            if (tc == null) return new HtmlString(SystemName);
            else return new HtmlString(tc.Text);
        }

        #endregion
    }
}
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Uco.Infrastructure.Livecycle;
using Uco.Models;
using Uco.Models.Shopping.Measure;

namespace Uco.Infrastructure.Repositories
{
    public static partial class RP
    {

        #region properties

        private static Db _db
        {
            //!!!WRONG
            //get { return new Db(); }

            get
            {
                return LS.CurrentEntityContext;
            }
        }

        #endregion

        private static Object s_lock = new Object();

        public static List<Uco.Models.Settings> GetSettingsRepositoryList()
        {

            string Token = "SettingsAll";
            if (LS.Cache[Token] == null)
            {
                LS.Cache[Token] = _db.SettingsAll.ToList();
            }
            return (List<Uco.Models.Settings>)LS.Cache[Token];
        }

        public static Uco.Models.Settings GetCurrentSettings()
        {
            if (LS.CurrentEntityContext == null) return
                new Settings()
                {
                    AdminEmail = "admin@uco.co.il",
                    Domain = "localhost",
                    LanguageCode = "en-US",
                    Roles = "",

                };

            if (SF.UseMultiDomain() == false)
            {
                string Token = "CurrentSettings_" + SF.GetLangCodeThreading();
                if (LS.Cache[Token] == null)
                {
                    lock (s_lock)
                    {
                        Settings s = GetSettingsRepositoryList().FirstOrDefault();
                        if (s == null) return null;

                        string LangCodeThreading = SF.GetLangCodeThreading();
                        if (SF.GetLangCodeWebconfig() != LangCodeThreading)
                        {
                            LanguagePage l = _db.LanguagePages.FirstOrDefault(r => r.DomainID == s.ID && r.LanguageCode == LangCodeThreading);
                            if (l != null)
                            {
                                s.LanguageCode = l.LanguageCode;
                                //s.LanguageRTL = l.ShowOnMainPage;
                                s.HeaderHtml = l.Text2;
                                s.FotterHtml = l.Text3;
                            }
                        }

                        LS.Cache[Token] = s;
                    }
                }
                return (Uco.Models.Settings)LS.Cache[Token];
            }
            else
            {
                string Domain = SF.GetCurrentDomain();

                if (LS.CurrentHttpContext.Request.IsLocal)
                {
                    string TokenAdmin = "AdminCurrentSettingsRepository";
                    if (LS.CurrentHttpContext.Session[TokenAdmin] != null)
                    {
                        return (Uco.Models.Settings)LS.CurrentHttpContext.Session[TokenAdmin];
                    }
                    else
                    {
                        string Token = string.Format("CurrentSettingsMultiDomain_{0}_{1}", Domain, SF.GetLangCodeThreading());
                        if (LS.Cache[Token] == null)
                        {
                            lock (s_lock)
                            {
                                Settings s = GetSettingsRepositoryList().FirstOrDefault();
                                if (s == null) return null;

                                string LangCodeThreading = SF.GetLangCodeThreading();
                                if (SF.GetLangCodeWebconfig() != LangCodeThreading)
                                {
                                    LanguagePage l = _db.LanguagePages.FirstOrDefault(r => r.DomainID == s.ID && r.LanguageCode == LangCodeThreading);
                                    if (l != null)
                                    {
                                        s.LanguageCode = l.LanguageCode;
                                        //s.LanguageRTL = l.ShowOnMainPage;
                                        s.HeaderHtml = l.Text2;
                                        s.FotterHtml = l.Text3;
                                    }
                                }

                                LS.Cache[Token] = s;
                            }
                        }
                        return (Uco.Models.Settings)LS.Cache[Token];
                    }
                }
                else
                {
                    string Token = string.Format("CurrentSettingsMultiDomain_{0}_{1}", Domain, SF.GetLangCodeThreading());
                    if (LS.Cache[Token] == null)
                    {
                        lock (s_lock)
                        {
                            Settings s = GetSettingsRepositoryList().FirstOrDefault(r => r.Domain == Domain);
                            if (s == null) return null;

                            string LangCodeThreading = SF.GetLangCodeThreading();
                            if (SF.GetLangCodeWebconfig() != LangCodeThreading)
                            {
                                LanguagePage l = _db.LanguagePages.FirstOrDefault(r => r.DomainID == s.ID && r.LanguageCode == LangCodeThreading);
                                if (l != null)
                                {
                                    s.LanguageCode = l.LanguageCode;
                                    //s.LanguageRTL = l.ShowOnMainPage;
                                    s.HeaderHtml = l.Text2;
                                    s.FotterHtml = l.Text3;
                                }
                            }

                            LS.Cache[Token] = s;
                        }
                    }
                    return (Uco.Models.Settings)LS.Cache[Token];
                }
            }
        }

        public static void CleanSettingsRepository()
        {
            LS.Cache.Remove("SettingsAll");
            foreach (Settings item in RP.GetSettingsRepositoryList())
            {
                LS.Cache.Remove("CurrentSettingsMultiDomain_" + item.Domain + "_" + item.LanguageCode);
                LS.Cache.Remove("CurrentSettings_" + item.LanguageCode);
            }
            LS.Cache.Remove("CurrentSettingsMultiDomain_" + SF.GetCurrentDomain() + "_" + SF.GetLangCodeThreading());
            LS.CurrentHttpContext.Session["AdminCurrentSettingsRepository"] = null;
        }

        public static Uco.Models.Settings GetAdminCurrentSettingsRepository()
        {
            string Token = "AdminCurrentSettingsRepository";
            if (LS.CurrentHttpContext == null || LS.CurrentHttpContext.Session == null) return RP.GetCurrentSettings();
            if (LS.CurrentHttpContext.Session[Token] == null)
            {
                Settings s = RP.GetCurrentSettings();
                if (s == null) return null;
                LS.CurrentHttpContext.Session[Token] = s;
            }
            return (Uco.Models.Settings)LS.CurrentHttpContext.Session[Token];
        }

        public static void ChangeAdminCurrentSettingsRepository(int ID)
        {
            Settings s = GetSettingsRepositoryList().FirstOrDefault(r => r.ID == ID);
            if (s != null) LS.CurrentHttpContext.Session["AdminCurrentSettingsRepository"] = s;
        }
    }
}
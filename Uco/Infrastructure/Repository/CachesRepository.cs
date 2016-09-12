using System.Configuration;
using System.IO;
using System.Web;
using Uco.Infrastructure.Livecycle;
using Uco.Infrastructure.Repositories;

namespace Uco.Infrastructure
{
    public static partial class CleanCache
    {
        #region Other

        public static void RestartApplication()
        {
            HttpRuntime.Close();
        }

        public static void CleanCacheAfterPageEdit()
        {
            CleanMenuCache();
            CleanOutputCache();
        }

        public static void CleanImageCache()
        {
            string sourceDir = LS.CurrentHttpContext.Server.MapPath("~/App_Data/cache/images");
            foreach (string item in Directory.GetFiles(sourceDir, "*.*"))
            {
                System.IO.File.Delete(item);
            }
        }

        public static void CleanSearchCache()
        {
            string sourceDir = LS.CurrentHttpContext.Server.MapPath("~/App_Data/cache/search");
            foreach (string item in Directory.GetFiles(sourceDir, "*.*"))
            {
                System.IO.File.Delete(item);
            }
        }

        public static void CleanSettingsCache()
        {
            RP.CleanSettingsRepository();
        }

        public static void CleanMenuCache()
        {
            RP.CleanMenuRepository(RP.GetCurrentSettings().ID);
            RP.CleanMenuRepository(RP.GetAdminCurrentSettingsRepository().ID);
        }

        public static void CleanSettingsAllCache()
        {
            RP.CleanSettingsRepository();
        }

        public static void CleanOutputCache()
        {
            LS.CurrentHttpContext.Application["LangCodeDomainAndCleanCacheGuid_" + System.Threading.Thread.CurrentThread.CurrentCulture.Name + "_" + SF.GetCurrentDomain()] = null;
        }

        #endregion

    }

}
using Uco.Infrastructure.Livecycle;

namespace Uco.Infrastructure
{
    public static partial class SF
    {
        public static bool UseMultiDomain()
        {
            if (System.Configuration.ConfigurationManager.AppSettings["UseMultiDomain"] == null) return false;
            if (System.Configuration.ConfigurationManager.AppSettings["UseMultiDomain"].ToString() == "true") return true;
            else return false;
        }

        public static string GetCurrentDomain()
        {
            try { return LS.CurrentHttpContext.Request.Url.DnsSafeHost; }
            catch { return null; }
        }

    }
}
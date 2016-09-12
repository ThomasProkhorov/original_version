using System;
using System.Net;
using System.Web;
using Uco.Infrastructure.Livecycle;

namespace Uco.Infrastructure
{
    public static partial class SF
    {

        #region Environment

        public static void CheckEnvironment()
        {
            string host = LS.CurrentHttpContext.Request.ServerVariables["HTTP_HOST"];
            //if (host != "hostname.co.il" && host != "www.hostname.co.il" && !host.Contains("localhost")) throw new HttpException(500, "Unknown environment");
        }

        #endregion

        #region Host

        public static void PingHost()
        {
            string host = LS.CurrentHttpContext.Request.ServerVariables["HTTP_HOST"];
            string sURL = "http://www.uco.co.il/l.aspx?host=" + host;

            string HostResponse = string.Empty;
            try
            {
                using (WebClient client = new WebClient())
                {
                    HostResponse = client.DownloadString(sURL);
                }
            }
            catch { }

            if (HostResponse.Contains("blacklisted host")) throw new HttpException(500, "Blacklisted host");
        }

        #endregion
    }
	
	public class FirstRequestInitialization
    {
        private static bool s_InitializedAlready = false;
        private static Object s_lock = new Object();
        public static void Initialize(HttpContext context)
        {
            if (s_InitializedAlready) return;
            lock (s_lock)
            {
                if (s_InitializedAlready) return;

                SF.CheckEnvironment();
                SF.PingHost();

                s_InitializedAlready = true;
            }
        }
    }
}
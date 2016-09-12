using System.Collections.Generic;
using Uco.Infrastructure.Livecycle;


namespace Uco.Infrastructure.Repositories
{
    public static partial class RP
    {

        #region Get/Clean Repository

        public static void CleanPluginsRepository()
        {
            LS.Cache.Remove("PluginsRepository");
        }

        public static List<string> GetPluginsReprository()
        {
            if (!SF.UsePlugins()) return new List<string>();

            string Token = "TasksRepository";
            if (LS.Cache[Token] == null)
            {
                List<string> l = new List<string>();

                string PluginsString;

                string PluginsFileName = "~/Plugins/ActivePlugins.txt";

                string PluginsFilePath = System.Web.Hosting.HostingEnvironment.MapPath(PluginsFileName);
                if (System.IO.File.Exists(PluginsFilePath))
                {
                    System.IO.StreamReader PluginsFile = new System.IO.StreamReader(PluginsFilePath);
                    PluginsString = PluginsFile.ReadToEnd();
                    PluginsFile.Close();

                    if (!string.IsNullOrEmpty(PluginsString))
                    {
                        PluginsString = PluginsString.Replace("\r", "");
                        foreach (string item in PluginsString.Split('\n'))
                        {
                            if (!string.IsNullOrEmpty(item))
                            {
                                l.Add(item.Trim());
                            }
                        }
                    }
                }

                if (LS.CurrentHttpContext != null) LS.Cache[Token] = l;
                return l;
            }
            else return LS.Cache[Token] as List<string>;
        }

        #endregion

    }
}
namespace Uco.Infrastructure
{
    public static partial class SF
    {
        public static bool UsePlugins()
        {
            if (System.Configuration.ConfigurationManager.AppSettings["UsePlugins"] == null) return false;
            if (System.Configuration.ConfigurationManager.AppSettings["UsePlugins"].ToString() == "true") return true;
            else return false;
        }
        public static bool UseTasks()
        {
            if (System.Configuration.ConfigurationManager.AppSettings["UseTasks"] == null) return true;
            if (System.Configuration.ConfigurationManager.AppSettings["UseTasks"].ToString() == "false") return false;
            else return true;
        }
    }
}
namespace Uco.Infrastructure
{
    public static partial class SF
    {
        public static bool UseThemes()
        {
            if (System.Configuration.ConfigurationManager.AppSettings["UseThemes"] == null) return false;
            if (System.Configuration.ConfigurationManager.AppSettings["UseThemes"].ToString() == "true") return true;
            else return false;
        }
    }
}
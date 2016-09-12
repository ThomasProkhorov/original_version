namespace Uco.Infrastructure.Livecycle
{
    public static partial class LS
    {
        public static System.Web.HttpContext CurrentHttpContext
        {
            get
            {
                if (System.Web.HttpContext.Current == null) return null;
                else return System.Web.HttpContext.Current;
            }
        }
    }
}
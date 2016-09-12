using System.Configuration;
using System.Web.WebPages;

namespace Uco.Infrastructure
{
    public static partial class SF
    {

        public static void EvaluateDisplayMode()
        {
            if (ConfigurationManager.AppSettings["DisplayMode"].ToString() == "Mobile") DisplayModeProvider.Instance.Modes.Insert(0, new DefaultDisplayMode("Mobile"));
            else if (ConfigurationManager.AppSettings["DisplayMode"].ToString() == "Pc") DisplayModeProvider.Instance.Modes.Insert(0, new DefaultDisplayMode());
            else
            {
                DisplayModeProvider.Instance.Modes.Insert(0, new DefaultDisplayMode()
                    {
                        ContextCondition = (ctx => (ctx.Request.Browser.IsMobileDevice && ctx.Request.Browser.ScreenPixelsWidth >= 768))
                    });
            }
        }

    }
}
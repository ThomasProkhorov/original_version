using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Uco.Infrastructure
{
    public class StylesHelper
    {
        public ViewContext ViewContext { get; private set; }
        public StylesHelper(ViewContext viewContext)
        {
            ViewContext = viewContext;
        }

    }

    public static class StylesExtensions
    {
        public static IHtmlString Render(
            this StylesHelper helper,
            string urlbase
        )
    {
        return new HtmlString("<link href=\""+urlbase+".css\" rel=\"stylesheet\">");
    }
    }

    public abstract class ExtWebViewPage<T> : WebViewPage<T>
    {
        public override void InitHelpers()
        {
            base.InitHelpers();
            Styles = new StylesHelper(ViewContext);
        }

        public StylesHelper Styles { get; private set; }
    }

}
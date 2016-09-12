using System;
using System.Collections.Generic;
using System.Globalization;
using System.Web;
using System.Web.Mvc;

namespace Uco.Infrastructure
{
    public static class ScriptBundleHelper
    {
        public static HtmlString ScriptBundle(this HtmlHelper htmlHelper, params string[] Files)
        {
            string OutString = string.Empty;
            foreach (string item in Files)
            {
                OutString = string.Format("{0}\r\n<script type=\"text/javascript\" src=\"{1}\"></script>", OutString, item.Replace("~/","/"));
            }
            return new HtmlString(OutString);
        }
    }
}

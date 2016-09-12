using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web;
using Uco.Infrastructure.Livecycle;
using Uco.Infrastructure.Repositories;
using Uco.Models;

namespace Uco.Infrastructure
{
    public static partial class SF
    {
        public static void SetCookie(string CookieName, string CookieValue)
        {
            //Create a new cookie, passing the name into the constructor
            System.Web.HttpCookie cookie = new System.Web.HttpCookie(UcoString.GetUtf8String(CookieName));
            //Set the cookies value
            cookie.Value = CookieValue;
            //Set the cookie to expire in 1 day
            DateTime dtNow = DateTime.Now;
            TimeSpan tsMinute = new TimeSpan(1, 0, 0, 0);
            cookie.Expires = dtNow + tsMinute;
            //Add the cookie
            LS.CurrentHttpContext.Response.Cookies.Add(cookie);
        }

        public static string GetCookie(string CookieValue)
        {
            //Grab the cookie
            System.Web.HttpCookie cookie = LS.CurrentHttpContext.Request.Cookies[CookieValue];
            //Check to make sure the cookie exists
            if (cookie == null) return string.Empty;
            else
            {
                if (cookie.Value == null) return string.Empty;
                else return cookie.Value;
            }
        }
    }
}
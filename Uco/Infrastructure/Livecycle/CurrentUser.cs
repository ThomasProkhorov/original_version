using Uco.Models;
using System.Linq;
using System;
using System.Web;
using System.Web.Security;
using Uco.Infrastructure.Services;
namespace Uco.Infrastructure.Livecycle
{
    public static partial class LS
    {
        public static string GetUser_IP(HttpRequest request)
        {
            string VisitorsIPAddr = string.Empty;
            if (request.ServerVariables["HTTP_X_FORWARDED_FOR"] != null)
            {
                VisitorsIPAddr = request.ServerVariables["HTTP_X_FORWARDED_FOR"].ToString();
            }
            else if (request.UserHostAddress.Length != 0)
            {
                VisitorsIPAddr = request.UserHostAddress;
            }
            return VisitorsIPAddr;
        }
        public static string GetUser_IP(HttpRequestBase request)
        {
            string VisitorsIPAddr = string.Empty;
            if (request.ServerVariables["HTTP_X_FORWARDED_FOR"] != null)
            {
                VisitorsIPAddr = request.ServerVariables["HTTP_X_FORWARDED_FOR"].ToString();
            }
            else if (request.UserHostAddress.Length != 0)
            {
                VisitorsIPAddr = request.UserHostAddress;
            }
            return VisitorsIPAddr;
        }
        public static int CurrentShopID
        {
            get
            {
                if (LS.CurrentHttpContext != null)
                {
                    if (LS.CurrentHttpContext.Session["CurrentShopID"] != null)
                    {
                        return (int)LS.CurrentHttpContext.Session["CurrentShopID"];
                    }
                }
                return 0;
            }
        }
        public static void Authorize(User user)
        {
            var oldGuid = LS.CurrentUser.ID;
            FormsAuthentication.SetAuthCookie(user.UserName, true);
            if(Guid.Empty != oldGuid && oldGuid!=user.ID)
            {
                //now need save old cart and new cart
                //and after user asking migrate  or remove old products
                
                //1) save old id to session
                var have = ShoppingCartService.SwitchShoppingCart(oldGuid, user.ID);
                if (have > 0)
                {
                   var newCustomerGuidCookie = new HttpCookie(cookieOldCartname);
                   newCustomerGuidCookie.Value = oldGuid.ToString();

                    int cookieExpires = 24 * 365; //TODO make configurable
                    newCustomerGuidCookie.Expires = DateTime.Now.AddHours(cookieExpires);

                    LS.CurrentHttpContext.Response.Cookies.Add(newCustomerGuidCookie);
                    LS.CurrentHttpContext.Request.Cookies.Add(newCustomerGuidCookie);
                }
                else
                {
                    DeleteCookie(cookieOldCartname);
                }
                
            }
            //remove cookie
            DeleteCookie(cookiename);
        }
        public static void DeleteCookie(string key)
        {
            if (LS.CurrentHttpContext != null)
            {
                HttpCookie currentUserCookie = LS.CurrentHttpContext.Request.Cookies[key];
                if (currentUserCookie != null)
                {
                    LS.CurrentHttpContext.Response.Cookies.Remove(key);
                    currentUserCookie.Expires = DateTime.Now.AddDays(-10);
                    currentUserCookie.Value = null;

                    LS.CurrentHttpContext.Response.SetCookie(currentUserCookie);
                }
            }
        }
        public static readonly string cookiename = "SALcustomer";
        public static readonly string cookieOldCartname = "SALcart";
        public static User CurrentUser
        {
            get
            {
                if (LS.CurrentHttpContext == null) return new User() { UserName = "Anonymous", Roles = "|Anonymous|" };
                else if (LS.CurrentHttpContext.Items["_CurrentUser"] == null)
                {
                    var guest = new User() { UserName = "Anonymous", Roles = "|Anonymous|" };
                    Guid customerGuid = Guid.Empty;
                    HttpCookie cookieCustomerID = LS.CurrentHttpContext.Request.Cookies[cookiename];
                    if (cookieCustomerID != null)
                    {
                        if (cookieCustomerID != null && !String.IsNullOrEmpty(cookieCustomerID.Value))
                        {
                            Guid.TryParse(cookieCustomerID.Value, out customerGuid);
                        }
                    }
                    if (customerGuid == Guid.Empty)
                    {
                        customerGuid = Guid.NewGuid();
                        var newCustomerGuidCookie = new HttpCookie(cookiename);
                        newCustomerGuidCookie.Value = customerGuid.ToString();

                        int cookieExpires = 24 * 365; //TODO make configurable
                        newCustomerGuidCookie.Expires = DateTime.Now.AddHours(cookieExpires);

                        LS.CurrentHttpContext.Response.Cookies.Add(newCustomerGuidCookie);
                    }
                    guest.ID = customerGuid;
                    LS.CurrentHttpContext.Items["_CurrentUser"] = guest;

                    return LS.CurrentHttpContext.Items["_CurrentUser"] as User;
                }
                else return LS.CurrentHttpContext.Items["_CurrentUser"] as User;
            }
        }
        public static bool isHaveID()
        {

            return Guid.Empty != CurrentUser.ID;
        }
        public static bool isLogined()
        {

            return "Anonymous" != CurrentUser.UserName;
        }
        public static Shop CurrentShop
        {
            get
            {
                if (LS.CurrentHttpContext.Items["_CurrentShop"] == null)
                {
                    if (LS.CurrentShopID > 0)
                    {
                        var curid = LS.CurrentShopID;
                        Shop ms = CurrentEntityContext.Shops.FirstOrDefault(r => r.UserID == CurrentUser.ID
                            && r.ID == curid);
                        if (ms != null)
                        {
                            LS.CurrentHttpContext.Items["_CurrentShop"] = ms;
                            return ms;
                        }
                    }
                    Shop m = CurrentEntityContext.Shops.FirstOrDefault(r => r.UserID == CurrentUser.ID);
                    if (m == null) return null;
                    LS.CurrentHttpContext.Items["_CurrentShop"] = m;
                }
                return LS.CurrentHttpContext.Items["_CurrentShop"] as Shop;
            }
        }
    }
}
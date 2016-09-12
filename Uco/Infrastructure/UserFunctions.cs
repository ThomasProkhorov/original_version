using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.Security;
using Uco.Infrastructure.Livecycle;
using Uco.Infrastructure.Providers;
using Uco.Models;

namespace Uco.Infrastructure
{
    public static partial class SF
    {

        public static bool UseCurrentUserByUserName(string UserName)
        {
            return (LS.CurrentUser != null && LS.CurrentUser.UserName == UserName);
        }

        public static bool UseCurrentUserByEmail(string Email)
        {
            return (LS.CurrentUser != null && LS.CurrentUser.Email == Email);
        }

        public static bool CreateUser(string UserName, string Password, string Email, List<string> Roles, string DefaultRole)
        {
            Membership.CreateUser(UserName, Password, Email);
            SURoleProvider rp = new SURoleProvider();
            rp.AddUsersToRoles(new string[] { UserName }, Roles.ToArray(), DefaultRole);
            return true;
        }

    }
}
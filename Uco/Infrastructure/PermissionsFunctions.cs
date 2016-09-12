using System.Configuration;
using System.Web;
using Uco.Infrastructure.Repositories;
using Uco.Models;
using System.Linq;
using System.Collections.Generic;
using System.Data.Entity;
using Uco.Infrastructure.Livecycle;
using System;

namespace Uco.Infrastructure
{
    public static partial class SF
    {
        public static bool UsePermissions()
        {
            if (System.Configuration.ConfigurationManager.AppSettings["UsePermissions"] == null) return false;
            if (System.Configuration.ConfigurationManager.AppSettings["UsePermissions"].ToString() == "true") return true;
            else return false;
        }

        public static void UpdateChildPermissions(string PermissionsViewValue, string PermissionsEditValue, int ParentID)
        {
            UpdateChildPermissionsRecurcive(PermissionsViewValue, PermissionsEditValue, ParentID, 100);
        }

        private static void UpdateChildPermissionsRecurcive(string PermissionsViewValue, string PermissionsEditValue, int ParentID, int MaxLoops)
        {
            if (MaxLoops <= 0) return;
            MaxLoops = MaxLoops - 1;
            Db _db = LS.CurrentEntityContext;

            foreach (AbstractPage item in _db.AbstractPages.Where(r => r.ParentID == ParentID).ToList())
            {
                item.PermissionsView = PermissionsViewValue;
                item.PermissionsEdit = PermissionsEditValue;
                item.PermissionsUpdateChildPages = true;
                _db.Entry(item).State = EntityState.Modified;
                _db.SaveChanges();
                UpdateChildPermissionsRecurcive(PermissionsViewValue, PermissionsEditValue, item.ID, MaxLoops);
            }
        }

        public static bool UserCanView(User CurrentUser, AbstractPage CurrentPage)
        {
            List<string> CurrentUserPermissions = new List<string>();

            if (CurrentUser == null) CurrentUserPermissions.Add("Anonymous");
            else CurrentUserPermissions = SF.RolesStringToList(CurrentUser.Roles);

            List<string> CurrentPagePermissions = SF.RolesStringToList(CurrentPage.PermissionsView);

            if (CurrentUserPermissions.Intersect<string>(CurrentPagePermissions).Count() > 0) return true;
            else return false;
        }

        public static List<string> AdminControlersList()
        {
            return  RP.GetAdminControlerTypesRepository().Select(r => r.Name).OrderBy(r => r).ToList();
        }


        public static bool CheckAdminControlersPermissions(Type item)
        {
            List<string> RolesList = LS.CurrentUser.RolesList;
            if (RolesList.Contains("Admin")) return true;
            else if (RolesList.Count() == 0) return false;
            else if (RolesList.Count() == 1 && RolesList.Contains("Anonymous")) return false;

            List<string> DomainRolesList = RP.GetAdminCurrentSettingsRepository().RolesList;
            List<string> SystemRolesList = RP.GetRolesReprository().Where(r => r.IsSystem).Select(r => r.Title).ToList();
            foreach (string SystemRoles in SystemRolesList)
            {
                DomainRolesList.Remove(SystemRoles);
            }

            if (DomainRolesList.Intersect(RolesList).Count() == 0) return false;

            List<string> MenuPermissionsList = new List<string>();
            foreach (Role role in RP.GetRolesReprository().Where(r => RolesList.Contains(r.Title)).ToList())
            {
                MenuPermissionsList.AddRange(role.MenuPermissionsList);
            }

            if (MenuPermissionsList.Contains(item.Name)) return true;
            else return false;
        }

    }
}
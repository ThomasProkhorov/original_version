using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web.Security;
using Uco.Infrastructure.Livecycle;
using Uco.Infrastructure.Services;
using Uco.Models;

namespace Uco.Infrastructure.Providers
{
    public class SURoleProvider : RoleProvider
    {
        #region properties
        private static DBContextService _dbService;
        private static DateTime _openedTime;
        private static int _timeOut=2;
        private static Db _db
        {
            get {
                if (_dbService == null || _openedTime == null || _openedTime.AddMinutes(_timeOut) < DateTime.Now)
                {
                    _dbService = new DBContextService();
                    _openedTime = DateTime.Now;
                }
                return _dbService.EntityContext;
            }
        }

        #endregion

        public override void AddUsersToRoles(string[] UserNames, string[] roleNames)
        {
            foreach (string item1 in UserNames)
            {
                User item2 = _db.Users.FirstOrDefault(r => r.UserName == item1);
                if (item2 == null) continue;
                foreach (string item3 in roleNames)
                {
                    item2.Roles = SF.RolesStringAdd(item2.Roles, item3);
                }
                _db.SaveChanges();
                _db.Entry(item2).State = EntityState.Detached;
            }

        }

        public void AddUsersToRoles(string[] UserNames, string[] roleNames, string DefaultRole)
        {
            foreach (string item1 in UserNames)
            {
                User item2 = _db.Users.FirstOrDefault(r => r.UserName == item1);
                if (item2 == null) continue;
                foreach (string item3 in roleNames)
                {
                    item2.Roles = SF.RolesStringAdd(item2.Roles, item3);
                    item2.RoleDefault = DefaultRole;
                }
                _db.SaveChanges();
                _db.Entry(item2).State = EntityState.Detached;
            }

        }

        public override string ApplicationName
        {
            get
            {
                return "/";
            }
            set
            {
                throw new NotImplementedException();
            }
        }

        private static string GetConfigValue(string configValue, string defaultValue)
        {
            return String.IsNullOrEmpty(configValue) ? defaultValue : configValue;
        }

        public override void CreateRole(string roleName)
        {
            if (string.IsNullOrEmpty(roleName)) return;
            roleName = roleName.Trim();
            if (_db.Roles.Count(r => r.Title.ToLower() == roleName.ToLower()) > 0) return;
            _db.Roles.Add(new Role() { IsSystem = false, Title = roleName });
            _db.SaveChanges();
        }

        public override bool DeleteRole(string roleName, bool throwOnPopulatedRole)
        {
            Role role = _db.Roles.FirstOrDefault(r => r.Title == roleName);
            if (role.IsSystem) return false;
            if (role != null)
            {
                foreach (User item in GetUsersListInRole(roleName))
                {
                    if (!throwOnPopulatedRole) return false;
                    if (item == null) continue;
                    item.Roles = SF.RolesStringDelete(item.Roles, roleName);
                    _db.Entry(item).State = EntityState.Modified;
                }
                _db.Entry(role).State = EntityState.Deleted;
                _db.SaveChanges();
                return true;
            }
            else return false;
        }

        public override string[] FindUsersInRole(string roleName, string UserNameToMatch)
        {
            List<string> Users = new List<string>();
            string Role = "|" + roleName + "|";
            foreach (User item in _db.Users.Where(r => r.Roles.Contains(Role) && r.UserName == UserNameToMatch))
            {
                Users.Add(item.UserName);
            }
            return Users.ToArray();
        }

        public override string[] GetAllRoles()
        {
            return _db.Roles.Select(r => r.Title).ToArray();
        }

        public override string[] GetRolesForUser(string UserName)
        {
            List<string> Roles = new List<string>();

            User u;
            if (SF.UseCurrentUserByUserName(UserName)) u = LS.CurrentUser;
            else u = _db.Users.FirstOrDefault(r => r.UserName == UserName);
            if (!SF.UseCurrentUserByUserName(UserName) && u != null) _db.Entry(u).State = EntityState.Detached;
            if (u != null) Roles.AddRange(SF.RolesStringToList(u.Roles));

            return Roles.ToArray();
        }

        public override string[] GetUsersInRole(string roleName)
        {
            List<string> Users = new List<string>();

            roleName = "|" + roleName + "|";
            foreach (User item in _db.Users.Where(r => r.Roles.Contains(roleName)))
            {
                Users.Add(item.UserName);
            }

            return Users.ToArray();
        }

        public List<User> GetUsersListInRole(string roleName)
        {
            List<User> Users = new List<User>();

            roleName = "|" + roleName + "|";
            foreach (User item in _db.Users.Where(r => r.Roles.Contains(roleName)))
            {
                Users.Add(item);
            }

            return Users;
        }

        public override bool IsUserInRole(string UserName, string roleName)
        {

            User user;
            if (SF.UseCurrentUserByUserName(UserName)) user = LS.CurrentUser;
            else user = _db.Users.FirstOrDefault(r => r.UserName == UserName);
            if (!SF.UseCurrentUserByUserName(UserName) && user != null) _db.Entry(user).State = EntityState.Detached;

            if (user == null) return false;
            if (string.IsNullOrEmpty(user.Roles)) return false;
            if (user.Roles.Contains("|" + roleName + "|")) return true;
            else return false;
        }

        public override void RemoveUsersFromRoles(string[] UserNames, string[] roleNames)
        {
            foreach (string item1 in UserNames)
            {
                User item2 = _db.Users.FirstOrDefault(r => r.UserName == item1);
                if (item2 == null) continue;
                foreach (string item3 in roleNames)
                {
                    item2.Roles = SF.RolesStringDelete(item2.Roles, item3);
                }
                _db.SaveChanges();
                _db.Entry(item2).State = EntityState.Detached;
            }
        }

        public override bool RoleExists(string roleName)
        {
            List<string> Roles = this.GetAllRoles().ToList();
            return Roles.Contains(roleName);
        }

    }
}

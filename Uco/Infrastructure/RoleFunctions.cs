using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Web;
using Uco.Infrastructure.Livecycle;
using Uco.Infrastructure.Repositories;
using Uco.Models;

namespace Uco.Infrastructure
{
    public static partial class SF
    {

        public static string RolesListToString(string[] l)
        {
            return RolesListToString(l.ToList());
        }

        public static string RolesListToString(List<string> l)
        {
            string Roles = "|";
            if (l == null) return Roles;
            foreach (string item in l)
            {
                Roles = Roles + item + "|";
            }
            return Roles;
        }

        public static List<string> RolesStringToList(string s)
        {
            List<string> Roles = new List<string>();
            if (string.IsNullOrEmpty(s)) return Roles;
            foreach (string item in s.Split('|'))
            {
                if (string.IsNullOrEmpty(item)) continue;
                Roles.Add(item);
            }
            return Roles;
        }

        public static string RolesStringAdd(string Roles, string newRole)
        {
            if (string.IsNullOrEmpty(newRole)) return Roles;
            if (string.IsNullOrEmpty(Roles)) return "|" + newRole + "|";

            if (!Roles.EndsWith("|")) Roles = Roles + "|";
            Roles = Roles + newRole + "|";

            return Roles;
        }

        public static string RolesStringDelete(string Roles, string oldRole)
        {
            if (string.IsNullOrEmpty(oldRole)) return Roles;
            if (string.IsNullOrEmpty(Roles)) return "|";

            if (!Roles.Contains("|" + oldRole + "|")) return Roles;
            Roles = Roles.Replace("|" + oldRole + "|", "|");

            return Roles;
        }

        public static List<string> GetRoleList()
        {
            return RP.GetRolesReprository().Select(r => r.Title).ToList();
        }

        public static List<Role> GetRoleObjectsList()
        {
            return RP.GetRolesReprository().ToList();
        }

    }
}
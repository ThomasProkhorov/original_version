using System.Collections.Generic;
using System.Linq;
using System.Web;
using Uco.Infrastructure.Livecycle;
using Uco.Models;

namespace Uco.Infrastructure.Repositories
{
    public static partial class RP
    {

        #region Get/Clean Repository

        public static void CleanRolesRepository()
        {
            LS.Cache.Remove("RolesReprository");
        }

        public static List<Role> GetRolesReprository()
        {
            if (LS.Cache["RolesReprository"] == null)
            {
                List<Role> l = _db.Roles.ToList();
                LS.Cache["RolesReprository"] = l;
                return l;
            }
            else return LS.Cache["RolesReprository"] as List<Role>;
        }

        #endregion

    }
}
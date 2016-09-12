using System;
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

        public static void CleanAdminControlerTypesRepository()
        {
            LS.Cache.Remove("AdminControlerTypesRepository");
        }

        public static List<Type> GetAdminControlerTypesRepository()
        {
            string Token = "AdminControlerTypesRepository";
            if (LS.Cache[Token] == null)
            {
                SF.PrepareReflection();
            }
            return LS.Cache[Token] as List<Type>;
        }

        #endregion

    }
}
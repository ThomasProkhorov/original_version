using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection;
using System.Web;
using Uco.Infrastructure.Livecycle;
using Uco.Models;

namespace Uco.Infrastructure.Repositories
{
    public static partial class RP
    {

        #region Get/Clean Repository

        public static void CleanPageTypesReprository()
        {
            LS.Cache.Remove("PageTypesRepository");
        }

        public static List<Type> GetPageTypesReprository()
        {
            string Token = "PageTypesRepository";
            if (LS.Cache[Token] == null)
            {
                SF.PrepareReflection();
            }
            return LS.Cache[Token] as List<Type>;
        }

        #endregion
    }
}
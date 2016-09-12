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

        public static void CleanPageTypesDictionaryReprository()
        {
            LS.Cache.Remove("PageTypesDictionaryReprository");
        }

        public static Dictionary<string, Type> GetPageTypesDictionaryReprository()
        {
            string Token = "PageTypesDictionaryReprository";
            if (LS.Cache[Token] == null)
            {
                SF.PrepareReflection();
            }
            return LS.Cache[Token] as Dictionary<string, Type>;
        }

        #endregion
    }
}
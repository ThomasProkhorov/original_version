using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Web;
using System.Web.Compilation;
using Uco.Infrastructure.Livecycle;
using Uco.Infrastructure.Tasks;
using Uco.Models;


namespace Uco.Infrastructure.Repositories
{
    public static partial class RP
    {
        #region Get/Clean Repository

        public static void CleanTasksReprository()
        {
            LS.Cache.Remove("TasksRepository");
        }

        public static List<ITask> GetTasksReprository()
        {
            string Token = "TasksRepository";
            if (LS.Cache[Token] == null)
            {
                SF.PrepareReflection();
            }
            return LS.Cache[Token] as List<ITask>;
        }

        #endregion
    }
}
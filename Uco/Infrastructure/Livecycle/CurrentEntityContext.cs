using System;
using System.Runtime.Remoting.Messaging;
using System.Threading;
using Uco.Models;

namespace Uco.Infrastructure.Livecycle
{
    public static partial class LS
    {
        public static Db CurrentEntityContext
        {
            get
            {
                if (LS.CurrentHttpContext == null) return new Db();
                else if (LS.CurrentHttpContext.Items["_EntityContext"] == null) return null;
                else return LS.CurrentHttpContext.Items["_EntityContext"] as Db;
            }
        }
       
    }
}
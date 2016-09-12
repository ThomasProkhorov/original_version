using System;
using System.Collections.Generic;
using System.Web;
using System.Web.Caching;
using Uco.Infrastructure;
using Uco.Infrastructure.Repositories;
using Uco.Infrastructure.Tasks;
using System.Linq;

namespace Uco
{
    public class TasksConfig
    {
        public static void StartTasks()
        {
            foreach (var instance in RP.GetTasksReprository())
            {
                AddTask(instance.Title, instance.StartSeconds);
            }

        }

        private static CacheItemRemovedCallback OnCacheRemove = null;

        private static void AddTask(string name, int seconds)
        {
            OnCacheRemove = new CacheItemRemovedCallback(CacheItemRemoved);
            HttpRuntime.Cache.Insert(name, seconds, null,
                DateTime.Now.AddSeconds(seconds), Cache.NoSlidingExpiration,
                CacheItemPriority.NotRemovable, OnCacheRemove);
        }

        public static void CacheItemRemoved(string k, object v, CacheItemRemovedReason r)
        {
            foreach (var instance in RP.GetTasksReprository())
            {
                if (k == instance.Title)
                {
                    try
                    {
                        instance.Execute();
                    }
                    catch (Exception e)
                    {
                        SF.LogError(e);
                    }
                    AddTask(k, SF.RandomNumber(instance.IntervalSecondsFrom, instance.IntervalSecondsTo));
                }
            }
        }
    }
}

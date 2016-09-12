using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net.Mail;
using System.Text;
using Uco.Infrastructure.Livecycle;
using Uco.Models;

namespace Uco.Infrastructure.Tasks
{
    //
    public class ImageSyncTask //: ITask 
    {

        public string Title { get { return "ImageImportSync"; } }
        public int StartSeconds { get { return 5; } }
        public int IntervalSecondsFrom { get { return 10; } }
        public int IntervalSecondsTo { get { return 30; } }
        public static object _lock = new object();
        public void Execute()
        {
            lock (_lock)
            {
                string key = "image.importsync.lock";

                if (LS.IsExistInCache(key) && LS.GetFromCache<bool>(key))
                {
                    return; // already running
                }
                LS.SetToCache(true, key);
                using (Db _db = new Db())
                {

                }

                //laset process
                LS.SetToCache(false, key);
            }
        }
    }
}
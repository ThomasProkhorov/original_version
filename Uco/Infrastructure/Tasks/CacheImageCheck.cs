using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Hosting;
using Uco.Infrastructure.Livecycle;
using Uco.Models;

namespace Uco.Infrastructure.Tasks
{
    public class CacheImageCheckTask : ITask
    {

        public static object _lock = new object();
        private static string LASTIDKEY = "ImageCache_LastProductID";
        private static bool isRunned = false;
        private static int porsionLimit = 1000;
        public void Execute()
        {

            if (!isRunned)
            {
                var needReturn = false;
                lock (_lock)
                {
                    if (isRunned)
                    {
                        needReturn = true;
                    }
                    else
                    {
                        isRunned = true;
                    }
                }
                if (needReturn)
                {
                    return;
                }
                try
                {
                    using (Db _db = new Db())
                    {
                        var lastProductID = _db.ShopSettings.FirstOrDefault(x => x.Key == LASTIDKEY);
                        int lastID = 0;
                        if (lastProductID != null)
                        {
                            int.TryParse(lastProductID.Value, out lastID);
                            lastProductID.Value = (lastID + porsionLimit).ToString();
                            _db.SaveChanges();

                        }
                        else
                        {
                            lastProductID = new ShopSetting() { Key = LASTIDKEY, Value = (lastID + porsionLimit).ToString() };
                            _db.ShopSettings.Add(lastProductID);
                            _db.SaveChanges();
                        }
                        int pictureSize = LS.Get<Settings>().FirstOrDefault().ProductBoxImageSize;
                        if (pictureSize == 0)
                        {
                            pictureSize = 174;
                        }
                        var products = _db.Products.Where(x => x.HasImage && x.ID > lastID)
                             .Select(x => new
                             {
                                 x.ID,
                                 x.Image
                             }).Take(porsionLimit).ToList();
                        if (products.Count == 0)
                        {
                            //reset position
                            lastProductID.Value = 0.ToString();
                            _db.SaveChanges();
                        }
                        foreach (var p in products)
                        {
                            try
                            {
                                SF.GetImage(p.Image, pictureSize, pictureSize, true, true);
                            }
                            catch { }
                        }


                    }
                }
                finally
                {
                    isRunned = false;
                }

            }



        }

        public string Title { get { return "CacheImageCheck"; } }

        public int StartSeconds { get { return 10; } }
        public int IntervalSecondsFrom { get { return 60; } }
        public int IntervalSecondsTo { get { return 60; } }
    }
}
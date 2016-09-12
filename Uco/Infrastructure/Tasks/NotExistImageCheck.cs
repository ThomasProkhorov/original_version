using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Hosting;
using Uco.Infrastructure.Livecycle;
using Uco.Models;

namespace Uco.Infrastructure.Tasks
{
    public class NotExistImageCheckTask //:ITask
    {

        public static object _lock = new object();
        public void Execute()
        {


            string key = "image.existchek.lastid";
            int fromID = 0;
            if (LS.IsExistInCache(key))
            {
                fromID = LS.GetFromCache<int>(key);
            }

            using (Db _db = new Db())
            {
                var products = _db.Products.Where(x => x.HasImage && x.ID > fromID)
                    .Select(x => new
                    {
                        x.ID,
                        x.Image
                    }).Take(1000).ToList();
                LS.SetToCache(products.Select(x => x.ID).DefaultIfEmpty(0).Max(), key);
                foreach (var p in products)
                {
                    if (!string.IsNullOrEmpty(p.Image))
                    {
                        var path = HostingEnvironment.MapPath("~" + p.Image);
                        if (path.Contains("wwwroot"))
                        {
                            if (!System.IO.File.Exists(path))
                            {
                                var sql = string.Format("UPDATE [{0}] SET [Image] = null, HasImage = 0  WHERE [ID] = {1} ",
                                "Products",
                                p.ID//,
                                    // p.Image.Replace("'", @"''") //fix string insert
                                );
                                _db.Database.ExecuteSqlCommand(sql);
                            }
                        }
                    }
                    else
                    {
                        var sql = string.Format("UPDATE [{0}] SET [Image] = null, HasImage = 0  WHERE [ID] = {1} ",
                            "Products",
                            p.ID//,
                            // p.Image.Replace("'", @"''") //fix string insert
                            );
                        _db.Database.ExecuteSqlCommand(sql);
                    }
                }
            }





        }

        public string Title { get { return "NotExistImageCheckTask"; } }

        public int StartSeconds { get { return 10; } }
        public int IntervalSecondsFrom { get { return 120; } }
        public int IntervalSecondsTo { get { return 360; } }
    }
}
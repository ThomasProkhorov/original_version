using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Net.Mail;
using System.Text;
using System.Web.Hosting;
using Uco.Infrastructure.Livecycle;
using Uco.Models;

namespace Uco.Infrastructure.Tasks
{
    public class CategoryHideTask : ITask
    {

        public string Title { get { return "CategoryHideTask"; } }
        public int StartSeconds { get { return 3600; } }
        public int IntervalSecondsFrom { get { return 3600; } }
        public int IntervalSecondsTo { get { return 3900; } }
        public void Execute()
        {
            using (Db _db = new Db())
            {
                //hide categories
                var filename = @"\App_Data\Tasks\CategoryHideTask.sql";
                var file = HostingEnvironment.ApplicationPhysicalPath + filename;
                if (File.Exists(file))
                {
                    var sql = File.ReadAllText(file);
                    _db.Database.ExecuteSqlCommand(sql);
                }

                //check if productshop not in category
                filename = @"\App_Data\Tasks\NotInCategory.sql";
                file = HostingEnvironment.ApplicationPhysicalPath + filename;
                if (File.Exists(file))
                {
                    var sql = File.ReadAllText(file);
                    _db.Database.ExecuteSqlCommand(sql);
                }

                //mark products as discounted
                filename = @"\App_Data\Tasks\DiscounedProducts.sql";
                file = HostingEnvironment.ApplicationPhysicalPath + filename;
                if (File.Exists(file))
                {
                    var sql = File.ReadAllText(file);
                    _db.Database.ExecuteSqlCommand(sql);
                }

                LS.RemoveFromCache(typeof(ShopCategory).Name);
                LS.RemoveFromCache(typeof(ShopCategoryMenu).Name);
                //var cats = LS.Get<Category>();
                //var shopCats = LS.Get<ShopCategory>();
                //var shops = LS.Get<Shop>();
                //foreach (var sh in shops)
                //{
                //    var products = (from ps in _db.ProductShopMap
                //                    join p in _db.Products
                //                    on ps.ProductID equals p.ID
                //                    where ps.ShopID == sh.ID
                //                    select new { ps.ID, ps.ProductID, p.CategoryID }).ToList();

                //}
            }
        }
    }
}
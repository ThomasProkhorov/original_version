using System.Collections.Generic;
using System.Configuration;
using System.Data.Entity;
using System.Web;
using System.Web.Security;
using Uco.Infrastructure;
using Uco.Infrastructure.Providers;
using Uco.Models;
using System.Linq;
using System.Data.Entity.Core.EntityClient;
using System.Data.SqlClient;

namespace Uco
{
    public class DbConfig
    {
        public static void InitDB()
        {
            if (ConfigurationManager.AppSettings["DbMigrations"].ToString() == "true")
            {
                Database.SetInitializer(new Uco.Models.Migrations.DbMigrationsInitializer());

                using (Db _db = new Db())
                {
					HttpContext.Current.Items["_EntityContext"] = _db;
										
                    if (!_db.Database.Exists())
                    {
                        _db.Database.Create();
                    }

                    if(_db.SettingsAll.Count() == 0)
                    {
                        //create DomainPage
                        DomainPage lp = new DomainPage();
                        lp.ID = 1;
                        lp.ParentID = 0;
                        lp.Title = "DomainPage";
                        lp.Visible = true;
                        lp.ShowInSitemap = true;
                        lp.ShowInMenu = true;
                        lp.SeoUrlName = "1";
                        lp.ShowInAdminMenu = true;
                        lp.DomainID = 1;
                        lp.PermissionsView = SF.RolesListToString(new List<string>() { "Admin", "Anonymous" });
                        lp.PermissionsEdit = SF.RolesListToString(new List<string>() { "Admin" });
                        _db.DomainPages.Add(lp);
                        _db.SaveChanges();

                        //create setting
                        Settings SettingsAll = new Settings();
                        SettingsAll.DomainPageID = lp.ID;
                        SettingsAll.Domain = "Default";
                        SettingsAll.AdminEmail = "info@uco.co.il";
                        SettingsAll.LanguageCode = ((System.Web.Configuration.GlobalizationSection)ConfigurationManager.GetSection("system.web/globalization")).UICulture; ;
                        SettingsAll.Roles = SF.RolesStringAdd("","Admin");
                        _db.SettingsAll.Add(SettingsAll);
                        _db.SaveChanges();

                        //create roles
                        _db.Roles.Add(new Role() { IsSystem = true, Title = "Admin" });
                        _db.Roles.Add(new Role() { IsSystem = true, Title = "Anonymous" });
                        _db.SaveChanges();

                        //update settings reference
                        lp.DomainID = SettingsAll.ID;
                        _db.SaveChanges();

                        //create UNIQUE INDEX on SeoUrlName
                        _db.Database.ExecuteSqlCommand("CREATE UNIQUE INDEX IX_AbstractPage_SeoUrlName ON AbstractPages (DomainID,RouteUrl,SeoUrlName)");

                        CleanCache.CleanOutputCache();

                        //create admin user
                        SF.CreateUser("admin", "adminadmin", "admin@uco.co.il", new List<string>() { "Admin" }, "Admin");
                    }
                }
            }
            else Database.SetInitializer<Db>(null);
        }

        public static bool TestConnection()
        {
            ConnectionStringSettings entityConString = ConfigurationManager.ConnectionStrings["Db"];
            string providerConnectionString = entityConString.ConnectionString;

            SqlConnectionStringBuilder conStringBuilder = new SqlConnectionStringBuilder();
            conStringBuilder.ConnectionString = providerConnectionString;
            conStringBuilder.ConnectTimeout = 1;
            string constr = conStringBuilder.ConnectionString;

            using (SqlConnection conn = new SqlConnection(constr))
            {
                try
                {
                    conn.Open();
                    return true;
                }
                catch
                {
                    return false;
                }
            }
        }
    }
}

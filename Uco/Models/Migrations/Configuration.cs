namespace Uco.Models.Migrations
{
    using System.Data.Entity;
    using System.Data.Entity.Migrations;

    public class DbMigrationsInitializer : MigrateDatabaseToLatestVersion<Db, Uco.Models.Migrations.DbMigrationsConfiguration>
    {

    }

    public sealed class DbMigrationsConfiguration : DbMigrationsConfiguration<Uco.Models.Db>
    {
        public DbMigrationsConfiguration()
        {
            if (System.Configuration.ConfigurationManager.AppSettings["DbDataLoss"] == null) AutomaticMigrationDataLossAllowed = false;
            else
            {
                if (System.Configuration.ConfigurationManager.AppSettings["DbDataLoss"].ToString() == "true") AutomaticMigrationDataLossAllowed = true;
                else AutomaticMigrationDataLossAllowed = false;
            }

            AutomaticMigrationsEnabled = true;
            MigrationsDirectory = "~/Models/Migrations";
        }

        protected override void Seed(Db _db)
        {
            base.Seed(_db);
        }
    }
}

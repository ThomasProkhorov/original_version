using System;
using System.Configuration;
using System.Data.Entity;
using System.Data.Entity.Core.Objects;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Threading;
using Uco.Infrastructure;
using Uco.Infrastructure.Repositories;
using Uco.Infrastructure.Livecycle;
using System.Reflection;
using System.Linq.Expressions;
using Uco.Models.Shopping.Measure;

namespace Uco.Models
{
    public class Db : DbContext
    {
        public Db()
            : base()
        {
            var objectContext = ((IObjectContextAdapter)this).ObjectContext;
            objectContext.SavingChanges += new EventHandler(objectContext_SavingChanges);

        }

        private void objectContext_SavingChanges(object sender, EventArgs e)
        {
            var objectContext = (ObjectContext)sender;
            var modifiedEntities = objectContext.ObjectStateManager.GetObjectStateEntries(EntityState.Modified);
            modifiedEntities.Select(x => x.Entity).Update();
            var insertEntities = objectContext.ObjectStateManager.GetObjectStateEntries(EntityState.Added);
            insertEntities.Select(x => x.Entity).Insert();
            var deletedEntities = objectContext.ObjectStateManager.GetObjectStateEntries(EntityState.Deleted);
            deletedEntities.Select(x => x.Entity).Delete();
        }
        public DbSet<AbstractPage> AbstractPages { get; set; }
        public DbSet<DomainPage> DomainPages { get; set; }
        public DbSet<LanguagePage> LanguagePages { get; set; }
        public DbSet<ContentPage> ContentPages { get; set; }
        public DbSet<SiteMapPage> SiteMapPages { get; set; }
        public DbSet<ArticleListPage> ArticleListPages { get; set; }
        public DbSet<ArticlePage> ArticlePages { get; set; }
        public DbSet<FormPage> FormPages { get; set; }
        public DbSet<NewsListPage> NewsListPages { get; set; }
        public DbSet<NewsPage> NewsPages { get; set; }

        public DbSet<Error> Errors { get; set; }
        public DbSet<User> Users { get; set; }
        public DbSet<Role> Roles { get; set; }
        public DbSet<Contact> Contacts { get; set; }
        public DbSet<Newsletter> Newsletters { get; set; }
        public DbSet<Settings> SettingsAll { get; set; }
        public DbSet<OutEmail> OutEmails { get; set; }
        public DbSet<TextComponent> TextComponents { get; set; }
        public DbSet<Translation> Translations { get; set; }
        public DbSet<MenuModel> Menus { get; set; }
        public DbSet<Banner> Banners { get; set; }

        #region ShopCatalog

        public DbSet<Order> Orders { get; set; }
        public DbSet<OrderNote> OrderNotes { get; set; }
        public DbSet<OrderItem> OrderItems { get; set; }
        public DbSet<Product> Products { get; set; }
        public DbSet<ProductAttribute> ProductAttributes { get; set; }
        public DbSet<ProductAttributeOption> ProductAttributeOptions { get; set; }
        public DbSet<ProductShop> ProductShopMap { get; set; }
        public DbSet<AttributeType> AttributeTypes { get; set; }
        public DbSet<ShoppingCartItem> ShoppingCartItems { get; set; }
        public DbSet<Category> Categories { get; set; }

        public DbSet<ProductRate> ProductRates { get; set; }
        public DbSet<ProductComment> ProductComments { get; set; }
        public DbSet<SpecificationAttribute> SpecificationAttributes { get; set; }
        public DbSet<SpecificationAttributeOption> SpecificationAttributeOptions { get; set; }
        public DbSet<ProductSpecificationAttributeOption> ProductSpecificationAttributeOptions { get; set; }
        public DbSet<SpecificationAttributeType> SpecificationAttributeTypes { get; set; }
        public DbSet<ProductNote> ProductNotes { get; set; }

        //public DbSet<OrderItemStatus> OrderItemStatuses { get; set; }

        public DbSet<WeekDay> WeekDays { get; set; }
        public DbSet<ProductFavorite> ProductFavorites { get; set; }
        public DbSet<CheckoutData> CheckoutDatas { get; set; }
        public DbSet<Poll> Polls { get; set; }
        public DbSet<PollVariant> PollVariants { get; set; }
        public DbSet<PollAnswer> PollAnswers { get; set; }
        public DbSet<MessageTemplate> MessageTemplates { get; set; }

        public DbSet<Discount> Discounts { get; set; }
        public DbSet<DiscountUsage> DiscountUsages { get; set; }

        public DbSet<UrlRecord> UrlRecords { get; set; }

        public DbSet<ActivityLog> ActivityLogs { get; set; }
        public DbSet<Manufacturer> Manufacturers { get; set; }
        public DbSet<UserCredit> UserCredits { get; set; }
        public DbSet<ProductSkuMap> ProductSkuMaps { get; set; }
        public DbSet<UserAddressSearchActivity> UserAddressSearchActivities { get; set; }
        public DbSet<ProcessError> ProcessErrors { get; set; }
        public DbSet<PlanedTask> PlanedTasks { get; set; }


        public DbSet<Measure> Measures { get; set; }


        public DbSet<ContentUnitMeasure> ContentUnitMeasures { get; set; }
        public DbSet<ContentUnitMeasureMap> ContentUnitMeasureMaps { get; set; }
        #endregion

        #region Shop
        public DbSet<Shop> Shops { get; set; }
        public DbSet<ShopCategory> ShopCategories { get; set; }
        public DbSet<ShopRate> ShopRates { get; set; }
        public DbSet<ShopComment> ShopComments { get; set; }
        public DbSet<ShopType> ShopTypes { get; set; }
        public DbSet<ShopWorkTime> ShopWorkTimes { get; set; }
        public DbSet<ShopShipTime> ShopShipTimes { get; set; }
        public DbSet<ShopCategoryMenu> ShopCategoryMenus { get; set; }
        public DbSet<ShopDeliveryZone> ShopDeliveryZones { get; set; }

        public DbSet<ShopDesign> ShopDesigns { get; set; }
        public DbSet<ShopSetting> ShopSettings { get; set; }
        public DbSet<ShopMessage> ShopMessages { get; set; }

        #endregion

        #region Payments

        #endregion
        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {

            //Set all deimal percision to normal uses (EF bug or )
            #region decimal fix
            foreach (var contextProperty in typeof(Db).GetProperties())
            {
                if (contextProperty.PropertyType.IsGenericType &&
                    contextProperty.PropertyType.GetGenericTypeDefinition() == typeof(DbSet<>))
                {
                    var entityType = contextProperty.PropertyType.GenericTypeArguments[0];

                    foreach (var decimalProperty in entityType.GetProperties().Where(p => p.PropertyType == typeof(decimal)))
                    {
                        var configurePropertyMethod =
                            GetType()
                            .GetMethod("ConfigureProperty", BindingFlags.Static | BindingFlags.NonPublic)
                            .MakeGenericMethod(entityType);
                        configurePropertyMethod.Invoke(null, new object[] { modelBuilder, decimalProperty });
                    }
                }
            }
            #endregion
            //register plugin models
            if (SF.UsePlugins())
            {
                //var entityMethod = typeof(DbModelBuilder).GetMethod("Entity");
                //foreach (var type in RP.GetPlugingsEntityModels())
                //{
                //    entityMethod.MakeGenericMethod(type)
                //        .Invoke(modelBuilder, new object[] { });
                //}
            }
            base.OnModelCreating(modelBuilder);
        }
        #region Double fix
        private static void ConfigureProperty<T>(DbModelBuilder modelBuilder, PropertyInfo propertyInfo)
    where T : class
        {
            var propertyExpression = BuildLambda<T, decimal>(propertyInfo);
            modelBuilder.Entity<T>().Property(propertyExpression).HasPrecision(14, 6);// multi, for coordinates and prices
        }

        private static Expression<Func<T, U>> BuildLambda<T, U>(PropertyInfo property)
        {
            var param = Expression.Parameter(typeof(T), "p");
            MemberExpression memberExpression = Expression.Property(param, property);
            var lambda = Expression.Lambda<Func<T, U>>(memberExpression, param);
            return lambda;
        }
        #endregion
        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
        }
    }
}
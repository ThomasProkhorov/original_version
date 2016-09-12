using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using Uco.Infrastructure.Livecycle;
using Uco.Models;
using Uco.Models.Mongo;
using System.Configuration;
namespace Uco.Infrastructure.Services
{
    // Web.config
    // <add key="UseMongo" value="true"/>
    //<add key="MongoConnection" value="mongodb://localhost:27017"/>
    //<add key="MongoDB" value="UcoDB"/>
    public static class UserActivityService
    {
        private static IMongoDatabase GetMongoDB()
        {
            var client = new MongoClient(ConfigurationManager.AppSettings["MongoConnection"]);
            var database = client.GetDatabase(ConfigurationManager.AppSettings["MongoDB"]);
            return database;
        }

        public async static Task InsertAddToCart(Guid userID, int ProductShopID, decimal Quantity
            , int ShopID, int ProductAttributeOptionID, string PageUrl, string RefererUrl, string IP)
        {
            if (ConfigurationManager.AppSettings["UseMongo"] != "true")
            {
                return;
            }
            var database = GetMongoDB();
            var collection = database.GetCollection<UserAddToCartActivity>("UserAddToCartActivity");
            var activity = new UserAddToCartActivity();
            activity.CreateOn = DateTime.Now;
            activity.PageUrl = PageUrl;
            activity.RefererUrl = RefererUrl;
            activity.ProductShopID = ProductShopID;
            activity.UserID = userID;
            activity.Quantity = Quantity;
            activity.ShopID = ShopID;
            activity.ProductAttributeOptionID = ProductAttributeOptionID;
            activity.UserIDStr = userID.ToString();
            activity.IP = IP;
            await collection.InsertOneAsync(activity);
        }
        public async static Task InsertUserClick(Guid userID,

            string PageUrl, string RefererUrl, string IP)
        {
            if (ConfigurationManager.AppSettings["UseMongo"] != "true")
            {
                return;
            }
            var database = GetMongoDB();
            var collection = database.GetCollection<UserClickActivity>("UserClickActivity");
            var activity = new UserClickActivity();
            activity.CreateOn = DateTime.Now;
            activity.PageUrl = PageUrl;
            activity.RefererUrl = RefererUrl;

            activity.UserID = userID;
            activity.UserIDStr = userID.ToString();
            activity.IP = IP;

            await collection.InsertOneAsync(activity);
        }
        public async static Task InsertShopContact(Guid userID, int ShopID,
            string ContactData,
                       string ContactEmail,
                       string ContactName
                       , string ContactPhone
                       , List<string> DropDownItems,

            string PageUrl, string RefererUrl, string IP)
        {
            if (ConfigurationManager.AppSettings["UseMongo"] != "true")
            {
                return;
            }
            var database = GetMongoDB();
            var collection = database.GetCollection<UserShopContactActivity>("UserShopContactActivity");
            var activity = new UserShopContactActivity();
            activity.CreateOn = DateTime.Now;
            activity.PageUrl = PageUrl;
            activity.RefererUrl = RefererUrl;
            activity.ShopID = ShopID;
            activity.UserID = userID;
            activity.UserIDStr = userID.ToString();
            activity.ContactData = ContactData;
            activity.ContactEmail = ContactEmail;
            activity.ContactName = ContactName;
            activity.ContactPhone = ContactPhone;
            activity.DropDownItems = DropDownItems;
            activity.IP = IP;
            await collection.InsertOneAsync(activity);
        }
        public async static Task InsertShopOpen(Guid userID, int ShopID, string PageUrl, string RefererUrl, string IP)
        {
            if (ConfigurationManager.AppSettings["UseMongo"] != "true")
            {
                return;
            }
            var database = GetMongoDB();
            var collection = database.GetCollection<UserShopOpenActivity>("UserShopOpenActivity");
            var activity = new UserShopOpenActivity();
            activity.CreateOn = DateTime.Now;
            activity.PageUrl = PageUrl;
            activity.RefererUrl = RefererUrl;
            activity.ShopID = ShopID;
            activity.UserID = userID;
            activity.UserIDStr = userID.ToString();
            activity.IP = IP;
            await collection.InsertOneAsync(activity);
        }
        public async static Task InsertProductOpen(Guid userID, int ProductShopID, int ProductID, string PageUrl
            , string RefererUrl, string IP)
        {
            if (ConfigurationManager.AppSettings["UseMongo"] != "true")
            {
                return;
            }
            var database = GetMongoDB();
            var collection = database.GetCollection<UserProductOpenActivity>("UserProductOpenActivity");
            var activity = new UserProductOpenActivity();
            activity.CreateOn = DateTime.Now;
            activity.PageUrl = PageUrl;
            activity.RefererUrl = RefererUrl;
            activity.ProductShopID = ProductShopID;
            activity.UserID = userID;
            activity.ProductID = ProductID;
            activity.UserIDStr = userID.ToString();
            activity.IP = IP;
            await collection.InsertOneAsync(activity);
        }
        public async static Task InsertFavoriteProduct(Guid userID, int ProductShopID, bool removing
            , string PageUrl, string RefererUrl, string IP)
        {
            if (ConfigurationManager.AppSettings["UseMongo"] != "true")
            {
                return;
            }
            var database = GetMongoDB();
            var collection = database.GetCollection<UserAddFavoriteProductActivity>("UserAddFavoriteProductActivity");
            var activity = new UserAddFavoriteProductActivity();
            activity.CreateOn = DateTime.Now;
            activity.PageUrl = PageUrl;
            activity.RefererUrl = RefererUrl;
            activity.ProductShopID = ProductShopID;
            activity.UserID = userID;
            activity.Removing = removing;
            activity.UserIDStr = userID.ToString();
            activity.IP = IP;
            await collection.InsertOneAsync(activity);
        }
        public async static Task InsertProductSearch(Guid userID, string PageUrl, string RefererUrl, string IP

            , int shopID, int page, int limit,
            int? categoryID = null, IList<int> filters = null
           , string keywords = null
            , string productName = null

            )
        {
            if (ConfigurationManager.AppSettings["UseMongo"] != "true")
            {
                return;
            }
            var database = GetMongoDB();
            var collection = database.GetCollection<UserProductSearchActivity>("UserProductSearchActivity");
            var activity = new UserProductSearchActivity();
            activity.CreateOn = DateTime.Now;
            activity.PageUrl = PageUrl;
            activity.RefererUrl = RefererUrl;
            activity.ShopID = shopID;
            activity.UserID = userID;
            activity.UserIDStr = userID.ToString();
            activity.categoryID = categoryID;
            activity.filters = filters != null ? filters.ToList() : null;
            activity.limit = limit;
            activity.page = page;
            if (!string.IsNullOrEmpty(keywords))
            {
                activity.SearchText = keywords;
            }
            if (!string.IsNullOrEmpty(productName))
            {
                if (string.IsNullOrEmpty(activity.SearchText))
                {
                    activity.SearchText = productName;
                }
                else
                {
                    activity.SearchText += " " + productName;
                }
            }
            activity.IP = IP;
            await collection.InsertOneAsync(activity);
        }
        public async static Task InsertUserSearch(string SearchText, int shopID, Guid userID, string PageUrl,
           string RefererUrl, string IP)
        {
            if (ConfigurationManager.AppSettings["UseMongo"] != "true")
            {
                return;
            }
            var database = GetMongoDB();
            var collection = database.GetCollection<UserSearchNotFoundActivity>("UserSearchNotFoundActivity");
            var activity = new UserSearchNotFoundActivity { SearchText = SearchText };
            activity.CreateOn = DateTime.Now;
            activity.PageUrl = PageUrl;
            activity.RefererUrl = RefererUrl;
            activity.ShopID = shopID;
            activity.UserID = userID;
            activity.UserIDStr = userID.ToString();
            activity.IP = IP;
            await collection.InsertOneAsync(activity);
        }
        public async static Task InsertSearchNotFound(string SearchText, int shopID, Guid userID, string PageUrl,
            string RefererUrl, string IP)
        {
            if (ConfigurationManager.AppSettings["UseMongo"] != "true")
            {
                return;
            }
            var database = GetMongoDB();
            var collection = database.GetCollection<UserSearchNotFoundActivity>("UserSearchNotFoundActivity");
            var activity = new UserSearchNotFoundActivity { SearchText = SearchText };
            activity.CreateOn = DateTime.Now;
            activity.PageUrl = PageUrl;
            activity.RefererUrl = RefererUrl;
            activity.ShopID = shopID;
            activity.UserID = userID;
            activity.UserIDStr = userID.ToString();
            activity.IP = IP;
            await collection.InsertOneAsync(activity);
        }
        public async static Task InsertOrder(Order order, string PageUrl, string RefererUrl, string IP)
        {
            if (ConfigurationManager.AppSettings["UseMongo"] != "true")
            {
                return;
            }
            var database = GetMongoDB();
            var collection = database.GetCollection<UserOrderActivity>("UserOrderActivity");
            var activity = new UserOrderActivity();
            activity.CreateOn = DateTime.Now;
            activity.PageUrl = PageUrl;
            activity.RefererUrl = RefererUrl;
            activity.ShopID = order.ShopID;
            activity.UserID = order.UserID;
            activity.UserIDStr = order.UserID.ToString();
            activity.Address = order.Address;
            activity.OrderID = order.ID;
            activity.Total = order.Total;
            var shop = LS.Get<Shop>().FirstOrDefault(x => x.ID == order.ShopID);
            if (shop != null)
            {
                if (shop.ShopTypeIDs != null)
                {
                    var types = LS.Get<ShopType>();
                    foreach (var type in types)
                    {
                        if (shop.ShopTypeIDs.Contains("," + type.ID.ToString() + ",")
                            ||
                            shop.ShopTypeIDs.StartsWith(type.ID.ToString() + ",")
                            ||
                            shop.ShopTypeIDs.EndsWith("," + type.ID.ToString())
                            )
                        {
                            if (string.IsNullOrEmpty(activity.ShopType))
                            {
                                activity.ShopType = type.Name;
                            }
                            else
                            {
                                activity.ShopType += ", " + type.Name;
                            }
                        }
                    }
                }
            }
            activity.IP = IP;
            await collection.InsertOneAsync(activity);
        }

    }
}
using MongoDB.Bson;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Uco.Models.Mongo
{

    public class UserProductSearchActivity
    {
        public ObjectId Id { get; set; }
        public Guid UserID { get; set; }
        public string UserIDStr { get; set; }
        public int ShopID { get; set; }
        public string SearchText { get; set; }

        public int page { get; set; }
        public int limit { get; set; }
        public int? categoryID { get; set; }
        public List<int> filters { get; set; }

        public DateTime CreateOn { get; set; }
        public string PageUrl { get; set; }
        public string RefererUrl { get; set; }
        public string IP { get; set; }
    }
    public class UserOrderActivity
    {
        public ObjectId Id { get; set; }
        public Guid UserID { get; set; }
        public string UserIDStr { get; set; }
        public int ShopID { get; set; }
        public int OrderID { get; set; }
        public string Address { get; set; }
        public decimal Total { get; set; }
        public string ShopType { get; set; }
        public DateTime CreateOn { get; set; }
        public string PageUrl { get; set; }
        public string RefererUrl { get; set; }
        public string IP { get; set; }
    }
    public class UserSearchNotFoundActivity
    {
        public ObjectId Id { get; set; }
        public Guid UserID { get; set; }
        public string UserIDStr { get; set; }
        public int ShopID { get; set; }
        public string SearchText { get; set; }
        public DateTime CreateOn { get; set; }
        public string PageUrl { get; set; }
        public string RefererUrl { get; set; }
        public string IP { get; set; }
    }
    public class UserClickActivity
    {
        public ObjectId Id { get; set; }
        public Guid UserID { get; set; }
        public string UserIDStr { get; set; }

        public DateTime CreateOn { get; set; }

        public string PageUrl { get; set; }
        public string RefererUrl { get; set; }
        public string IP { get; set; }
    }
    public class UserShopContactActivity
    {
        public ObjectId Id { get; set; }
        public Guid UserID { get; set; }
        public string UserIDStr { get; set; }
        public int ShopID { get; set; }
        public DateTime CreateOn { get; set; }
        public string ContactData { get; set; }
        public string ContactEmail { get; set; }
        public string ContactName { get; set; }
        public string ContactPhone { get; set; }
        public List<string> DropDownItems { get; set; }
        public string PageUrl { get; set; }
        public string RefererUrl { get; set; }
        public string IP { get; set; }
    }
    public class UserShopOpenActivity
    {
        public ObjectId Id { get; set; }
        public Guid UserID { get; set; }
        public string UserIDStr { get; set; }
        public int ShopID { get; set; }
        public DateTime CreateOn { get; set; }
        public string PageUrl { get; set; }
        public string RefererUrl { get; set; }
        public string IP { get; set; }
    }
    public class UserProductOpenActivity
    {
        public ObjectId Id { get; set; }
        public Guid UserID { get; set; }
        public string UserIDStr { get; set; }
        public int ProductShopID { get; set; }
        public int ProductID { get; set; }
        public DateTime CreateOn { get; set; }
        public string PageUrl { get; set; }
        public string RefererUrl { get; set; }
        public string IP { get; set; }
    }
    public class UserAddFavoriteProductActivity
    {
        public ObjectId Id { get; set; }
        public Guid UserID { get; set; }
        public string UserIDStr { get; set; }
        public int ProductShopID { get; set; }
        public bool Removing { get; set; }
        public DateTime CreateOn { get; set; }
        public string PageUrl { get; set; }
        public string RefererUrl { get; set; }
        public string IP { get; set; }
    }
    public class UserAddToCartActivity
    {
        public ObjectId Id { get; set; }
        public Guid UserID { get; set; }
        public string UserIDStr { get; set; }
        public int ProductShopID { get; set; }
        public decimal Quantity { get; set; }
        public int ShopID { get; set; }
        public int ProductAttributeOptionID { get; set; }
        public DateTime CreateOn { get; set; }
        public string PageUrl { get; set; }
        public string RefererUrl { get; set; }
        public string IP { get; set; }
    }
}
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Uco.Infrastructure;

namespace Uco.Models.Overview
{
    public class CheckoutModel
    {
        [Required(ErrorMessage = "Errors.Model.CheckoutModel.Address")]
        public string Address { get; set; }
        [Required(ErrorMessage = "Errors.Model.CheckoutModel.FullName")]
        public string FullName { get; set; }
        [Required(ErrorMessage = "Errors.Model.CheckoutModel.Phone")]
        [UcoPhoneAttribute(ErrorMessageResourceName = "PhoneNotValid", ErrorMessageResourceType = typeof(Uco.Models.Resources.SystemModels))]
        public string Phone { get; set; }
        public string CompanyName { get; set; }
        public bool ShippOn { get; set; }
        public DateTime ShipTime { get; set; }
        public bool RegularOrder { get; set; }
        public RegularInterval RegularInterval { get; set; }
        public string Email { get; set; }
        public int ShopID { get; set; }
        public PaymentMethod PaymentMethod { get; set; }
        public ShippingMethod ShippingMethod { get; set; }
        public string Note { get; set; }
        public string CouponCode { get; set; }
        //  public string FirstName { get; set; }
        //  public string LastName { get; set; }

        public ShoppingCartOverviewModel Cart { get; set; }

    }
    public class DiscountUsageModel
    {
        public string Address { get; set; }
        public string FullName { get; set; }
        public string Phone { get; set; }
        public string Email { get; set; }

    }
    public class ShoppingCartOverviewModel
    {
        public ShoppingCartOverviewModel()
        {
            Items = new List<ShoppingCartItemModel>();
            NotAvaliableItems = new List<ShoppingCartItemModel>();
            ShipTimes = new List<ShipTimeModel>();
            DiscountIDs = new List<int>();
        }
        public bool CanEditCart = true;
        public bool IsLogined = false;
        public bool IsShipEnabled { get; set; }
        public bool InStorePickUpEnabled { get; set; }

        public decimal SubTotal { get; set; }
        public string SubTotalStr { get; set; }

        public decimal ShippingCost { get; set; }
        public decimal FreeShipFrom { get; set; }
        public decimal ShopShipCost { get; set; }
        public string ShippingCostStr { get; set; }

        public decimal Fee { get; set; }
        public decimal TotalCredits { get; set; }
        public string FeeStr { get; set; }

        public decimal TotalWithoutShip { get; set; }
        public string TotalWithoutShipStr { get; set; }
        public int ShopID { get; set; }
        public Shop Shop { get; set; }
        public decimal Total { get; set; }
        public string TotalStr { get; set; }
        public decimal TotalDiscount { get; set; }
        public string TotalDiscountStr { get; set; }
        public string DiscountDescription { get; set; }
        public string DiscountByCouponeCodeText { get; set; }
        public bool IsLessMemberFee { get; set; }
        public IList<int> DiscountIDs { get; set; }
        public int Count { get; set; }
        public IList<ShoppingCartItemModel> Items { get; set; }
        public IList<ShoppingCartItemModel> NotAvaliableItems { get; set; }
        public IList<ShipTimeModel> ShipTimes { get; set; }
        public IList<ShipTimeModel> WorkTimes { get; set; }
        public IList<ProductOverviewModel> FeaturedProducts { get; set; }
    }
    public class ShipTimeModel
    {
        public DayOfWeek Day { get; set; }
        public string DayStr { get; set; }
        public int TimeFromInt { get; set; }
        public string TimeFromeStr { get; set; }
        public int TimeToInt { get; set; }
        public string TimeToStr { get; set; }

        public bool IsSpecial { get; set; }
        public DateTime Date { get; set; }
        public string DateStr { get; set; }

    }
    public class ShoppingCartItemModel
    {
        public ShoppingCartItemModel()
        {
            Attributes = new List<ProductAttributeOptionModel>();
            DiscountIDs = new List<int>();
            Errors = new List<string>();
            IsValid = true;
        }
        public int ID { get; set; }
        // public Guid UserID { get; set; }
        public string Comments { get; set; }
        public string Name { get; set; }
        public string SKU { get; set; }
        public string Manufacturer { get; set; }

        public string Image { get; set; }
        public int ShopID { get; set; }
        public int ProductID { get; set; }
        public int ProductManufacturerID { get; set; }
        public int CategoryID { get; set; }
        public int ProductShopID { get; set; }
        public string MeasureUnit { get; set; }
        public bool SoldByWeight { get; set; }
        public string Capacity { get; set; }
        public decimal? MeasureUnitStep { get; set; }
        public decimal QuantityResource { get; set; }
        public ProductQuantityType QuantityType { get; set; }
        public decimal Price { get; set; }
        public decimal? PriceByUnit { get; set; }
        public decimal TotalDiscountAmount { get; set; }
        public string DiscountDescription { get; set; }
        public IList<int> DiscountIDs { get; set; }
        public string PriceStr { get; set; }
        public decimal UnitPrice { get; set; }
        public string UnitPriceStr { get; set; }
        public int ProductAttributeOptionID { get; set; }
        public string AttributeDescription { get; set; }

        public bool IsNotAvaliable { get; set; }
        public bool SelectedAttributeNotAvaliable { get; set; }
        public bool IsHaveNotQuantity { get; set; }
        public decimal Quantity { get; set; }
        public QuantityType QuantityPriceType { get; set; }
        public bool IsValid { get; set; }
        public List<string> Errors { get; set; }
        public IList<ProductAttributeOptionModel> Attributes { get; set; }

        public virtual string Shop { get; set; }
    }
    public class ProductAttributeOptionModel
    {
        public int ID { get; set; }

        public int ProductShopID { get; set; }

        public int ProductAttributeID { get; set; }

        public virtual string ProductAttribute { get; set; }

        public string Name { get; set; }

        public string OverridenSku { get; set; }

        public decimal? OverridenPrice { get; set; }
        public string OverridenPriceStr { get; set; }

        public decimal Quantity { get; set; }
    }
    public class AddToCartModel
    {
        public ShoppingCartItem item { get; set; }
        public List<string> errors { get; set; }
    }
}
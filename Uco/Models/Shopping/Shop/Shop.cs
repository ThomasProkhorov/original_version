using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Mvc;
using Uco.Infrastructure;
using Uco.Infrastructure.Repositories;
using Uco.Models.Overview;


namespace Uco.Models
{
    [ModelGeneral(Role = "Admin", AjaxEdit = false, Edit = true, Create = true, CreateAjax = false)]
    [ModelGeneral(Role = "Member", CanBack = false, Show = false, Edit = false, AjaxEdit = false, Create = false, CreateAjax = false, Delete = false)]
    public partial class Shop
    {
        public Shop()
        {
            Active = true;
            InStorePickUpEnabled = true;
            Kosher = true;
            IsShipEnabled = true;
            CreateTime = DateTime.Now;
            RadiusLatitude = 32.0677778m;
            RadiusLongitude = 34.7647222m;

            var settings = RP.GetAdminCurrentSettingsRepository();
            if (settings != null)
            {
                this.CreditGuardMid = settings.CreditGuardMid;
                this.CreditGuardPass = settings.CreditGuardPass;
                this.CreditGuardTerminal = settings.CreditGuardTerminal;
                this.CreditGuardUrl = settings.CreditGuardUrl;
                this.CreditGuardUser = settings.CreditGuardUser;
            }
            if (PaymentMethodIDs == null)
            {
                PaymentMethodIDs = ",1,2,3,4,5,";
            }
           
        }
        //[HiddenInput(DisplayValue = false)]
        [Key]
        [Model(Edit = false, Filter = true, Sort = true, Show = false)]
        [Display(Name = "Main", Order = 1)]
        public int ID { get; set; }

        [Model(Edit = false, AjaxEdit = false, Filter = true, Sort = true, Show = false)]
        [Display(Name = "UserID")]
        public Guid UserID { get; set; }



        [NotMapped]
        [Model(Show = false, Edit = false)]
        [Model(Role = "Admin", Show = false, Edit = true)]
        [Display(Name = "User", Order = 100, Prompt = "TabMain")]
        public User User { get; set; }

        [Model(Edit = false, ShowInEdit = true, Filter = true, Sort = true)]
        [Model(Role = "Admin", Filter = true, Sort = true)]
        [Display(Name = "Name", Order = 100, ResourceType = typeof(Uco.Models.Resources.Models), Prompt = "TabMain")]
        [Required(ErrorMessage = "NameRequired")]

        public string Name { get; set; }


        [Model(Show = true, Edit = false)]
        [Model(Role = "Admin", Show = true, Edit = true)]
        [Display(Name = "SeoUrl", Order = 100, Prompt = "TabMain")]
        [UIHint("SeoUrl")]
        [SeoUrl()]
        public string SeoUrl { get; set; }

        [Display(Name = "Theme", Order = 100, Prompt = "TabMain")]
        [UIHint("Themes")]
        [Model(Show = false, Edit = true)]
        public virtual string Theme { get; set; }

        [Model(Edit = false, ShowInEdit = true, Filter = true, Sort = true)]
        [Model(Role = "Admin", Filter = true, Sort = true)]
        [Required(ErrorMessage = "EmailRequired")]
        //[EmailAddress(ErrorMessage = "Invalid Email Address")]
        [Display(Name = "Email", Order = 100, ResourceType = typeof(Uco.Models.Resources.Models), Prompt = "TabMain")]
        public string Email { get; set; }

        [Model(Edit = false, ShowInEdit = true)]
        [Model(Role = "Admin", Show = true, Edit = true)]
        [Required(ErrorMessage = "PhoneRequired")]
        [Display(Name = "Phone", Order = 100, ResourceType = typeof(Uco.Models.Resources.Models), Prompt = "TabMain")]
        public string Phone { get; set; }


        [Model(Edit = false, ShowInEdit = true)]
        [Model(Role = "Admin", Show = true, Edit = true)]
        [Display(Name = "Phone2", Order = 100, Prompt = "TabMain")]
        public string Phone2 { get; set; }

        [UIHint("DateTime")]
        [Model(Filter = true, Show = false, Sort = true, Edit = false, AjaxEdit = false)]
        [Display(Name = "CreateTime", Order = 100, ResourceType = typeof(Uco.Models.Resources.Models), Prompt = "TabMain")]
        [Model(Role = "Member", Edit = false, AjaxEdit = false, Show = false)]
        public DateTime CreateTime { get; set; }

        [Model(Show = false, Edit = false)]
        [Model(Role = "Admin", Show = true, Edit = true)]
        [Display(Name = "DisplayOrder", Order = 100, ResourceType = typeof(Uco.Models.Resources.Models), Prompt = "TabMain")]
        public int DisplayOrder { get; set; }



        [Model(Filter = true, ShowInEdit = true, Edit = false)]
        [Model(Role = "Admin", Show = false, Edit = false)]
        [Display(Name = "Address", Order = 100, ResourceType = typeof(Uco.Models.Resources.Models), Prompt = "TabMain")]
        public string Address { get; set; }



        [UIHint("AddressMapWthCoord")]
        [Display(Name = "AddressMap", Order = 100, ResourceType = typeof(Uco.Models.Resources.Models), Prompt = "TabMain")]
        [Model(Show = false, Edit = false)]
        [Model(Role = "Admin", Show = false, Edit = true)]
        public string AddressMap { get; set; }

        // public string DeliveryPointAddress { get; set; }

        [Model(ShowInEdit = true, Edit = false, Filter = true, Sort = true)]
        [Model(Role = "Admin", Filter = true, Sort = true)]
        [Display(Name = "Kosher", Order = 100, ResourceType = typeof(Uco.Models.Resources.Models), Prompt = "TabMain")]
        public bool Kosher { get; set; }

        [Display(Name = "Youtube", Order = 100, ResourceType = typeof(Uco.Models.Resources.Models), Prompt = "TabMain")]
        [Model(Show = false, ShowInEdit = true, Edit = false)]
        [Model(Role = "Admin", Show = false)]
        public string Youtube { get; set; }


        [Model(Show = false)]
        [Model(Role = "Member", Edit = false, AjaxEdit = false, Show = false)]
        [Display(Name = "Percent Fee", Order = 100, Prompt = "TabMain")]
        public decimal PercentFee { get; set; }

        [Model(Show = false)]
        [Model(Role = "Member", Edit = false, AjaxEdit = false, Show = false)]
        [Display(Name = "Mounthly Fee", Order = 100, Prompt = "TabMain")]
        [UIHint("Price")]
        public decimal MounthlyFee { get; set; }

        [Model(Show = false)]
        [Model(Role = "Member", Edit = false, AjaxEdit = false, Show = false)]
        [Display(Name = "Special percent fee", Order = 100, Prompt = "TabMain")]
        public decimal SpecialPercentFee { get; set; }
        //ALTER TABLE [dbo].[Shops]
        //ADD [SpecialPercentFee] DECIMAL (14, 6) DEFAULT 0 NOT NULL;


        // [Model(Show = false)]
        // [Model(Role = "Member", Edit = false, AjaxEdit = false, Show = false)]
        //[Display(Name = "Special mounthly Fee", Order = 100, Prompt = "TabMain")]
        //[UIHint("Price")]
        // public decimal SpecialMounthlyFee { get; set; }
        //ALTER TABLE [dbo].[Shops]
        //ADD [SpecialMounthlyFee] DECIMAL (14, 6) DEFAULT 0 NOT NULL;

        [UIHint("Image")]
        [Model(AjaxEdit = false, Show = false)]
        [Model(Role = "Member", Edit = false, AjaxEdit = false, Show = false, ShowInEdit = true)]
        [Display(Name = "Image", Order = 100, ResourceType = typeof(Uco.Models.Resources.Models), Prompt = "TabMain")]
        public string Image { get; set; }


        [UIHint("Image")]
        [Model(AjaxEdit = false, Show = false)]
        [Model(Role = "Member", Edit = false, AjaxEdit = false, Show = false, ShowInEdit = true)]
        [Display(Name = "Logo", Order = 100, Prompt = "TabMain")]
        public string Logo { get; set; }


        [UIHint("Image")]
        [Model(AjaxEdit = false, Show = false)]
        [Model(Role = "Member", Edit = true, AjaxEdit = false, Show = false, ShowInEdit = true)]
        [Display(Name = "FavIcon", Order = 100, Prompt = "TabMain")]
        public string FavIcon { get; set; }

        [DataType(DataType.MultilineText)]
        [AllowHtml]
        [Model(Role = "Admin", Show = false, Filter = true, Sort = true)]
        [Model(Show = false, ShowInEdit = true, Edit = false)]
        [Display(Name = "ShortDescription", Order = 100, ResourceType = typeof(Uco.Models.Resources.Models), Prompt = "TabMain")]
        public string ShortDescription { get; set; }



        [UIHint("Html")]
        [AllowHtml]
        [Model(Role = "Admin", Show = false, AjaxEdit = false)]
        [Model(Show = false, ShowInEdit = true, Edit = false)]
        [Display(Name = "FullDescription", Order = 100, ResourceType = typeof(Uco.Models.Resources.Models), Prompt = "TabMain")]
        public string FullDescription { get; set; }

        [Display(Name = "SeoDescription", Order = 340,  Prompt = "TabSeo")]
        [DataType(DataType.MultilineText)]
        [MaxLength(4000)]
        [Model(Role = "Admin", Show = false, AjaxEdit = false)]
        [Model(Show = false, ShowInEdit = true, Edit = false)]
        public string SeoDescription { get; set; }

        [Display(Name = "SeoKeywords", Order = 340, Prompt = "TabSeo")]
        [DataType(DataType.MultilineText)]
        [MaxLength(4000)]
        [Model(Role = "Admin", Show = false, AjaxEdit = false)]
        [Model(Show = false, ShowInEdit = true, Edit = false)]
        public string SeoKeywords { get; set; }


        [Model(Show = false, Edit = false)]
        [Display(Name = "Longitude", Order = 100, ResourceType = typeof(Uco.Models.Resources.Models), Prompt = "TabMain")]
        public decimal Longitude { get; set; }

        [Model(Show = false, Edit = false)]
        [Display(Name = "Latitude", Order = 100, ResourceType = typeof(Uco.Models.Resources.Models), Prompt = "TabMain")]
        public decimal Latitude { get; set; }



        //[Model(Show = false)]
        //[Display(Prompt = "Hours", Order = 50)]
        // public int TraidingDayFrom { get; set; }

        //[Model(Show = false)]
        //[Display(Prompt = "Hours")]
        // public int TraidingDayTo {get;set;}

        //[Model(Show = false)]
        //[Display(Prompt = "Hours")]
        // public int TraidingHourFrom { get; set; }

        //[Model(Show = false)]
        //[Display(Prompt = "Hours")]
        // public int TraidingHourTo { get; set; }


        //[Model(Show = false)]
        //[Display(Prompt = "Shiping", Order = 100)]
        //public int ShipHourFrom { get; set; }

        //[Model(Show = false)]
        //[Display(Prompt = "Shiping")]
        //public int ShipHourTo { get; set; }

        [Model(Show = false)]
        [Model(Role = "Member", Show = false, Edit = false)]
        [Display(Name = "InStorePickUpEnabled", Order = 100, Prompt = "TabShiping")]
        public bool InStorePickUpEnabled { get; set; }





        [Model(Show = false, Edit = false)]
        [Display(Name = "RadiusLongitude", Order = 100, ResourceType = typeof(Uco.Models.Resources.Models), Prompt = "TabMain")]
        public decimal RadiusLongitude { get; set; }

        [Model(Show = false, Edit = false)]
        [Display(Name = "RadiusLatitude", Order = 100, ResourceType = typeof(Uco.Models.Resources.Models), Prompt = "TabMain")]
        public decimal RadiusLatitude { get; set; }


        [Model(Show = false)]
        [Model(Role = "Member", Show = false, Edit = false)]
        [Display(Name = "ShipCost", Order = 100, ResourceType = typeof(Uco.Models.Resources.Models), Prompt = "TabShiping")]
        public decimal ShipCost { get; set; }




        // [Model(Show = false)]


        [Model(Show = false)]
        [Model(Role = "Member", Show = false, Edit = false)]
        [Display(Name = "FreeShipFrom", Order = 100, ResourceType = typeof(Uco.Models.Resources.Models), Prompt = "TabShiping")]
        public decimal FreeShipFrom { get; set; }

        [Model(Show = false)]
        [Model(Role = "Member", Show = false, Edit = false)]
        [Display(Name = "DeliveryTime", Order = 100, ResourceType = typeof(Uco.Models.Resources.Models), Prompt = "TabShiping")]
        public int DeliveryTime { get; set; }

        [Model(Show = false)]
        [Model(Role = "Member", Show = false, Edit = false)]
        [Display(Name = "DeliveryManualDescription", Order = 100, ResourceType = typeof(Uco.Models.Resources.Models), Prompt = "TabShiping")]
        [DataType(DataType.MultilineText)]
        public string DeliveryManualDescription { get; set; }

        [Model(Show = false)]
        [Model(Role = "Member", Show = false, Edit = true)]
        [Display(Name = "ClubCardTypes", Order = 100,  Prompt = "TabClubCard")]
        [DataType(DataType.MultilineText)]
        public string ClubCardTypes { get; set; }

        [UIHint("MapRadius")]
        [Model(Show = false)]
        [Model(Role = "Member", Show = false, Edit = false)]
        [Display(Name = "ShipRadius", Order = 100, ResourceType = typeof(Uco.Models.Resources.Models), Prompt = "TabShiping")]
        public decimal ShipRadius { get; set; }


        //Grid
        [NotMapped]
        //[Display(Name = "Products", Order = 270, Prompt = "Products")]
        [Model(Edit = false, AjaxEdit = false, Show = false)]
        public List<ProductShop> Products { get; set; }

        [NotMapped]
        // [Display(Name = "Orders", Order = 270, Prompt = "Orders")]
        [Model(Edit = false, AjaxEdit = false, Show = false)]
        public List<Order> Orders { get; set; }

        // [NotMapped]
        // [Display(Name = "Categories", Order = 300, ResourceType = typeof(Uco.Models.Resources.Models), Prompt = "TabCategories")]
        // public List<ShopCategory> ShopCategories { get; set; }

        [Model(Show = true, Edit = true)]
        [Display(Name = "Active", Order = 900, Prompt = "TabShopWorkTimes")]
        public bool Active { get; set; }

         [NotMapped]
         [Display(Name = "ShopDeliveryZones", Order = 920, Prompt = "ShopDeliveryZones")]
        public List<ShopDeliveryZone> ShopDeliveryZones { get; set; }
        
         [NotMapped]
         [Display(Name = "ShopMessages", Order = 920, Prompt = "ShopMessages")]
         public List<ShopMessage> ShopMessages { get; set; }
        

        [NotMapped]
        [Display(Name = "ShopWorkTimes", Order = 920, ResourceType = typeof(Uco.Models.Resources.Models), Prompt = "TabShopWorkTimes")]
        public List<ShopWorkTime> ShopWorkTimes { get; set; }

        [NotMapped]
        [Display(Name = "ShopWorkSpecialTimes", Order = 930, ResourceType = typeof(Uco.Models.Resources.Models), Prompt = "TabShopWorkSpecialTimes")]
        public List<ShopWorkTime> ShopWorkSpecialTimes { get; set; }

        [Display(Name = "IsShipEnabled", Order = 940, Prompt = "TabShopShipTimes")]
        [Model(Show = false)]
        public bool IsShipEnabled { get; set; }

        [NotMapped]
        [Display(Name = "ShopShipTimes", Order = 950, ResourceType = typeof(Uco.Models.Resources.Models), Prompt = "TabShopShipTimes")]
        public List<ShopShipTime> ShopShipTimes { get; set; }

        [NotMapped]
        [Display(Name = "ShopShipSpecialTimes", Order = 960, ResourceType = typeof(Uco.Models.Resources.Models), Prompt = "TabShopShipSpecialTimes")]
        public List<ShopShipTime> ShopShipSpecialTimes { get; set; }

        //temporary hidden
        //  [NotMapped]
        // [Display(Name = "Shopping cart", Order = 270, Prompt = "Shopping cart")]
        // [Model(Edit = false, AjaxEdit = false,Show=false)]
        //public List<ShoppingCartItem> ShoppingCartItems { get; set; }


        //Rating Functional
        [Display(ResourceType = typeof(Uco.Models.Resources.Models), Prompt = "TabMain")]
        [Model(Edit = false, AjaxEdit = false, Show = false)]
        public decimal Rate { get; set; }

        [Display(ResourceType = typeof(Uco.Models.Resources.Models), Prompt = "TabMain")]
        [Model(Edit = false, AjaxEdit = false, Show = false)]
        public int RateCount { get; set; }


        [Model(Role = "Admin", Show = false, Edit = true)]
        [Model(Edit = false, AjaxEdit = false, Show = false)]
        [Display(ResourceType = typeof(Uco.Models.Resources.Models), Prompt = "TabPayment")]
        public bool IsToShopOwnerCredit { get; set; }

        [Model(Role = "Admin", Show = false, Edit = true)]
        [Model(Edit = false, AjaxEdit = false, Show = false)]
        [Display(ResourceType = typeof(Uco.Models.Resources.Models), Prompt = "TabPayment")]
        public string CreditGuardUser { get; set; }

        [Model(Role = "Admin", Show = false, Edit = true)]
        [Display(ResourceType = typeof(Uco.Models.Resources.Models), Prompt = "TabPayment")]
        [Model(Edit = false, AjaxEdit = false, Show = false)]
        public string CreditGuardPass { get; set; }

        [Model(Edit = false, AjaxEdit = false, Show = false)]
        [Model(Role = "Admin", Show = false, Edit = true)]
        [Display(ResourceType = typeof(Uco.Models.Resources.Models), Prompt = "TabPayment")]
        public string CreditGuardTerminal { get; set; }

        [Model(Edit = false, AjaxEdit = false, Show = false)]
        [Model(Role = "Admin", Show = false, Edit = true)]
        [Display(ResourceType = typeof(Uco.Models.Resources.Models), Prompt = "TabPayment")]
        public string CreditGuardMid { get; set; }

        [Model(Edit = false, AjaxEdit = false, Show = false)]
        [Model(Role = "Admin", Show = false, Edit = true)]
        [Display(ResourceType = typeof(Uco.Models.Resources.Models), Prompt = "TabPayment")]
        public string CreditGuardUrl { get; set; }

        [NotMapped]
        //[Display(Name = "Rates", Order = 270, Prompt = "Rates")]
        [Model(Edit = false, AjaxEdit = false, Show = false)]
        public List<ShopRate> ShopRate { get; set; }

        [NotMapped]
        //[Display(Name = "Comments", Order = 270, Prompt = "Comments")]
        [Model(Edit = false, AjaxEdit = false, Show = false)]
        public List<ShopComment> ShopComment { get; set; }


        [NotMapped]
        [Model(Edit = false, AjaxEdit = false, Show = false)]
        public List<ShopCommentModel> ShopCommentModels { get; set; }

        //[Display(Name = "ShopType")]
        //[HiddenInput(DisplayValue = false)]
        //public int ShopTypeID { get; set; }


        //[NotMapped]
        //[Model(Show = false, Edit = false)]
        //public ShopType ShopType { get; set; }


        [Display(Name = "ShopTypes")]
        [HiddenInput(DisplayValue = false), StringLength(450, ErrorMessage = "LengthError"), Index()]
        public string ShopTypeIDs { get; set; }

        [NotMapped]
        [Model(Show = true, Edit = true)]
        [Model(Role = "Member", Show = false, Edit = false)]
        [Display(Name = "ShopType", Order = 400, ResourceType = typeof(Uco.Models.Resources.Models), Prompt = "TabShopType")]
        public List<ShopType> ShopTypes { get; set; }


        [UIHint("PaymentMethodsSelect")]
        [Display(Name = "PaymentMethods", Prompt = "TabPayment")]
        [StringLength(450, ErrorMessage = "LengthError")]
        public string PaymentMethodIDs { get; set; }



        [NotMapped]
        [Model(Edit = false, AjaxEdit = false, Show = false)]
        public IList<ShipTimeModel> WorkTimes { get; set; }

        [NotMapped]
        [Model(Edit = false, AjaxEdit = false, Show = false)]
        public IList<ShipTimeModel> ShipTimes { get; set; }

    }

}
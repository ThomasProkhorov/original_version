using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Mvc;
using Uco.Infrastructure;
using Uco.Infrastructure.Livecycle;
using Uco.Models.Overview;


namespace Uco.Models
{
    [ModelGeneral(Role = "Admin", AjaxEdit = false, Edit = true, Create = true, CreateAjax = false)]
    public partial class CheckoutData
    {
        public CheckoutData()
        {
            IsRegistered = LS.isLogined();
            LastAction = DateTime.UtcNow;
        }
        //[HiddenInput(DisplayValue = false)]
        [Key]
        [Model(Edit = false, Filter = true, Sort = true, Show = false)]
        public int ID { get; set; }

        [Model(Edit = false, AjaxEdit = false, Filter = true, Sort = true, Show = false)]
        public Guid UserID { get; set; }

        public bool IsApproved { get; set; }
        public string Address { get; set; }
        public string FullName { get; set; }
        public string Phone { get; set; }
        public string CompanyName { get; set; }
        public bool ShippOn { get; set; }
       // public bool ShowShipTime { get; set; }
        public DateTime ShipTime { get; set; }
        public bool RegularOrder { get; set; }
        public RegularInterval RegularInterval { get; set; }
        public string Email { get; set; }
        public PaymentMethod PaymentMethod { get; set; }
        public ShippingMethod ShippingMethod { get; set; }
        public string Note { get; set; }
        public string CouponCode { get; set; }
        public string smsCode { get; set; }
        public bool smsCodeUsed { get; set; }

        public string LastSeenProducts { get; set; }

        public bool IsRegistered { get; set; }
        public DateTime LastAction { get; set; }


        
    }
    public enum PaymentMethod
    {
        Credit = 1,
        Cash = 2,
        ByPhone = 3,
        CreditShopOwner =4,
        ClubCard = 5
    }
    public enum ShippingMethod
    {
        Courier = 1,
        Manual = 2,
    }
    public enum RegularInterval
    {
        NotRegular = 1,
        Daily = 2,
        Weekly = 3,
        EveryTwoWeeks = 4,
        Mounthly = 5,
        Quartly = 6,
    }
   
}
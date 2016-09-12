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
using Uco.Infrastructure.Repositories;

namespace Uco.Models
{
    [ModelGeneral(AjaxEdit = false, Edit = true, Create = false, CreateAjax = false)]
    [ModelGeneralAttribute(
       Role = "Member", Acl = true, Edit = true,
       AjaxEdit = false, Create = false, CreateAjax = false,
       Delete = false, Show = true, DependedShow = true,
       EditText = "Details")]

    public partial class Order
    {

        //  public int Status { get; set; }

        [HiddenInput(DisplayValue = false)]
        [Key]
        [Display(Prompt = "Main", Order = 1)]
        public int ID { get; set; }

        [Model(Edit = false,ShowInEdit=true, AjaxEdit = false, Filter = true, Sort = false, Show = false)]
        [Display(Prompt = "Main")]
        public Guid UserID { get; set; }

        // ShopID - need ? 
        // Yes, of course:)
        [Display(Prompt = "Main")]
        [Model(Show = false, Edit = false)]
        [Index]
        public int ShopID { get; set; }

        [NotMapped]
        [Display(Prompt = "Main")]
        [Model(Role="Admin",Show = true, Edit = false)]
        [Model(Show = false, Edit = false)]
        public Shop Shop { get; set; }

        [Display(Prompt = "Main")]
        [Model(Show = false, Edit = false)]
        public string Address { get; set; }
        [Display(Prompt = "Main")]
        [Model(Show = false, Edit = false)]
        public string FullName { get; set; }
        [Display(Prompt = "Main")]
        [Model(Show = false, Edit = false)]
        public string Phone { get; set; }
        [Display(Prompt = "Main")]
        [Model(Show = false, Edit = false)]
        public string CompanyName { get; set; }
        [Display(Prompt = "Main")]
        [Model(Show = false, Edit = false)]
        public bool ShippOn { get; set; }
        [Display(Prompt = "Main")]
        [Model(Show = false, Edit = false)]
        public DateTime ShipTime { get; set; }



        [Model(Show = false, Edit = false, AjaxEdit = false)]
        [NotMapped]
        public string ShipTimeStr
        {
            get
            {
                return ShipTime.ToString("dd.MM.yyyy HH:mm");
            }
        }


        [Display(Prompt = "Main")]
        [Model(Show = false, Edit = false)]
        public bool RegularOrder { get; set; }

        [Display(Prompt = "Main")]
        [Model(Show = false, Edit = false)]
        public RegularInterval RegularInterval { get; set; }

        [NotMapped]
        [Display(Prompt = "Main")]
        [Model(Show = false, Edit = false)]
        public string RegularIntervalStr { get; set; }

        [NotMapped]
        [Display(Prompt = "Main")]
        [Model(Show = false, Edit = false)]
        public string OrderStatusStr { get; set; }
        [Display(Prompt = "Main")]
        [Model(Show = false, Edit = false)]
        public bool Active { get; set; }

        [Display(Prompt = "Main")]
        [Model(Show = false, Edit = false)]
        public bool Questioned { get; set; }

        [Display(Prompt = "Main")]
        [Model(Show = false, Edit = false)]
        public bool NotSentMailed { get; set; }

        [Display(Prompt = "Main")]
        [Model(Show = false, Edit = false)]
        public int ParentOrderID { get; set; }

        [Display(Prompt = "Main")]
        [Model(Show = false, Edit = false)]
        public string MessageToOwner { get; set; } //ex: for regular order auto processing

        [Display(Prompt = "Main")]
        [Model(Show = false, Edit = false)]
        public string Email { get; set; }
        [Display(Prompt = "Main")]
        [Model(Show = false, Edit = false)]
        public PaymentMethod PaymentMethod { get; set; }
         [Model(Show = false, Edit = false)]
        public string PaymentMethodStr
        {
            get
            {
                return RP.S("Enums.PaymentMethod."+ PaymentMethod.ToString());
            }
        }
        [Display(Prompt = "Main", Name = "ShippingMethod")]
        [Model(Show = false, Edit = false)]
        public ShippingMethod ShippingMethod { get; set; }
         [Model(Show = false, Edit = false)]
        public string ShippingMethodHebrew
        {
            get
            {
                return ShippingMethod == Models.ShippingMethod.Courier ? "ע\"י שליח" : "איסוף עצמי";
            }
        }
        [Display(Prompt = "Main")]
        [Model(Show = false, Edit = false)]
        public string Note { get; set; }
        [Display(Prompt = "Main")]
        [Model(Show = false, Edit = false)]
        public string CouponCode { get; set; }

        [HiddenInput(DisplayValue = false)]
        [Index]
        [Display(Prompt = "Main")]
        [Model(Show = false, Edit = false)]
        public Guid UID { get; set; }

        [NotMapped]
        [Model(Show = true, Edit = false, AjaxEdit = false)]
        [Display(Prompt = "Main")]
        public virtual User User { get; set; }

        [Display(Prompt = "Main")]
        public decimal SubTotal { get; set; }
        [NotMapped] // for showing in view
        [Model(Show = false, Edit = false)]
        public string SubTotalStr
        {
            get
            {
                return ShoppingService.FormatPrice(SubTotal);
            }
            set
            {

            }
        }

        [Display(Prompt = "Main")]
        [Model(Show = false, Edit = true, AjaxEdit = false)]
        public decimal ShipCost { get; set; }

        [Display(Prompt = "Main")]
        public decimal Total { get; set; }
         // ,[TotalCard]
      //,[TotalCash]
        //[Display(Prompt = "Main")]
        //public decimal TotalCard { get; set; }

        //[Display(Prompt = "Main")]
        //public decimal TotalCash { get; set; }

        [NotMapped] // for showing in view
        [Model(Show = false, Edit = false)]
        public string TotalStr
        {
            get
            {
                return ShoppingService.FormatPrice(Total);
            }
            set
            {

            }
        }

        //[Display(Prompt = "Main")]
        //public decimal TotalCash { get; set; }

         [Model(ShowInGrid = false, Edit = false)]
        [Display(Prompt = "Main")]
        public decimal TotalDiscountAmount { get; set; }
         [NotMapped] // for showing in view
         [Model(Show = false, Edit = false)]
         public string TotalDiscountAmountStr
         {
             get
             {
                 return ShoppingService.FormatPrice(TotalDiscountAmount);
             }
             set
             {

             }
         }

        [Display(Prompt = "Main")]
        [Model(ShowInGrid = false, Edit = false)]
        public string TotalDiscountDescription { get; set; }

        [Display(Prompt = "Main")]
        [Model(Show = false, Edit = false, AjaxEdit = false)]
        public decimal Fee { get; set; }

        

        [Display(Prompt = "Main")]
        public string ShipAddress { get; set; }

        [Display(Prompt = "Main")]
        [Model(Show = false, Edit = false, AjaxEdit = false)]
        public decimal PaymentToShopOwner { get; set; }


        [Display(Prompt = "Main")]
        [Model(Show = false, Edit = false, AjaxEdit = false)]
        public bool IsPaidUp { get; set; } // Store owner and admin pay each other

        [Display(Name = "Date", Prompt = "Main")]
        [Model(FilterOnTop = true)]
        public DateTime CreateOn { get; set; }

        [Model(Show = false, Edit = false, AjaxEdit = false)]
        [NotMapped]
        public string OrderTime {
            get
            {
                return CreateOn.ToString("dd.MM.yyyy HH:mm");
            }
        }
        [Model(Show = false, Edit = false, AjaxEdit = false)]
        [Display(Name = "Date of paid", Prompt = "Main")]
        public DateTime? PayedOn { get; set; }

        [Model(Show = false, Edit = false, AjaxEdit = false)]
        [Display(Name = "Date of delivering", Prompt = "Main")]
        public DateTime? DeliveredOn { get; set; }

        [Model(Show = false, Edit = false, AjaxEdit = false)]
        [Display( Prompt = "Main")]
        public DateTime? SentOn { get; set; }

        //[Display(Prompt = "Main")]
        //[Model(Show = false, Edit = false,Filter=true)]
        //public int OrderStatusID { get; set; }
        [Model(Show = false, Edit = false, AjaxEdit = false)]
        [Display(Prompt = "Main")]
        public DateTime? RefundOn { get; set; }

        [Model(Show = false, Edit = false, AjaxEdit = false)]
        [Display(Prompt = "Main")]
        public decimal RefundAmount { get; set; }

        [Model(Show = false, Edit = false, AjaxEdit = false)]
        [Display(Prompt = "Main")]
        public string MemberComment { get; set; }

        [Model(Show = false, Edit = false)]
        public string ApprovedGuid { get; set; }
         [Model(Show = false, Edit = false)]
        public string ApprovedToken { get; set; }
         [Model(Show = false, Edit = false)]
        public DateTime? ApprovedDate { get; set; }

        [Model(Show = false, Edit = false)]
         public bool LessFee { get; set; }
        //ALTER TABLE [dbo].[Orders]
        //ADD [LessFee] BIT DEFAULT 0 NOT NULL;

        [Model(Show = true, Edit = true, FilterOnTop = true)]
         [Display(Prompt = "Main")]
        [Index]
        public OrderStatus OrderStatus { get; set; }


        // [Display(Name="Pay Date",Prompt = "Main")]
        //  [Model(Show = false, Edit = false, AjaxEdit = false)]
        //   public DateTime? PayDate { get; set; }



        [NotMapped]
        [Display(Name = "Products", Order = 270, Prompt = "Products")]
        // [Model(ForeignKey="")]
        public List<OrderItem> OrderItems { get; set; }

        
            [NotMapped]
            [Display(Name = "OrderNote", Order = 270, Prompt = "OrderNote")]
        // [Model(ForeignKey="")]
        public List<OrderNote> OrderNotes { get; set; }

        #region acl
        public static bool AccessTest(Order order, ModelGeneralAttribute attr)
        {
            if (attr.Acl)
            {
                if (LS.CurrentShop != null)
                {
                    var join = (from oi in LS.CurrentEntityContext.OrderItems
                                join ps in LS.CurrentEntityContext.ProductShopMap
                                on oi.ProductShopID equals ps.ID
                                where ps.ShopID == LS.CurrentShop.ID
                                select ps.ShopID).Any();
                    return join;
                }
                return false;
            };
            return true;
        }
        public static IQueryable<Order> AccessList(ModelGeneralAttribute attr, Dictionary<string, object> param = null)
        {
            int ShopID = 0;
            if (param != null && param.ContainsKey("ShopID"))
            {
                ShopID = (int)param["ShopID"];
            }
            else if(attr.Acl)
            {
                ShopID = LS.CurrentShop.ID;
            }
            else
            {
                return LS.CurrentEntityContext.Orders;
            }
            return (
                from o in LS.CurrentEntityContext.Orders
              
                where o.ShopID == ShopID
                select o).Distinct();
        }
        #endregion
    }
}

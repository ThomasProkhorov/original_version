using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Uco.Models.Overview
{
    public class PaymentReportModel
    {
        public PaymentReportModel()
        {
            Items = new List<PaymentReportItemModel>();
        }
        public decimal TotalShop { get; set; }
        public string TotalShopStr { get; set; }

        public decimal TotalAdmin { get; set; }
        public string TotalAdminStr { get; set; }


        public decimal TotalFee { get; set; }
        public string TotalFeeStr { get; set; }

        public List<PaymentReportItemModel> Items { get; set; }
    }
    public class PaymentReportItemModel
    {
        public int ID { get; set; }
        public int OrderID { get; set; }
        public DateTime? Date { get; set; }
        public string DateStr { get; set; }

        public bool IsPaidUp { get; set; }

        public int ShopID { get; set; }
        public decimal Card { get; set; }
        public decimal Cash { get; set; }
        public decimal Total { get; set; }
        public string TotalStr { get; set; }


        public PaymentMethod PaymentMethod { get; set; }
        public PayedToType PayedTo { get; set; }
        public string PaymentMethodStr { get; set; }
        public string PayedToStr { get; set; }
    }
    public enum PayedToType
    {
        ToAdmin = 0,
        ToShop = 1,
    }
}
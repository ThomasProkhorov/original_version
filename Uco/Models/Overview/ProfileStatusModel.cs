using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Uco.Models.Overview
{
    public class ProfileStatusLine
    {
        public ProfileStatusLine()
        {
            Orders = new List<OrderStatusLine>();
        }
        public IList<OrderStatusLine> Orders { get; set; }
    }
    public class OrderStatusLine
    {
        public string Status { get; set; }
        public string OrderDate { get; set; }
        public string OrderEndDate { get; set; }
        public int StepStatus { get; set; }
        public int OrderID { get; set; }
        public string OrderTotal { get; set; }
    }
}
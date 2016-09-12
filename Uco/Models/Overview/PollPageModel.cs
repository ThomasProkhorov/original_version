using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Uco.Models.Overview
{
    public class PollPageModel
    {
        public int OrderID { get; set; }
        public int ShopID { get; set; }
        public int ProductID { get; set; }
        public string PollUrl { get; set; }

        public Guid? token { get; set; }
    }
}
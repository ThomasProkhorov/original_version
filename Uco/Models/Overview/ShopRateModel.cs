using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Uco.Models.Overview
{
    public class ShopRateModel
    {
        public int ID { get; set; }

        public Guid UserID { get; set; }

        public int ShopID { get; set; }

        public decimal Rate { get; set; }

        public string UserName { get; set; }
    }
    public class ShopCommentModel
    {
        public int ID { get; set; }

        public Guid UserID { get; set; }

        public int ShopID { get; set; }

        public int ParentID { get; set; }

        public string Title { get; set; }

        public string Text { get; set; }
        public DateTime CreateDate { get; set; }
        public string UserName { get; set; }

        public IList<ShopCommentModel> ShopComments { get; set; }
    }
}
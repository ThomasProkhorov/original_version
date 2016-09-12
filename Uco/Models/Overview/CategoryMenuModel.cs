using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Uco.Models.Overview
{
    public class CategoryMenuModel
    {
        public int CategoryID { get; set; }
        public bool Published { get; set; }
        public bool HeadOfGroup { get; set; }
        public int Order { get; set; }
        //public List<CategoryMenuModel> Items {get;set;}
    }
}
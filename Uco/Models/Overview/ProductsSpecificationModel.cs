using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Uco.Models.Overview
{
    public class ProductsSpecificationModel
    {
        public IEnumerable<ProductOverviewModel> ProductsOverviewModel { get; set; }
        public IEnumerable<SpecificationOptionModel> Specifications { get; set; }
    }
}
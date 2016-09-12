using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using System.Web.Mvc;
using Uco.Infrastructure;

namespace Uco.Models
{
    [ModelGeneral(AjaxEdit = true, Edit = false, Create = false, CreateAjax = true)]
    public partial class ProductAttributeOption
    {
        [HiddenInput(DisplayValue = false)]
        [Key]
        public int ID { get; set; }

        [Model(Show = false, Edit = false)]
        public int ProductShopID { get; set; }

        // Don`t needed for this project
        //  [Model(Show = false, Edit = false)]
        // public int ProductAttributeID { get; set; }

        // [NotMapped]
        // [Model(Filter = true, Sort = true, IsDropDownName = true)]
        //  public virtual ProductAttribute ProductAttribute { get; set; }

        [NotMapped]
        [Model(Show = false, Edit = false, AjaxEdit = false)]
        public virtual ProductShop ProductShop { get; set; }

        [Model(ShowInParentGrid = true, Filter = true)]
        public string Name { get; set; }

        [Display(Name = "SKU")]
        public string OverridenSku { get; set; }

        [Display(Name = "Price")]
        public decimal? OverridenPrice { get; set; }

        public decimal Quantity { get; set; }
    }
}

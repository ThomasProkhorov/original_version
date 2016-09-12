using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Mvc;
using Uco.Infrastructure;

namespace Uco.Models
{
    [ModelGeneral(Role = "Admin", AjaxEdit = true, Edit = true, Create = true, CreateAjax = true)]
   public partial class ShoppingCartItem

    {

        public ShoppingCartItem()
        {
            LastDate = DateTime.UtcNow;
        }

       [HiddenInput(DisplayValue = false)]
       [Key]
       public int ID { get; set; }

       [Model(Show = false, Edit = false)]
       public Guid UserID { get; set; }

     

       [Model(Show = false, Edit = false)]
       public int ShopID { get; set; }

       [Model(Show = false, Edit = false)]
       public int ProductShopID { get; set; }


       [NotMapped]
       [Model(Show = true, Edit = false, AjaxEdit = true)]
       public virtual ProductShop ProductShop { get; set; }

       // public int SelectedAttributeID { get; set; }
       [Model(Show = false, Edit = false)]
       public int ProductAttributeOptionID { get; set; }

       [NotMapped]
       [Model(Show = true, Edit = false, AjaxEdit = true)]
       public virtual ProductAttributeOption ProductAttributeOption { get; set; }


      
       public decimal Quantity { get; set; }

        [NotMapped]
        [Model(Show = false, Edit = false)]
       public string MeasureUnit { get; set; }
       public QuantityType QuantityType { get; set; }

       [NotMapped]
      // [Model(Show = false, AjaxEdit = false)]
      // [Display(Prompt = "Main")]
       public virtual Shop Shop { get; set; }

       public virtual DateTime LastDate { get; set; }
   }

public enum QuantityType
{
    Default = 0,
    ByUnit = 1
}
}
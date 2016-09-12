using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Web.Mvc;
using Uco.Infrastructure;

namespace Uco.Models
{
    [ModelGeneral(AjaxEdit = true, Edit = true, Create = true, CreateAjax = true)]
    public partial class ProductFavorite
    {
        [Display(Prompt = "Main", Order = 1)]
        [HiddenInput(DisplayValue = false)]
        [Key]
        public int ID { get; set; }

        [Model(Edit = false, AjaxEdit = false, Filter = true, Sort = true)]
        [Display(Prompt = "Main")]
        public Guid UserID { get; set; }

        //[Display(Prompt = "Main")]
        //[Model(Show = false, Edit = false)]
        //public int ProductID { get; set; }

        //[Display(Prompt = "Main")]
        //[Model(Show = false, Edit = false)]
        //public int ShopID { get; set; }
        [Display(Prompt = "Main")]
        [Model(Show = false, Edit = false)]
        public int ProductShopID { get; set; }

        [Display(Prompt = "Main")]
        public DateTime CreateDate { get; set; }
    }
}

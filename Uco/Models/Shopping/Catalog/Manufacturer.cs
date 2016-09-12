using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Uco.Infrastructure;
using Uco.Infrastructure.Livecycle;

namespace Uco.Models
{

    [ModelGeneral(Role = "Admin", AjaxEdit = false, Edit = true, Create = true, CreateAjax = false)]
    public partial class Manufacturer
    {
        [HiddenInput(DisplayValue = false)]
        [Key]
        [Display(Prompt = "Main", Order = 1)]
        public int ID { get; set; }

        [Display(Prompt = "Main")]
        [Model(Filter = true, IsDropDownName = true, ShowInParentGrid = true)]
        public string Name { get; set; }

        [UIHint("Image")]
        [Display(Prompt = "Main")]
        public string Image { get; set; }



        [Model(Show = false, Edit = false)]
        [Model(Role = "Admin", Show = true, Edit = true, Sort = true)]
        [Display(Prompt = "Main")]
        public int DisplayOrder { get; set; }

        [Model(Show = false, Edit = false)]
        [Model(Role = "Admin", Show = false, Edit = false, Filter = true)]
        [Display(Prompt = "Main")]
        public bool Published { get; set; }

        #region Events
        public static void OnUpdating(Manufacturer manufacturer)
        {

            LS.CurrentEntityContext.Database.ExecuteSqlCommand(@"UPDATE Products SET DisplayOrder = "
                + manufacturer.DisplayOrder + @" WHERE 
            ProductManufacturerID = " + manufacturer.ID);
        }

        public static void OnDeleted(Manufacturer manufacturer)
        {
            LS.CurrentEntityContext.Database.ExecuteSqlCommand(@"UPDATE Products SET DisplayOrder = 0 WHERE 
            ProductManufacturerID = " + manufacturer.ID);

        }
        #endregion
    }
}
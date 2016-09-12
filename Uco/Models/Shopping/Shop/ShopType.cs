using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Mvc;
using Uco.Infrastructure;

namespace Uco.Models
{
    [ModelGeneral(Role = "Admin", Cached = true, AjaxEdit = true, Edit = false, Create = false, CreateAjax = true)]
    public partial class ShopType
    {
        [HiddenInput(DisplayValue = false)]
        [Key]
        public int ID { get; set; }

          [Model(Filter = true,Sort=true)]
        public string Name {get;set;}

          [Model(Filter = true,Sort=true)]
       public int DisplayOrder { get; set; }
    }
}

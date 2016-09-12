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
    [ModelGeneral(Role="Admin",AjaxEdit = false, Edit = true, Create = true, CreateAjax = false)]
    [ModelGeneral(AjaxEdit = false, CreateAjax = false,Edit = true, Create = true)]
    public partial class ShopMessage
    {
        [HiddenInput(DisplayValue = false)]
        [Key]
        [Model(Show = false, Edit = false)]
        public int ID { get; set; }
        [Model(Show = false, Edit = false)]
        public int ShopID { get; set; }
        [NotMapped]
        [Model(Role = "Admin", Show = true, Edit = true)]
        [Model(Show = false, Edit = false)]
        public Shop Shop { get; set; }
        public bool Active { get; set; }

       public DateTime? StartDate { get; set; }

       public DateTime? EndDate { get; set; }

       [DataType(DataType.MultilineText)]
        public string Message { get; set; }
    }

   
}

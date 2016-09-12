using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Mvc;
using Uco.Infrastructure;
using Uco.Infrastructure.Livecycle;


namespace Uco.Models
{
    [ModelGeneral(Role = "Admin", AjaxEdit = false, Edit = false, Create = false, CreateAjax = false, Delete = false)]
   // [ModelGeneral(Role = "Member", AjaxEdit = false, Edit = false, Create = false, CreateAjax = false,Delete=false)]
    public partial class UserAddressSearchActivity
    {
        
        [HiddenInput(DisplayValue = false)]
        [Key]
        [Model(Edit = false, Filter = true, Sort = true, Show = false)]
        public int ID { get; set; }

        [Model(Edit = false, AjaxEdit = false, Filter = true, Sort = true, Show = false)]
        [Display(Name = "UserID")]
        public Guid UserID { get; set; }

        [NotMapped]
        [Model(Show = false, Edit = false)]
        [Model(Role = "Admin", Show = true, Edit = true)]
        [Display(Name = "User", Order = 100, Prompt = "TabMain")]
        public User User { get; set; }

         [Model(Edit = false, Filter = true, Sort = true, Show = true)]
        public string Address { get; set; }

         [Model(Edit = false, Filter = true, Sort = true, Show = true)]
         public string AddressWroten { get; set; }

         [Model(Edit = false, Filter = true, Sort = true, Show = true)]
         public string ShopType { get; set; }

        [Model(Show = false, Edit = false)]
        public decimal Latitude {get;set;}
        [Model(Show = false, Edit = false)]
        public decimal Longitude { get; set; }
        [Model(Show = false, Edit = false)]
        public int ShopID { get; set; }
         [Model(Edit = false, Filter = true, Sort = true, Show = true)]
        public string ShopName { get; set; }
         [Model(Edit = false, Filter = true, Sort = true, Show = true)]
        public DateTime CreateOn { get; set; }
        [Model(Show = false, Edit = false)]
        public string PageUrl { get; set; }
        [Model(Show = false, Edit = false)]
        public string RefererUrl { get; set; }
        [Model(Show = true, Edit = false)]
        public string IP { get; set; }
     
      }

}

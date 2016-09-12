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
    [ModelGeneral( AjaxEdit = true, Edit = true, Create = true, CreateAjax = true)]
    public partial class ActivityLog
    {
       [HiddenInput(DisplayValue = false)]
       [Key]
       public int ID { get; set; }

       [Model(Edit = false, AjaxEdit = false, Filter = true, Sort = true, Show = false)]
       [Display(Name = "UserID")]
       public Guid UserID { get; set; }



       [NotMapped]
       [Model(Show = false, Edit = false)]
       [Model(Role = "Admin", Show = false, Edit = true)]
      public User User { get; set; }

        [Index]
       public EntityType EntityType { get; set; }

        [Index]
        public int? EntityID { get; set; }

        [Index]
       public ActivityType ActivityType { get; set; }
       public DateTime CreateOn { get; set; }

       public string RequestUrl { get; set; }
       public string ShortDescription { get; set; }

       public string FullText { get; set; }

       public string DirectSQL { get; set; }

       
       public string UploadedFileName { get; set; }

       public string CopiedFileName { get; set; }
    }
    public enum ActivityType {
    Create = 1,
        Update = 2,
        Delete = 3,
        Bulk = 4,
        Other = 10
    }
    public enum EntityType
    {
        Product = 1,
        Category = 2,
        ProductShop = 3,
    }
}

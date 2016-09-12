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
    [ModelGeneral(Role = "Admin", Cached = true, AjaxEdit = true, Edit = false, Create = false, CreateAjax = true)]
    public partial class UrlRecord
    {
        [HiddenInput(DisplayValue = false)]
        [Key]
        public int ID { get; set; }

        [Index("SeoUrl",0)]
        [Model(Filter = true)]
        public int EntityID { get; set; }

        [Index("SeoUrl", 1), StringLength(250)]
        [Model(Filter = true)]
        public string EntityName { get; set; }

        [Index("UrlSlugUniq", IsUnique = true), StringLength(400)]
        [Model(Filter = true)]
        public string Slug {get;set;}

          [Model(Filter = true)]
       public bool IsActive { get; set; }
    }
}

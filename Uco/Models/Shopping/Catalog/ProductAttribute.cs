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
    public partial class ProductAttribute
    {
        [HiddenInput(DisplayValue = false)]
        [Key]
        public int ID { get; set; }

        [Model(Show = false, Edit = false)]
        public int AttributeTypeID { get; set; }

        [NotMapped]
        public AttributeType AttributeType { get; set; }


        [Model(Filter = true)]
        public string Name { get; set; }

    }
}

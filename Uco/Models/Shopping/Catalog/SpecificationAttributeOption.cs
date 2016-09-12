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
    [ModelGeneral(AjaxEdit = true, Edit = true, Create = true, CreateAjax = true)]
    public partial class SpecificationAttributeOption
    {
        [HiddenInput(DisplayValue = false)]
        [Key]
        public int ID { get; set; }

        //  [Model(Show = false, Edit = false)]
        //  public int ProductID { get; set; }

        [Model(Show = false, Edit = false)]
        public int SpecificationAttributeID { get; set; }

        [NotMapped]
        [Model(Filter = true, Sort = true)]
        public virtual SpecificationAttribute SpecificationAttribute { get; set; }

        //  [NotMapped]
        //  [Model(Show = false, AjaxEdit = false)]
        //  public virtual Product Product { get; set; }

        [Model(Filter = true)]
        public string Name { get; set; }
    }
}

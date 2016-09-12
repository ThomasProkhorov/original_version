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
    public partial class SpecificationAttribute
    {
        [HiddenInput(DisplayValue = false)]
        [Key]
        [Display(Prompt = "Main", Order = 1)]
        public int ID { get; set; }

        [Model(Show = false, Edit = false)]
        [Display(Prompt = "Main")]
        public int SpecificationAttributeTypeID { get; set; }

        [NotMapped]
        [Display(Prompt = "Main")]
        public SpecificationAttributeType SpecificationAttributeType { get; set; }

        [Display(Prompt = "Main")]
        [Model(Filter = true)]
        public string Name { get; set; }

        //Grid
        [NotMapped]
        [Display(Name = "Options", Order = 270, Prompt = "Options")]
        public List<SpecificationAttributeOption> SpecificationAttributeOptions { get; set; }

    }
}

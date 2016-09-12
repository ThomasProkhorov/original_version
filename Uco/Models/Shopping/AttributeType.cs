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
    [ModelGeneral(Role="Admin",Cached=true,AjaxEdit = true, Edit = true, Create = true, CreateAjax = true)]
    public partial class AttributeType
    {
        [HiddenInput(DisplayValue = false)]
        [Key]
        public int ID { get; set; }
       
        public string Name {get;set;}

    }
}

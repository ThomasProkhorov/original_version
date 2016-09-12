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
   // [ModelGeneral(AjaxEdit = true, CreateAjax = true, Create = false)]
    public partial class MessageTemplate
    {
        [HiddenInput(DisplayValue = false)]
        [Key]
        [Model(Show = false, Edit = false)]
        public int ID { get; set; }

      //  [Display(Name = "LanguageCode", Order = 900, )]
        [UIHint("Languages")]
        [Index("Uniq_SystemName",2, IsUnique = true), StringLength(50)]
        [Model(Filter=true)]
        public  string LanguageCode { get; set; }

        [Model(Filter = true)]
        public bool Active { get; set; }

        [Index("Uniq_SystemName",1,IsUnique = true), StringLength(500)]
        [Model(Filter = true)]
        public string SystemName { get; set; }

        public string Bcc { get; set; }

        [DataType(DataType.MultilineText)]
        [AllowHtml]
        public string Subject { get; set; }

        [UIHint("Html")]
        [AllowHtml]
        [Model(Show=false)]
        public string Body { get; set; }
    }


}

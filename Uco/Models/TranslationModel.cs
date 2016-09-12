using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web.Mvc;
using System.Xml.Serialization;
using Uco.Infrastructure;
using Uco.Infrastructure.Repositories;

namespace Uco.Models
{

    public class Translation
    {
        [Key]
        [HiddenInput(DisplayValue = false)]
        public int ID { get; set; }
        [Display(Name = "SystemName", ResourceType = typeof(Uco.Models.Resources.SystemModels))]
        [Required()]
        public string SystemName { get; set; }

        [Display(Name = "LangCode", ResourceType = typeof(Uco.Models.Resources.SystemModels))]
        [UIHint("Languages")]
        public string LangCode { get; set; }

        [Display(Name = "Text", ResourceType = typeof(Uco.Models.Resources.SystemModels))]
        [AllowHtml]
        [DataType(DataType.MultilineText)]
        public string Text { get; set; }

        public Translation()
        {
            this.LangCode = SF.GetLangCodeThreading();
        }
    }
}
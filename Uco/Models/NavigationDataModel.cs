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
    public class NavigationData
    {
        [Key]
        [Display(Name = "ID")]
        [HiddenInput(DisplayValue = false)]
        public int ID { get; set; }

        [Display(Name = "ParentID", ResourceType = typeof(Uco.Models.Resources.SystemModels))]
        [UIHint("NavigationParent")]
        [Required(ErrorMessage = "עמוד האב חובה")]
        public int ParentID { get; set; }

        [Display(Name = "Order", ResourceType = typeof(Uco.Models.Resources.SystemModels))]
        [Required(ErrorMessage = "סדר הצגה חובה")]
        public int Order { get; set; }

        [Display(Name = "Title", ResourceType = typeof(Uco.Models.Resources.SystemModels))]
        [Required(ErrorMessage = "כתובת חובה")]
        public string Title { get; set; }

        [Display(Name = "Url", ResourceType = typeof(Uco.Models.Resources.SystemModels))]
        [Required(ErrorMessage = "קישור חובה")]
        public string Url { get; set; }

        [Display(Name = "Segment")]
        [UIHint("NavigationSegment")]
        //[Required(ErrorMessage = "תפריט חובה")]
        public string Segment { get; set; }

        [Display(Name = "LangCode", ResourceType = typeof(Uco.Models.Resources.SystemModels))]
        //[Required()]
        [UIHint("Languages")]
        public string LangCode { get; set; }

        [Display(Name = "CustumClass")]
        public string CustumClass { get; set; }

        [HiddenInput(DisplayValue = false)]
        [Required(ErrorMessage = "DomainID חובה")]
        public int DomainID { get; set; }

        public NavigationData()
        {
            this.DomainID = RP.GetAdminCurrentSettingsRepository().ID;
            this.Segment = "-";
            this.LangCode = SF.GetLangCodeCurrentSettings();
        }
    }

}
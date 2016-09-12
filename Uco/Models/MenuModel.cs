using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Uco.Models
{
    public class MenuModel
    {
        public MenuModel()
        {
            Childrens = new List<MenuModel>();
        }
        [HiddenInput(DisplayValue = false)]
        [Key]
        public int ID { get; set; }

        public virtual int ParentID { get; set; }

        public int DisplayOrder { get; set; }

        [Display(Name = "LangCode", ResourceType = typeof(Uco.Models.Resources.SystemModels))]
        [UIHint("Languages")]
        public string LangCode { get; set; }

        public string Group { get; set; }

        [Display(Name = "Title", Order = 100, ResourceType = typeof(Uco.Models.Resources.SystemModels), Prompt = "TabContent")]
        [Required(ErrorMessageResourceType = typeof(Uco.Models.Resources.SystemModels), ErrorMessageResourceName = "TitleRequired")]
        public virtual string Title { get; set; }

        //public virtual string ControllerName { get; set; }
        //public virtual string ActionName { get; set; }
        public virtual string PageUrl { get; set; } // from list 
        public virtual bool TargetBlank { get; set; }
        public virtual bool Published { get; set; }
        public virtual string IconUrl { get; set; }
        public virtual string CustomCss { get; set; }
        public virtual string CustomClass { get; set; }

        public virtual List<MenuModel> Childrens { get; set; }
    }
}
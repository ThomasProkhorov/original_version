using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Mvc;
using Uco.Infrastructure;
using Uco.Infrastructure.Livecycle;

namespace Uco.Models
{
    [ModelGeneral(AjaxEdit=true,CreateAjax=true,Create=false)]
    public partial class ShopCategoryMenu

    {
        public ShopCategoryMenu()
        {
            
        }

        [Display(Prompt = "TabMain", Order = 1)]
        [HiddenInput(DisplayValue = false)]
        [Key]
        public int ID { get; set; }

        [Display(Prompt = "TabMain", Order = 1)]
        [Model(Show = false, Edit = false)]
        public int ShopID { get; set; }

        [Display(Name = "CategoryID", Order = 100,  Prompt = "TabMain")]
        [Model(Show = false, Edit = false)]
        public int CategoryID { get; set; }

        [Model(Sort = true)]
        [Display(Name = "DisplayOrder", Order = 100, Prompt = "TabMain")]
        public int DisplayOrder { get; set; }

        [Model(Filter = true, Sort = true)]
        [Display(Prompt = "TabMain", Order = 1)]
        public int GroupNumber { get; set; }

        /// <summary>
        /// 0 - main
        /// 1 - submenu
        /// </summary>
        [Model(Filter = true, Sort = true)]
        [Display(Prompt = "TabMain", Order = 1)]
        
        public int Level { get; set; }

        [Model(Filter = true)]
        public bool Published { get; set; }
         [Model(Filter = true)]
        public bool HeadOfGroup { get; set; }
        
        [NotMapped]
        [Model(Filter = true, Sort = true)]
        [Display(Prompt = "TabMain", Order = 1)]
        public virtual Category Category { get; set; }
       

        [NotMapped]
        [Model(Show = false,AjaxEdit=false)]
        [Display(Prompt = "TabMain", Order = 1)]
        public virtual Shop Shop { get; set; }

        [NotMapped]
        [Model(Show = false, Edit = false, AjaxEdit = false)]
        public int CacheProdCount { get; set; }
   }
}

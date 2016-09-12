using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Web.Mvc;
using Uco.Infrastructure;

namespace Uco.Models
{
    [ModelGeneral(AjaxEdit = true, Edit = false, Create = false, CreateAjax = false)]
    public partial class ShopComment
    {
        [Display(Prompt = "Main", Order = 1)]
        [HiddenInput(DisplayValue = false)]
        [Key]
        public int ID { get; set; }

        [Model(Show = false)]
        [Display(Prompt = "Main")]
        [HiddenInput(DisplayValue = false)]
        public Guid UserID { get; set; }

        [Display(Prompt = "Main")]
        [Model(Show = false, Edit = false)]
        public int ShopID { get; set; }

        [Display(Prompt = "Main")]
        [Model(Show = false, Edit = false)]
        public int ParentID { get; set; }

        [Display(Prompt = "Main")]
        [Model(ShowInParentGrid = true, Edit = true,Filter=true)]
        public string Title { get; set; }

        [Display(Prompt = "Main")]
        [Model(ShowInParentGrid = false, Edit = true,Filter=true)]
        [DataType(DataType.MultilineText)]
        public string Text { get; set; }

        [Display(Prompt = "Main")]
        [Model(Filter = true)]
        public bool Approved { get; set; }

        [Display(Prompt = "Main")]
        [Model(Role = "Member", Edit = false,AjaxEdit=false)]
        public DateTime CreateTime { get; set; }

       // [NotMapped]
        [Display(Prompt = "Main")]
        [Model(ShowInParentGrid = false, Edit = false,Filter=true)]
        public string UserName { get; set; }

        [NotMapped]
        public IList<ShopComment> ShopComments { get; set; }

    }
}

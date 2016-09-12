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
    [ModelGeneral(AjaxEdit = true, CreateAjax = true, Create = false)]
    public partial class Poll
    {
        [Display(Prompt = "Main")]
        [HiddenInput(DisplayValue = false)]
        [Key]
        [Model(Show = false, Edit = false)]
        public int ID { get; set; }
        [Display(Prompt = "Main")]
        [Model(Show = false, Edit = false)]
        public int ShopID { get; set; }
        [Display(Prompt = "Main")]
          [Model(Filter = true)]
        public bool Active { get; set; }
        [Display(Prompt = "Main")]
          [Model(Filter = true)]
        public bool IsRating { get; set; }
        [Display(Prompt = "Main")]
        [Index("Uniq_SystemName",IsUnique = true), StringLength(500)]
        [Model(Filter = true)]
        public string SystemName { get; set; }
        [Display(Prompt = "Main")]
        [DataType(DataType.MultilineText)]
        [Model(Filter = true)]
        public string Question { get; set; }
        [Display(Prompt = "Main")]
        public PollAssignType AssignType { get; set; }
        [Display(Prompt = "Variants")]
        [NotMapped]
        public List<PollVariant> Variants { get; set; }
        [Display(Prompt = "Answers")]
        [NotMapped]
        public List<PollAnswer> PollAnswers { get; set; }
    }

    public enum PollAssignType
    {
        Order = 1,
        Shop = 2,
        General = 3,
        Product = 4,
    }
    [ModelGeneral(Role = "Admin", AjaxEdit = true, Edit = false, Create = false, CreateAjax = true)]
    [ModelGeneral(AjaxEdit = true, CreateAjax = true, Create = false)]
    public partial class PollVariant
    {
        [HiddenInput(DisplayValue = false)]
        [Key]
        [Model(Show = true, Edit = false,AjaxEdit=false)]
        public int ID { get; set; }

        [Model(Show = false, Edit = false,AjaxEdit=false)]
        public int PollID { get; set; }

        public int DisplayOrder { get; set; }
        [Model(IsDropDownName=true)]
        public string Value { get; set; }

    }

     [ModelGeneral(Role = "Admin", AjaxEdit = false, Edit = false, Create = false, CreateAjax = false)]
    public partial class PollAnswer
    {
        [HiddenInput(DisplayValue = false)]
        [Key]
        [Model(Show = false, Edit = false)]
        public int ID { get; set; }

        public int PollID { get; set; }

        [NotMapped,Model(Show=false,Edit=false,AjaxEdit=false)]
        public Poll Poll {get;set;}
        [Model(Edit = false, Show = false, AjaxEdit = false)]
        [Display(Prompt = "Main")]
        public Guid UserID { get; set; }

        [NotMapped]
        [Model(Show = false, Edit = false)]
        [Model(Role = "Admin", Show = true, Edit = false,AjaxEdit=false)]
        [Display(Name = "User", Order = 100, Prompt = "TabMain")]
        public User User { get; set; }


         [Model(Show = true, Edit = false,AjaxEdit=false)]
        public int PollVariantID { get; set; }

        [NotMapped, Model(Show = true, Edit = false, AjaxEdit = false)]
        public PollVariant PollVariant { get; set; }
        public int Rate { get; set; }

        public int? OrderID { get; set; }

        public int? ShopID { get;set;}
        public int? ProductID { get; set; }
    }
   
}

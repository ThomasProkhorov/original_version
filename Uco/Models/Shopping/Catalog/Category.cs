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
    [ModelGeneral(Role = "Admin", AjaxEdit = false, Edit = true, Create = true, CreateAjax = false)]
    public partial class Category
    {
        [HiddenInput(DisplayValue = false)]
        [Key]
        [Display(Prompt = "Main", Order = 1)]
        public int ID { get; set; }

        [Display(Prompt = "Main")]
        [Model(Filter = true, IsDropDownName = true, ShowInParentGrid = true)]
        public string Name { get; set; }



        [Display(Prompt = "Main")]
        [DataType(DataType.MultilineText)]
        [MaxLength(4000)]
        [Model(Role = "Admin", Show = false, AjaxEdit = false)]
        [Model(Show = false, ShowInEdit = true, Edit = false)]
        public string SeoDescription { get; set; }

        [Display(Prompt = "Main")]
        [DataType(DataType.MultilineText)]
        [MaxLength(4000)]
        [Model(Role = "Admin", Show = false, AjaxEdit = false)]
        [Model(Show = false, ShowInEdit = true, Edit = false)]
        public string SeoKeywords { get; set; }



        [UIHint("Image")]
        [Display(Prompt = "Main")]
        public string Image { get; set; }

        [Display(Prompt = "Main")]
        [Model(Show = false, Edit = false)]
        public int ParentCategoryID { get; set; }

        [NotMapped]
        [Display(Prompt = "Main")]
        public virtual Category ParentCategory { get; set; }

        [Model(Show = true, Edit = true)]
        [Model(Role = "Admin", Show = true, Edit = true, Sort = true)]
        [Display(Prompt = "Main")]
        public int DisplayOrder { get; set; }

        [Model(Show = false, Edit = false)]
        [Model(Role = "Admin", Show = false, Edit = true, Filter = true)]
        [Display(Prompt = "Main")]
        public bool Published { get; set; }

        [NotMapped]
        [Display(Name = "Child categories", Order = 270, Prompt = "Child categories")]
        [Model(ForeignKey = "ParentCategoryID")]
        public List<Category> ChildCategories { get; set; }

        [NotMapped]
        [Display(Name = "Products", Order = 290, Prompt = "Products")]
        [Model(ForeignKey = "CategoryID")]
        public List<Product> Products { get; set; }

        [NotMapped]
        [Model(Show = false, Edit = false)]
        [Display(Name = "CachedProductCount", Order = 290, Prompt = "Main")]
        public int CachedProductCount { get; set; }

        public static void OnUpdating(Category category)
        {
            if (category != null)
            {

                var allCategories = LS.Get<Category>();
                var childs = allCategories.Any(x => x.ParentCategoryID == category.ID);
                if (childs && category.ParentCategoryID != 0)
                {
                    category.ParentCategoryID = 0;
                }


            }
        }
        //public static void OnCreating(Category category)
        //{

        //}
    }
}

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Uco.Infrastructure;
using Uco.Infrastructure.Livecycle;

namespace Uco.Models
{

    [ModelGeneral(Role = "Admin", AjaxEdit = false, Edit = true, Create = true, CreateAjax = false)]
    public partial class Measure
    {
        [HiddenInput(DisplayValue = false)]
        [Key]
        [Display(Prompt = "Main", Order = 1)]
        public int ID { get; set; }

        [Display(Prompt = "Main")]
        [Model(Filter = true, IsDropDownName = true, ShowInParentGrid = true)]
        public string Name { get; set; }

        [Display(Prompt = "Main")]
        [Model(Filter = true, IsDropDownName = true, ShowInParentGrid = true)]
        public string NameForUser { get; set; }



        [Model(Show = true, Edit = true, Filter = true)]
        [Display(Prompt = "Main")]
        public bool ShowMiliMeasure { get; set; }

        [Display(Prompt = "Main")]
        [Model(Filter = true)]
        public string MiliName { get; set; }

        [Display(Prompt = "Main")]
        [Model(Filter = true)]
        public string MiliNameForUser { get; set; }

        [Model(Show = true, Edit = true, Filter = true)]//default 100
        [Display(Prompt = "Main")]
        public decimal PriceMultiplier { get; set; }

        [Display(Prompt = "Main")]
        [Model(Filter = true)]
        [DataType(DataType.MultilineText)]
        public string NameVariants { get; set; }

        [NotMapped]
        [Model(Show = false, Edit = false)]
        private List<string> _VariantList { get; set; }

        [NotMapped]
        [Model(Show = false, Edit = false)]
        public List<string> VariantList
        {
            get
            {
                if (_VariantList == null)
                {
                    if (!string.IsNullOrEmpty(NameVariants))
                    {
                        _VariantList = NameVariants.Split(new char[] { ',', '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries).ToList();
                        _VariantList.Add(Name);
                        _VariantList.Add(NameForUser);
                    }
                }
                if (_VariantList == null)
                {
                    _VariantList = new List<string>();
                    _VariantList.Add(Name);
                    _VariantList.Add(NameForUser);
                }
                return _VariantList;

            }

        }

    }
}
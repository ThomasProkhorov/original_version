﻿using System;
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
    public partial class ProductRate
    {
        [Display(Prompt = "Main", Order = 1)]
        [HiddenInput(DisplayValue = false)]
        [Key]
        public int ID { get; set; }

        [Model(Edit = false, Show = false, AjaxEdit = false, Filter = true, Sort = true)]
        [Display(Prompt = "Main")]
        public Guid UserID { get; set; }

        [NotMapped]
        [Model(Show = false, Edit = false)]
        [Model(Role = "Admin", Show = false, Edit = true)]
        [Display(Name = "User", Order = 100, Prompt = "TabMain")]
        public User User { get; set; }

        [Display(Prompt = "Main")]
        [Model(Show = false, Edit = false)]
        public int ProductID { get; set; }

        [Display(Prompt = "Main")]
        public decimal Rate { get; set; }
    }
}

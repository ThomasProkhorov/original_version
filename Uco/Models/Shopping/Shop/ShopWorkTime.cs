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
    [ModelGeneral(Role="Admin",AjaxEdit = true, Edit = true, Create = true, CreateAjax = true)]
    [ModelGeneral(AjaxEdit = true, CreateAjax = true, Create = false)]
    public partial class ShopWorkTime
    {
        [HiddenInput(DisplayValue = false)]
        [Key]
        [Model(Show = false, Edit = false)]
        public int ID { get; set; }
        [Model(Show = false, Edit = false)]
        public int ShopID { get; set; }
        public bool Active { get; set; }

        public DayOfWeek Day { get; set; }

        [UIHint("IntTime")]
        public  int TimeFrom { get; set; }

        [UIHint("IntTime")]
        public int TimeTo { get; set; }

        public bool IsSpecial { get; set; }

        [UIHint("Date")]
        public DateTime Date { get; set; }
    }

    [ModelGeneral(Role = "Admin",Cached=true, AjaxEdit = true, Edit = true, Create = true, CreateAjax = true)]
    [ModelGeneral(Cached=true,Role="Member")]
    public partial class WeekDay
    {
        [HiddenInput(DisplayValue = false)]
        [Key]
        public int ID { get; set; }

        public string Name { get; set; }

    }

   
}

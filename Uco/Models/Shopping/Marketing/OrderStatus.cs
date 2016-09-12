using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Mvc;
using Uco.Infrastructure;

namespace Uco.Models
{
   // [ModelGeneral(Role="Admin",Cached=true,AjaxEdit = true, Edit = true, Create = true, CreateAjax = true)]
    //public partial class OrderStatus
    //{
    //    [HiddenInput(DisplayValue = false)]
    //    [Key]
    //    public int ID { get; set; }
       
    //    public string Name {get;set;}

    //}

    public enum OrderStatus
    {
        New=1,
Paid=2,
Accepted=3,
Sent=4,
Delivered=5,
Rejected=6,
Canceled=7,
   //     Refunded=8,
     //   PartialRefunded=9,
        Completed=10
    }
}

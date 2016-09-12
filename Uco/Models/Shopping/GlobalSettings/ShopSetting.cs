using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using System.Web.Mvc;
using Uco.Infrastructure;
using Uco.Infrastructure.Livecycle;

namespace Uco.Models
{

    public class ShopSetting
    {
        
       [HiddenInput(DisplayValue = false)]
       [Key]
       public int ID { get; set; }


       [Index("KeyUniq", IsUnique = true), StringLength(400)]
       [Model(Filter = true)]
       public string Key { get; set; }


      
       [Model(Filter = true)]
       public string Value { get; set; }
    }
   
}

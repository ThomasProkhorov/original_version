using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace Uco.Models.Shopping.Measure
{
    public class ContentUnitMeasure
    {
        [Key]
        public int ID { get; set; }
        public string Name { get; set; }
        public string DisplayName { get; set; } 
    }
}
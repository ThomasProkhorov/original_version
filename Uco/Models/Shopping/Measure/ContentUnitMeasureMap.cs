using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace Uco.Models.Shopping.Measure
{
    public class ContentUnitMeasureMap
    {
        [Key]
        public int ID { get; set; }
        public string Name { get; set; }

        [ForeignKey("ContentUnitMeasure")]
        public int ContentUnitMeasureID { get; set; }
        public string Synonymous { get; set; }
        public decimal Multiplicator { get; set; }

        public ContentUnitMeasure ContentUnitMeasure { get; set; }

    }
}
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
    [ModelGeneral(AjaxEdit = false, CreateAjax = false,Edit = true, Create = true)]
    public partial class ShopDeliveryZone
    {
        [HiddenInput(DisplayValue = false)]
        [Key]
        [Model(Show = false, Edit = false)]
        public int ID { get; set; }
        [Model(Show = false, Edit = false)]
        public int ShopID { get; set; }
        public bool Active { get; set; }

        [Model(Show = false, Edit = false)]
         public decimal RadiusLongitude { get; set; }

        [Model(Show = false, Edit = false)]
         public decimal RadiusLatitude { get; set; }

        [Model(Show = true)]
        [Model(Role = "Member", Show = true, Edit = true)]
        public int DeliveryTime { get; set; }

        [Model(Show = true)]
        [Model(Role = "Member", Show = true, Edit = true)]
        public decimal ShipCost { get; set; }

        [Model(Show = true)]
        [Model(Role = "Member", Show = true, Edit = true)]
        public decimal FreeShipFrom { get; set; }

        [UIHint("MapRadius")]
        [Model(Show = true)]
        [Model(Role = "Member", Show = true, Edit = true)]
        public decimal ShipRadius { get; set; }

        public bool DeliveryFroAllCity { get; set; }
        public string City { get; set; }
    }

    public class DeliveryZoneSmall
    {
        public int ID { get; set; }
        public decimal FreeShipFrom { get; set; }
        public decimal ShipCost { get; set; }
        public int DeliveryTime { get; set; }
        public int ShopID { get; set; }
        public double Distance { get; set; }
    }
   
}

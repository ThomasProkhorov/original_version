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
    public partial class ShopDesign
    {
        [HiddenInput(DisplayValue = false)]
        [Key]
        [Model(Show = false, Edit = false)]
        public int ID { get; set; }
        [Model(Show = false, Edit = false)]
        public int ShopID { get; set; }
        [NotMapped]
        [Model(Role = "Admin", Show = true, Edit = true)]
        [Model(Show = false, Edit = false)]
        public Shop Shop { get; set; }
        public bool Active { get; set; }


        #region productBox
        [Model(Show = true, Edit = true)]
        [StringLength(64),UIHint("ColorPicker")]
        public string ProductBoxBackground { get; set; }

        [Model(Show = true, Edit = true)]
        [StringLength(64), UIHint("ColorPicker")]
        public string ProductPriceColor { get; set; }

        [Model(Show = false, Edit = true)]
        [StringLength(64), UIHint("ColorPicker")]
        public string ProductBoxBorderColor { get; set; }
        [Model(Show = true, Edit = true)]
        [StringLength(64), UIHint("ColorPicker")]
        public string BuyButtonBackground { get; set; }
        [Model(Show = false, Edit = true)]
        [StringLength(64), UIHint("ColorPicker")]
        public string BuyButtonTextColor { get; set; }
        [Model(Show = false, Edit = true)]
        [StringLength(64), UIHint("ColorPicker")]
        public string BuyButtonBorderColor { get; set; }
        [Model(Show = false, Edit = true)]
        [StringLength(64), UIHint("ColorPicker")]
        public string BuyButtonHoverBackground { get; set; }
        [Model(Show = false, Edit = true)]
        [StringLength(64), UIHint("ColorPicker")]
        public string BuyButtonHoverTextColor { get; set; }
        [Model(Show = false, Edit = true)]
        [StringLength(64), UIHint("ColorPicker")]
        public string BuyButtonHoverBorderColor { get; set; }

        #endregion

        #region General

        [Model(Show = true, Edit = true)]
        [StringLength(64), UIHint("ColorPicker")]
        public string HightLightColor { get; set; }
        [Model(Show = false, Edit = true)]
        [StringLength(64), UIHint("ColorPicker")]
        public string PanelHeaderBackground { get; set; }

        [Model(Show = false, Edit = true)]
        [StringLength(64), UIHint("ColorPicker")]
        public string TopHeaderBackground { get; set; }
        
        #endregion

        #region Mobile
        
        [Model(Show = true, Edit = true)]
        [StringLength(64), UIHint("ColorPicker")]
        public string MobileMenuBackground { get; set; }

        [Model(Show = true, Edit = true)]
        [StringLength(64), UIHint("ColorPicker")]
        public string MobileMenuColor { get; set; }


        [Model(Show = false, Edit = true)]
        [StringLength(64), UIHint("ColorPicker")]
        public string MobileHeaderBackground { get; set; }


        [Model(Show = false, Edit = true)]
        [StringLength(64), UIHint("ColorPicker")]
        public string MobileBuyButton { get; set; }

        [Model(Show = false, Edit = true)]
        [StringLength(64), UIHint("ColorPicker")]
        public string MobileSearchButton { get; set; }

        [Model(Show = false, Edit = true)]
        [StringLength(64), UIHint("ColorPicker")]
        public string MobileProductBoxText { get; set; }
        //[Model(Show = true, Edit = true)]
        //[StringLength(64), UIHint("ColorPicker")]
        //public string MobileSlaveButton { get; set; }

        #endregion
        
       



        #region Cachedinfo

        [NotMapped]//we cache styles for performance
        [HiddenInput(DisplayValue = false)]
        [Model(Show = false, Edit = false)]
        public string RenderedStyle { get; set; }
        #endregion
    }


   
}

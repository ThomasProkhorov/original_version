using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web.Mvc;
using System.Xml.Serialization;
using Uco.Infrastructure;
using Uco.Infrastructure.Repositories;

namespace Uco.Models
{
    public class Banner
    {
        [Key]
        [HiddenInput(DisplayValue = false)]
        public virtual int ID { get; set; }

        [HiddenInput(DisplayValue = false)]
        public int DomainID { get; set; }

        [Display(Name = "Title", Order = 10, Prompt = "Data")]
        [Required(ErrorMessage = "Title Required")]
        public string Title { get; set; }

        [Display(Name = "Show for all", Order = 10, Prompt = "Data")]
        public bool ShowForAll { get; set; }

        [Display(Name = "BannerGroup", Order = 10, Prompt = "Data")]
        [Required(ErrorMessage = "BannerGroup Required")]
        [UIHint("BannerGroup")]
        public string BannerGroup { get; set; }

        [Display(Name = "Clicks", Order = 10, Prompt = "Data")]
        [HiddenInput(DisplayValue = true)]
        public int Clicks { get; set; }

        [Display(Name = "LangCode", Order = 15, Prompt = "Data")]
        [UIHint("Languages")]
        public string LangCode { get; set; }

        [Display(Name = "ShowDateMax", Order = 15, Prompt = "Data")]
        public DateTime ShowDateMax { get; set; }

        [Display(Name = "Width", Order = 15, Prompt = "Data")]
        public int Width { get; set; }

        [Display(Name = "Height", Order = 15, Prompt = "Data")]
        public int Height { get; set; }

        [HiddenInput(DisplayValue = false)]
        public int Type { get; set; }
        [Display(Name = "Type", Order = 25, Prompt = "Data")]
        [UIHint("Enum")]
        [NotMapped]
        public BannerType BannerTypeName
        {
            get { return (BannerType)Type; }
            set { Type = (int)value; }
        }
        public enum BannerType
        {
            Image,
            Flash,
            FlashAndBackground,
            Html,
            Text
        }

        [Display(Name = "Image or flash", Order = 30, Prompt = "Data")]
        [UIHint("File")]
        public string MainFile { get; set; }

        [NotMapped]
        [HiddenInput(DisplayValue = false)]
        public string Output { get; set; }

        [Display(Name = "Flash background", Order = 40, Prompt = "Data")]
        [UIHint("File")]
        public string OtherFile { get; set; }

        [Display(Name = "Link", Order = 50, Prompt = "Data")]
        public string Link { get; set; }

        [Display(Name = "Html", Order = 60, Prompt = "Data")]
        [DataType(DataType.MultilineText)]
        [DisplayFormat(HtmlEncode = false, ApplyFormatInEditMode = true)]
        [AllowHtml()]
        public string Html { get; set; }

        [Display(Name = "Text", Order = 120, Prompt = "Data")]
        [DataType(DataType.Html)]
        [DisplayFormat(HtmlEncode = false, ApplyFormatInEditMode = true)]
        [AllowHtml()]
        public virtual string Text { get; set; }

        public Banner()
        {
            this.DomainID = RP.GetAdminCurrentSettingsRepository().ID;
        }







    }

    public partial class Settings
    {
        [Display(Name = "AutocompleteOptions", Order = 70, Prompt = "Settings")]
        [DataType(DataType.MultilineText)]
        [AllowHtml]
        public string AutocompleteOptions { get; set; }
    }
    public class FacebookProfile
    {
        public string FacebookToken { get; set; }
        public string FacebookUsername { get; set; }
        public string FacebookEmail { get; set; }
        public string FacebookLocale { get; set; }
        public long FacebookID { get; set; }

        public string FacebookName { get; set; }
        public string FacebookFirstName { get; set; }
        public string FacebookLastName { get; set; }
        public string FacebookLink { get; set; }
        public string FacebookGender { get; set; }
        public int FacebookTimezone { get; set; }
        public bool FacebookVerified { get; set; }
        public DateTime FacebookUpdatedTime { get; set; }
    }
}
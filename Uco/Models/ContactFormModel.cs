using System.ComponentModel.DataAnnotations;
using Uco.Infrastructure;

namespace Uco.Models
{
    public class ContactSmallForm
    {
        [Display(Name = "Name", Order = 20, ResourceType = typeof(Uco.Models.Resources.SystemModels))]
        [Required(ErrorMessageResourceName = "NameRequired", ErrorMessageResourceType = typeof(Uco.Models.Resources.SystemModels))]
        public string ContactName { get; set; }

        [Display(Name = "Email", Order = 30, ResourceType = typeof(Uco.Models.Resources.SystemModels))]
        [UcoEmail(ErrorMessageResourceName = "EmailNotValid", ErrorMessageResourceType = typeof(Uco.Models.Resources.SystemModels))]
        public string ContactEmail { get; set; }

        [Display(Name = "Phone", Order = 40, ResourceType = typeof(Uco.Models.Resources.SystemModels))]
        [Required(ErrorMessageResourceName = "PhoneRequired", ErrorMessageResourceType = typeof(Uco.Models.Resources.SystemModels))]
        [UcoPhoneAttribute(ErrorMessageResourceName = "PhoneNotValid", ErrorMessageResourceType = typeof(Uco.Models.Resources.SystemModels))]
        public string ContactPhone { get; set; }
    }
}
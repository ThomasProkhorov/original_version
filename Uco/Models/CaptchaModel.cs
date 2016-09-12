using System.ComponentModel.DataAnnotations;
using Uco.Infrastructure;

namespace Uco.Models
{
    public class Captcha
    {
        [Display(Name = "Captcha", Order = 20, ResourceType = typeof(Uco.Models.Resources.SystemModels))]
        [AreaRemote("ValidateCaptcha", "Captcha", "", ErrorMessageResourceType = typeof(Uco.Models.Resources.SystemModels), ErrorMessageResourceName = "ValidateCaptcha")]
        [Required(ErrorMessageResourceType = typeof(Uco.Models.Resources.SystemModels), ErrorMessageResourceName = "CaptchaRequired")]
        public virtual string CaptchaValue { get; set; }
        public Captcha()
        {

        }
    }

    public class InvisibleCaptcha
    {
        [Display(Name = "InvisibleCaptcha", Order = 20, ResourceType = typeof(Uco.Models.Resources.SystemModels))]
        [AreaRemote("ValidateInvisibleCaptcha", "Captcha", "", ErrorMessageResourceType = typeof(Uco.Models.Resources.SystemModels), ErrorMessageResourceName = "ValidateInvisibleCaptcha")]
        public virtual string InvisibleCaptchaValue { get; set; }
        public InvisibleCaptcha()
        {

        }
    }
}
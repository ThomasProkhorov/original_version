using System;
using System.ComponentModel.DataAnnotations;

namespace Uco.Models
{

    public class ChangePasswordModel
    {
        [Required(ErrorMessage = "Old password required")]
        [DataType(DataType.Password)]
        [Display(Name = "OldPassword")]
        public string OldPassword { get; set; }

        [Required(ErrorMessage = "New password required")]
        [StringLength(100, ErrorMessage = "Password is to short", MinimumLength = 6)]
        [DataType(DataType.Password)]
        [Display(Name = "NewPassword")]
        public string NewPassword { get; set; }

        [DataType(DataType.Password)]
        [Display(Name = "ConfirmPassword")]
        [System.ComponentModel.DataAnnotations.Compare("NewPassword", ErrorMessage = "Confirm new password")]
        public string ConfirmPassword { get; set; }
    }

    public class LogOnModel
    {
        [Required(ErrorMessageResourceType = typeof(Uco.Models.Resources.SystemModels), ErrorMessageResourceName = "UserNameRequired")]
        [Display(Name = "UserName", ResourceType = typeof(Uco.Models.Resources.SystemModels))]
        public string UserName { get; set; }

        [Required(ErrorMessageResourceType = typeof(Uco.Models.Resources.SystemModels), ErrorMessageResourceName = "PasswordRequired")]
        [DataType(DataType.Password)]
        [Display(Name = "Password", ResourceType = typeof(Uco.Models.Resources.SystemModels))]
        public string Password { get; set; }

        [Display(Name = "RememberMe", ResourceType = typeof(Uco.Models.Resources.SystemModels))]
        public bool RememberMe { get; set; }
    }

    public class RegisterAjxModel
    {
        [Required(ErrorMessage = "שם פרטי חובה")]
        [Display(Name = "שם פרטי")]
        public string FirstName { get; set; }

        [Required(ErrorMessage = "שם משפחה חובה")]
        [Display(Name = "שם משפחה")]
        public string LastName { get; set; }

        [Required(ErrorMessageResourceType = typeof(Uco.Models.Resources.SystemModels), ErrorMessageResourceName = "EmailRequired")]
        [DataType(DataType.EmailAddress)]
        [Display(Name = "Email", ResourceType = typeof(Uco.Models.Resources.SystemModels))]
        [EmailAddress(ErrorMessage = "Invalid Email Address")]
        public string Email { get; set; }

        //[Required(ErrorMessageResourceType = typeof(Uco.Models.Resources.SystemModels), ErrorMessageResourceName = "EmailRequired")]
        [DataType(DataType.EmailAddress)]
        [Display(Name = "ConfirmEmail", ResourceType = typeof(Uco.Models.Resources.SystemModels))]
        [Compare("Email", ErrorMessageResourceType = typeof(Uco.Models.Resources.SystemModels), ErrorMessageResourceName = "ConfirmEmailCompare")]
        //[EmailAddress(ErrorMessage = "Invalid Email Address")]
        public string ConfirmEmail { get; set; }

        [Required(ErrorMessageResourceType = typeof(Uco.Models.Resources.SystemModels), ErrorMessageResourceName = "PasswordRequired")]
        [StringLength(100, ErrorMessageResourceType = typeof(Uco.Models.Resources.SystemModels), ErrorMessageResourceName = "PasswordMinimumLength", MinimumLength = 6)]
        [DataType(DataType.Password)]
        [Display(Name = "Password", ResourceType = typeof(Uco.Models.Resources.SystemModels))]
        public string Password { get; set; }

        //[DataType(DataType.Password)]
        //[Display(Name = "ConfirmPassword", ResourceType = typeof(Uco.Models.Resources.SystemModels))]
        //[Compare("Password", ErrorMessageResourceType = typeof(Uco.Models.Resources.SystemModels), ErrorMessageResourceName = "ConfirmPasswordCompare")]
        //public string ConfirmPassword { get; set; }

        public bool NewsLetter { get; set; }
        [Display(Name = "מספר משתמש")]
        public Guid UserID { get; set; }
    }

    public class RegisterModel
    {
        [Required(ErrorMessage = "שם פרטי חובה")]
        [Display(Name = "שם פרטי")]
        public string FirstName { get; set; }

        [Required(ErrorMessage = "שם משפחה חובה")]
        [Display(Name = "שם משפחה")]
        public string LastName { get; set; }

        [Required(ErrorMessageResourceType = typeof(Uco.Models.Resources.SystemModels), ErrorMessageResourceName = "UserNameRequired")]
        [Display(Name = "UserName", ResourceType = typeof(Uco.Models.Resources.SystemModels))]
        public string UserName { get; set; }

        [Required(ErrorMessageResourceType = typeof(Uco.Models.Resources.SystemModels), ErrorMessageResourceName = "EmailRequired")]
        [DataType(DataType.EmailAddress)]
        [Display(Name = "Email", ResourceType = typeof(Uco.Models.Resources.SystemModels))]
        public string Email { get; set; }

        [Required(ErrorMessageResourceType = typeof(Uco.Models.Resources.SystemModels), ErrorMessageResourceName = "PasswordRequired")]
        [StringLength(100, ErrorMessageResourceType = typeof(Uco.Models.Resources.SystemModels), ErrorMessageResourceName = "PasswordMinimumLength", MinimumLength = 6)]
        [DataType(DataType.Password)]
        [Display(Name = "Password", ResourceType = typeof(Uco.Models.Resources.SystemModels))]
        public string Password { get; set; }

        [DataType(DataType.Password)]
        [Display(Name = "ConfirmPassword", ResourceType = typeof(Uco.Models.Resources.SystemModels))]
        [System.ComponentModel.DataAnnotations.Compare("Password", ErrorMessageResourceType = typeof(Uco.Models.Resources.SystemModels), ErrorMessageResourceName = "ConfirmPasswordCompare")]
        public string ConfirmPassword { get; set; }

        [Display(Name = "סכום תשלום")]
        [Required(ErrorMessage = "סכום תשלום חובה")]
        public string PayPrice { get; set; }

        [Display(Name = "שם מוצר")]
        public string PayText { get; set; }

        [Display(Name = "מספר משתמש")]
        public Guid UserID { get; set; }
    }
}

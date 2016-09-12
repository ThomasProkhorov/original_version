using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web.Mvc;
using System.Xml.Serialization;
using Uco.Infrastructure;
using Uco.Infrastructure.Livecycle;
using Uco.Infrastructure.Repositories;

namespace Uco.Models
{
    public partial class Settings
    {
        [Key]
        [HiddenInput(DisplayValue = false)]
        public int ID { get; set; }

        [Display(Name = "AdminEmail", Order = 50, ResourceType = typeof(Uco.Models.Resources.SystemModels), Prompt = "TabSettings")]
        [Required(ErrorMessageResourceName = "AdminEmailRequired", ErrorMessageResourceType = typeof(Uco.Models.Resources.SystemModels))]
        public string AdminEmail { get; set; }

        [Display(Name = "HeaderHtml", Order = 60, ResourceType = typeof(Uco.Models.Resources.SystemModels), Prompt = "TabSettings")]
        [DataType(DataType.MultilineText)]
        [MaxLength(4000)]
        [AllowHtml]
        public string HeaderHtml { get; set; }
        [Display(Name = "FotterHtml", Order = 70, ResourceType = typeof(Uco.Models.Resources.SystemModels), Prompt = "TabSettings")]
        [DataType(DataType.MultilineText)]
        [MaxLength(4000)]
        [AllowHtml]
        public string FotterHtml { get; set; }

        [HiddenInput(DisplayValue = false)]
        public int DomainPageID { get; set; }

        [Display(Name = "LanguageRTL", Order = 100, ResourceType = typeof(Uco.Models.Resources.SystemModels), Prompt = "TabDomain")]
        public bool LanguageRTL { get; set; }

        [Display(Name = "LanguageCode", Order = 100, ResourceType = typeof(Uco.Models.Resources.SystemModels), Prompt = "TabDomain")]
        [UIHint("Languages")]
        public string LanguageCode { get; set; }

        [Display(Name = "Domain", Order = 100, ResourceType = typeof(Uco.Models.Resources.SystemModels), Prompt = "TabDomain")]
        [Required]
        public virtual string Domain { get; set; }

        [Display(Name = "Themes", Order = 100, ResourceType = typeof(Uco.Models.Resources.SystemModels), Prompt = "TabDomain")]
        [UIHint("Themes")]
        public virtual string Themes { get; set; }


        [Display(Name = "ProductBoxImageSize", Order = 100, Prompt = "TabDomain")]
        public virtual int ProductBoxImageSize { get; set; }

        [Display(Name = "ProductBoxImageSize", Order = 100, Prompt = "TabDomain")]
        public virtual int ProductBoxNoImageSize { get; set; }

        [Display(Name = "ProductPageImageSize", Order = 100, Prompt = "TabDomain")]
        public virtual int ProductPageImageSize { get; set; }



        [Display(Name = "Error404", Prompt = "Error404")]
        [AllowHtml()]
        [DataType(DataType.MultilineText)]
        public string Error404 { get; set; }

        [Display(Name = "MenuGroups", Prompt = "MenuGroups")]
        public string MenuGroups { get; set; }

        [Display(Name = "Roles", Order = 300, ResourceType = typeof(Uco.Models.Resources.SystemModels), Prompt = "TabRoles")]
        [UIHint("RolesDomain")]
        public string Roles { get; set; }

        //[Model(Role = "Admin", Show = false, Edit = true)]
        //[Model(Edit = false, AjaxEdit = false, Show = false)]
        //[Display( Prompt = "TabPayment")]
        //public string ClubCardTypes { get; set; }


        [Model(Role = "Admin", Show = false, Edit = true)]
        [Model(Edit = false, AjaxEdit = false, Show = false)]
        [Display(ResourceType = typeof(Uco.Models.Resources.Models), Prompt = "TabPayment")]
        public string CreditGuardUser { get; set; }

        [Model(Role = "Admin", Show = false, Edit = true)]
        [Display(ResourceType = typeof(Uco.Models.Resources.Models), Prompt = "TabPayment")]
        [Model(Edit = false, AjaxEdit = false, Show = false)]
        public string CreditGuardPass { get; set; }

        [Model(Edit = false, AjaxEdit = false, Show = false)]
        [Model(Role = "Admin", Show = false, Edit = true)]
        [Display(ResourceType = typeof(Uco.Models.Resources.Models), Prompt = "TabPayment")]
        public string CreditGuardTerminal { get; set; }

        [Model(Edit = false, AjaxEdit = false, Show = false)]
        [Model(Role = "Admin", Show = false, Edit = true)]
        [Display(ResourceType = typeof(Uco.Models.Resources.Models), Prompt = "TabPayment")]
        public string CreditGuardMid { get; set; }

        [Model(Edit = false, AjaxEdit = false, Show = false)]
        [Model(Role = "Admin", Show = false, Edit = true)]
        [Display(ResourceType = typeof(Uco.Models.Resources.Models), Prompt = "TabPayment")]
        public string CreditGuardUrl { get; set; }

        [NotMapped]
        public List<string> RolesList
        {
            get
            {
                return SF.RolesStringToList(this.Roles);
            }
        }
    }

    public class TextComponent
    {
        [Key]
        [HiddenInput(DisplayValue = false)]
        public int ID { get; set; }
        [Display(Name = "SystemName", ResourceType = typeof(Uco.Models.Resources.SystemModels))]
        [Required()]
        public string SystemName { get; set; }

        [Display(Name = "Description")]
        public string Description { get; set; }

        [HiddenInput(DisplayValue = false)]
        public int DomainID { get; set; }

        [Display(Name = "LangCode", ResourceType = typeof(Uco.Models.Resources.SystemModels))]
        [Required()]
        [UIHint("Languages")]
        public string LangCode { get; set; }

        [Display(Name = "Text", ResourceType = typeof(Uco.Models.Resources.SystemModels))]
        [UIHint("Html")]
        [AllowHtml]
        public string Text { get; set; }

        public TextComponent()
        {
            this.DomainID = RP.GetAdminCurrentSettingsRepository().ID;
        }
    }

    public class OutEmail
    {
        [Key]
        [HiddenInput(DisplayValue = false)]
        public int ID { get; set; }

        [HiddenInput(DisplayValue = false)]
        public int DomainID { get; set; }

        [Display(Name = "MailTo", ResourceType = typeof(Uco.Models.Resources.SystemModels))]
        public string MailTo { get; set; }
        [Display(Name = "Subject", Order = 30, ResourceType = typeof(Uco.Models.Resources.SystemModels))]
        public string Subject { get; set; }
        [Display(Name = "Body", Order = 40, ResourceType = typeof(Uco.Models.Resources.SystemModels))]
        [UIHint("Html")]
        [AllowHtml]
        public string Body { get; set; }
        [Display(Name = "LastTry", Order = 50, ResourceType = typeof(Uco.Models.Resources.SystemModels))]
        [Column(TypeName = "datetime2")]
        public DateTime LastTry { get; set; }
        [Display(Name = "TimesSent", Order = 60, ResourceType = typeof(Uco.Models.Resources.SystemModels))]
        public int TimesSent { get; set; }

        [Index]
        public OutEmailStatus SendStatus { get; set; }

        public DateTime? SendTime { get; set; }

        public OutEmail()
        {
            if (RP.GetAdminCurrentSettingsRepository() == null) this.DomainID = 0;
            else this.DomainID = RP.GetAdminCurrentSettingsRepository().ID;
        }
    }
    public enum OutEmailStatus
    {
        NotSendedYet = 0,
        Sended = 1,
        SendFailure = 2
    }
    public class Error
    {
        [Key]
        [HiddenInput(DisplayValue = false)]
        public int ID { get; set; }

        [HiddenInput(DisplayValue = false)]
        public int DomainID { get; set; }

        [Display(Name = "Date", Order = 10, ResourceType = typeof(Uco.Models.Resources.SystemModels))]
        [Column(TypeName = "datetime2")]
        public DateTime Date { get; set; }
        [Display(Name = "Message", Order = 10, ResourceType = typeof(Uco.Models.Resources.SystemModels))]
        public string Message { get; set; }
        [Display(Name = "Host", Order = 10, ResourceType = typeof(Uco.Models.Resources.SystemModels))]
        public string Host { get; set; }
        [Display(Name = "UserName", Order = 10, ResourceType = typeof(Uco.Models.Resources.SystemModels))]
        public string UserName { get; set; }
        [Display(Name = "PhysicalPath", Order = 10, ResourceType = typeof(Uco.Models.Resources.SystemModels))]
        public string PhysicalPath { get; set; }
        [Display(Name = "UserAgent", Order = 10, ResourceType = typeof(Uco.Models.Resources.SystemModels))]
        public string UserAgent { get; set; }
        [Display(Name = "UserHostAddress", Order = 10, ResourceType = typeof(Uco.Models.Resources.SystemModels))]
        public string UserHostAddress { get; set; }
        [Display(Name = "Url", Order = 10, ResourceType = typeof(Uco.Models.Resources.SystemModels))]
        public string Url { get; set; }
        [Display(Name = "UrlReferrer", Order = 10, ResourceType = typeof(Uco.Models.Resources.SystemModels))]
        public string UrlReferrer { get; set; }
        [Display(Name = "InnerException", Order = 10, ResourceType = typeof(Uco.Models.Resources.SystemModels))]
        public string InnerException { get; set; }
        [Display(Name = "Source", Order = 10, ResourceType = typeof(Uco.Models.Resources.SystemModels))]
        public string Source { get; set; }
        [Display(Name = "StackTrace", Order = 10, ResourceType = typeof(Uco.Models.Resources.SystemModels))]
        public string StackTrace { get; set; }
        [Display(Name = "TargetSite", Order = 10, ResourceType = typeof(Uco.Models.Resources.SystemModels))]
        public string TargetSite { get; set; }

        public Error()
        {
            this.DomainID = RP.GetAdminCurrentSettingsRepository().ID;
        }
    }

    public class Contact
    {
        [Key]
        [HiddenInput(DisplayValue = false)]
        public int ID { get; set; }

        [HiddenInput(DisplayValue = false)]
        public int DomainID { get; set; }

        [Display(Name = "Date", Order = 10, ResourceType = typeof(Uco.Models.Resources.SystemModels))]
        public DateTime ContactDate { get; set; }

        // [Required(ErrorMessage = "Full name required")]
        [Display(Name = "Name", Order = 20, ResourceType = typeof(Uco.Models.Resources.SystemModels))]
        public string ContactName { get; set; }

        //  [Required(ErrorMessage = "Email required")]
        [EmailAddress(ErrorMessage = "Invalid Email Address")]
        [Display(Name = "Email", Order = 30, ResourceType = typeof(Uco.Models.Resources.SystemModels))]
        public string ContactEmail { get; set; }

        // [Required(ErrorMessage = "Phone name required")]
        [Display(Name = "Phone", Order = 40, ResourceType = typeof(Uco.Models.Resources.SystemModels))]
        public string ContactPhone { get; set; }

        // [Required(ErrorMessage = "Text required")]
        [Display(Name = "Content", Order = 50, ResourceType = typeof(Uco.Models.Resources.SystemModels))]
        [UIHint("Html")]
        [AllowHtml]
        public string ContactData { get; set; }

        [Display(Name = "Url", Order = 60, ResourceType = typeof(Uco.Models.Resources.SystemModels))]
        [AllowHtml]
        [UIHint("Html")]
        public string ContactUrl { get; set; }

        [Display(Name = "Referal", Order = 70, ResourceType = typeof(Uco.Models.Resources.SystemModels))]
        public string ContactReferal { get; set; }

        [HiddenInput(DisplayValue = false)]
        public string RoleDefault { get; set; }

        [HiddenInput(DisplayValue = false)]
        public string Rool { get; set; }

        public int ShopID { get; set; }

        [NotMapped]
        public Shop Shop { get; set; }

        [NotMapped]
        public List<string> DropDownItems { get; set; }

        public Contact()
        {
            this.DomainID = RP.GetAdminCurrentSettingsRepository().ID;
        }
    }

    public class Newsletter
    {
        [Key]
        [HiddenInput(DisplayValue = false)]
        public int ID { get; set; }

        [HiddenInput(DisplayValue = false)]
        public int DomainID { get; set; }

        [Display(Name = "NewsletterDate", Order = 10, ResourceType = typeof(Uco.Models.Resources.SystemModels))]
        [Required(ErrorMessageResourceType = typeof(Uco.Models.Resources.SystemModels), ErrorMessageResourceName = "NewsletterDateRequired")]
        [Column(TypeName = "datetime2")]
        public DateTime NewsletterDate { get; set; }
        [Display(Name = "NewsletterName", Order = 10, ResourceType = typeof(Uco.Models.Resources.SystemModels))]
        [Required(ErrorMessageResourceType = typeof(Uco.Models.Resources.SystemModels), ErrorMessageResourceName = "NewsletterNameRequired")]
        public string NewsletterName { get; set; }
        [Display(Name = "NewsletterEmail", Order = 10, ResourceType = typeof(Uco.Models.Resources.SystemModels))]
        [Required(ErrorMessageResourceType = typeof(Uco.Models.Resources.SystemModels), ErrorMessageResourceName = "NewsletterDataRequired")]
        [UcoEmail(ErrorMessageResourceType = typeof(Uco.Models.Resources.SystemModels), ErrorMessageResourceName = "NewsletterMailNotValid")]
        public string NewsletterEmail { get; set; }
        [Display(Name = "NewsletterData", Order = 10, ResourceType = typeof(Uco.Models.Resources.SystemModels))]
        public string NewsletterData { get; set; }

        [Display(Name = "NewsletterPhone", Order = 10)]
        public string NewsletterPhone { get; set; }

        [NotMapped]
        [Display(Name = "NewsletterAccept", Order = 10, ResourceType = typeof(Uco.Models.Resources.SystemModels))]
        [Mandatory(ErrorMessageResourceType = typeof(Uco.Models.Resources.SystemModels), ErrorMessageResourceName = "NewsletterAcceptMandatory")]
        public bool NewsletterAccept { get; set; }

        [HiddenInput(DisplayValue = false)]
        public string RoleDefault { get; set; }

        public Newsletter()
        {
            this.DomainID = RP.GetAdminCurrentSettingsRepository().ID;
        }
    }

    [ModelGeneral(Show = true, Edit = true, AjaxEdit = false, CreateAjax = false, Create = false)]
    [ModelGeneral(Role = "Member", Acl = true, Show = true, Edit = true, AjaxEdit = false, CreateAjax = false, Create = false)]
    public class User
    {
        #region ACL

        public static IQueryable<User> AccessList(ModelGeneralAttribute attr, Dictionary<string, object> param = null)
        {
            int ShopID = 0;

            var query = from oi in LS.CurrentEntityContext.Users
                        select oi;
            if (attr.Acl) // show only ordered users
            {
                ShopID = LS.CurrentShop.ID;
                query = (from oi in LS.CurrentEntityContext.Users
                         join o in LS.CurrentEntityContext.Orders
                         on oi.ID equals o.UserID
                         where o.ShopID == ShopID
                         select oi).Distinct();
            }

            return query;
        }
        #endregion
        public User()
        {
            IsApproved = true;
        }
        [Key]
        [HiddenInput(DisplayValue = false)]
        public Guid ID { get; set; }


        [UIHint("ShopSelect")]
        [Model(Show = false, Edit = false)]
        public int ShopID { get; set; }

        [NotMapped]
        //[UIHint("GenericDropDown")]
        [Model(Role = "Admin", Show = true, Edit = true, AjaxEdit = false, FilterOnTop = true)]
        [Model(Show = false, Edit = false)]
        public Shop Shop { get; set; }

        [HiddenInput(DisplayValue = false)]
        [Index]
        public long FacebookID { get; set; }

        [HiddenInput(DisplayValue = false)]
        [Index, StringLength(48)]
        public string GoogleID { get; set; }

        [Display(Name = "CreationDate", Order = 10, ResourceType = typeof(Uco.Models.Resources.SystemModels))]
        [Column(TypeName = "datetime2")]
        public DateTime CreationDate { get; set; }

        [Display(Name = "UserName", Order = 10, ResourceType = typeof(Uco.Models.Resources.SystemModels))]
        [Required(ErrorMessageResourceType = typeof(Uco.Models.Resources.SystemModels), ErrorMessageResourceName = "UserNameRequired")]
        [MinLength(3)]
        [Model(IsDropDownName = true)]
        public string UserName { get; set; }

        [Display(Name = "FirstName", Order = 10, ResourceType = typeof(Uco.Models.Resources.SystemModels))]
        [Model(ShowInParentGrid = true, IsDropDownName = true)]
        public string FirstName { get; set; }

        [Display(Name = "LastName", Order = 10, ResourceType = typeof(Uco.Models.Resources.SystemModels))]
        [DataType(DataType.ImageUrl)]
        [Model(ShowInParentGrid = true, IsDropDownName = true)]
        public string LastName { get; set; }

        [Display(Name = "ApplicationName", Order = 10, ResourceType = typeof(Uco.Models.Resources.SystemModels))]
        [HiddenInput(DisplayValue = false)]
        public string ApplicationName { get; set; }

        [Display(Name = "Email", Order = 10, ResourceType = typeof(Uco.Models.Resources.SystemModels))]
        [DataType(DataType.EmailAddress)]
        [Required(ErrorMessageResourceType = typeof(Uco.Models.Resources.SystemModels), ErrorMessageResourceName = "EmailRequired")]
        [UcoEmail()]
        public string Email { get; set; }

        [Display(Name = "Password", Order = 10, ResourceType = typeof(Uco.Models.Resources.SystemModels))]
        [Required(ErrorMessageResourceType = typeof(Uco.Models.Resources.SystemModels), ErrorMessageResourceName = "PasswordRequired")]
        [MinLength(6)]
        public string Password { get; set; }

        [Display(Name = "PasswordQuestion", Order = 10, ResourceType = typeof(Uco.Models.Resources.SystemModels))]
        [HiddenInput(DisplayValue = false)]
        public string PasswordQuestion { get; set; }

        [Display(Name = "PasswordAnswer", Order = 10, ResourceType = typeof(Uco.Models.Resources.SystemModels))]
        [HiddenInput(DisplayValue = false)]
        public string PasswordAnswer { get; set; }

        [Display(Name = "IsApproved", Order = 10, ResourceType = typeof(Uco.Models.Resources.SystemModels))]
        public bool IsApproved { get; set; }

        [Display(Name = "LastActivityDate", Order = 10, ResourceType = typeof(Uco.Models.Resources.SystemModels))]
        [HiddenInput(DisplayValue = false)]
        [Column(TypeName = "datetime2")]
        public DateTime LastActivityDate { get; set; }

        [Display(Name = "LastLoginDate", Order = 10, ResourceType = typeof(Uco.Models.Resources.SystemModels))]
        [HiddenInput(DisplayValue = false)]
        [Column(TypeName = "datetime2")]
        public DateTime LastLoginDate { get; set; }

        [Display(Name = "LastPasswordChangedDate", Order = 10, ResourceType = typeof(Uco.Models.Resources.SystemModels))]
        [HiddenInput(DisplayValue = false)]
        [Column(TypeName = "datetime2")]
        public DateTime LastPasswordChangedDate { get; set; }

        [Display(Name = "IsOnline", Order = 10, ResourceType = typeof(Uco.Models.Resources.SystemModels))]
        [HiddenInput(DisplayValue = false)]
        public bool IsOnline { get; set; }

        [Model(Show = false, Edit = false)]
        public bool ApprovedBySms { get; set; }


        [Display(Name = "IsLockedOut", Order = 10, ResourceType = typeof(Uco.Models.Resources.SystemModels))]
        public bool IsLockedOut { get; set; }

        [Display(Name = "LastLockedOutDate", Order = 10, ResourceType = typeof(Uco.Models.Resources.SystemModels))]
        [HiddenInput(DisplayValue = false)]
        [Column(TypeName = "datetime2")]
        public DateTime LastLockedOutDate { get; set; }

        [Display(Name = "FailedPasswordAttemptCount", Order = 10, ResourceType = typeof(Uco.Models.Resources.SystemModels))]
        [HiddenInput(DisplayValue = false)]
        public int FailedPasswordAttemptCount { get; set; }

        [Display(Name = "FailedPasswordAttemptWindowStart", Order = 10, ResourceType = typeof(Uco.Models.Resources.SystemModels))]
        [HiddenInput(DisplayValue = false)]
        [Column(TypeName = "datetime2")]
        public DateTime FailedPasswordAttemptWindowStart { get; set; }

        [Display(Name = "FailedPasswordAnswerAttemptCount", Order = 10, ResourceType = typeof(Uco.Models.Resources.SystemModels))]
        [HiddenInput(DisplayValue = false)]
        public int FailedPasswordAnswerAttemptCount { get; set; }

        [Display(Name = "FailedPasswordAnswerAttemptWindowStart", Order = 10, ResourceType = typeof(Uco.Models.Resources.SystemModels))]
        [HiddenInput(DisplayValue = false)]
        [Column(TypeName = "datetime2")]
        public DateTime FailedPasswordAnswerAttemptWindowStart { get; set; }

        [Display(Name = "LastModified", Order = 10, ResourceType = typeof(Uco.Models.Resources.SystemModels))]
        [Column(TypeName = "datetime2")]
        public DateTime LastModified { get; set; }

        [Display(Name = "RoleDefault", Order = 10, ResourceType = typeof(Uco.Models.Resources.SystemModels))]
        // [UIHint("RoleDefault")]
        [HiddenInput(DisplayValue = false)]
        public string RoleDefault { get; set; }

        [Display(Name = "Roles", Order = 10, ResourceType = typeof(Uco.Models.Resources.SystemModels))]
        [UIHint("Roles")]
        [Required(ErrorMessageResourceType = typeof(Uco.Models.Resources.SystemModels), ErrorMessageResourceName = "RolesRequired")]
        public string Roles { get; set; }

        [Display(Name = "Comment", Order = 10, ResourceType = typeof(Uco.Models.Resources.SystemModels))]
        [DataType(DataType.MultilineText)]
        [MaxLength(4000)]
        public string Comment { get; set; }

        [UIHint("AddressMapWthCoord")]
        public string AddressMap { get; set; }

        [Model(Show = false, Edit = false)]
        public decimal Longitude { get; set; }

        [Model(Show = false, Edit = false)]
        public decimal Latitude { get; set; }


        //[Required(ErrorMessage = "Phone required")]
        public string Phone { get; set; }

        public string CompanyName { get; set; }

        [NotMapped]
        public List<Order> Orders { get; set; }

        [NotMapped]
        public List<string> RolesList
        {
            get
            {
                return SF.RolesStringToList(this.Roles);
            }
        }

        public bool IsInRole(string Role)
        {
            if (RolesList.Contains(Role)) return true;
            else return false;
        }
    }

    public class Role
    {
        [Key]
        [HiddenInput(DisplayValue = false)]
        public int ID { get; set; }

        [Display(Name = "Title", ResourceType = typeof(Uco.Models.Resources.SystemModels), Prompt = "TabData")]
        public string Title { get; set; }

        [Display(Name = "IsSystem", ResourceType = typeof(Uco.Models.Resources.SystemModels), Prompt = "TabData")]
        [HiddenInput(DisplayValue = false)]
        public bool IsSystem { get; set; }

        [Display(Name = "MenuPermissions", ResourceType = typeof(Uco.Models.Resources.SystemModels), Prompt = "Hidden")]
        [HiddenInput(DisplayValue = false)]
        [UIHint("MenuPermissions")]
        public string MenuPermissions { get; set; }

        [NotMapped]
        public List<string> MenuPermissionsList
        {
            get
            {
                return SF.RolesStringToList(this.MenuPermissions);
            }
        }
    }

    [NotMapped]
    public class FormField
    {
        [Display(Name = "ID", Order = 10, ResourceType = typeof(Uco.Models.Resources.SystemModels))]
        public int FormFieldID { get; set; }

        [Display(Name = "Order", Order = 10, ResourceType = typeof(Uco.Models.Resources.SystemModels))]
        [UIHint("Integer")]
        public int FormFieldOrder { get; set; }

        [Display(Name = "Title", Order = 10, ResourceType = typeof(Uco.Models.Resources.SystemModels))]
        [Required(ErrorMessageResourceType = typeof(Uco.Models.Resources.SystemModels), ErrorMessageResourceName = "TitleRequired")]
        public string FormFieldTitle { get; set; }

        [Display(Name = "Required", Order = 10, ResourceType = typeof(Uco.Models.Resources.SystemModels))]
        public bool FormFieldRequired { get; set; }

        [HiddenInput(DisplayValue = false)]
        public int FormFieldTypeValue { get; set; }

        [HiddenInput(DisplayValue = false)]
        [NotMapped]
        public string FormFieldTypeText
        {
            get
            {
                return ((FormFildType)FormFieldTypeValue).ToString();
            }
        }

        [Display(Name = "Type", Order = 10, ResourceType = typeof(Uco.Models.Resources.SystemModels))]
        [UIHint("Enum")]
        [XmlIgnore]
        public FormFildType FormFieldType
        {
            get { return (FormFildType)FormFieldTypeValue; }
            set { FormFieldTypeValue = (int)value; }
        }

        [Display(Name = "RequiredTitle", Order = 10, ResourceType = typeof(Uco.Models.Resources.SystemModels))]
        public string FormFieldRequiredTitle { get; set; }

        public enum FormFildType
        {
            Name,
            Text,
            EmailAddress,
            PhoneNumber,
            MultilineText,
            DropDown,
            RadioBottonList,
            CheckboxList
            //Html,
            //Date,
            //Time,
            //DateTime,
            //Currency,
            //Integer,
            //Number,
            //Url,
            //Duration,
            //Password,
            //UploadAnonim
        }

    }

    [NotMapped]
    public class FormRool
    {
        [Display(Name = "ID", Order = 10, ResourceType = typeof(Uco.Models.Resources.SystemModels))]
        public int FormRoolID { get; set; }

        [Display(Name = "Order", Order = 10, ResourceType = typeof(Uco.Models.Resources.SystemModels))]
        [UIHint("Integer")]
        public int FormRoolOrder { get; set; }

        [Display(Name = "Title", Order = 10, ResourceType = typeof(Uco.Models.Resources.SystemModels))]
        [Required(ErrorMessageResourceType = typeof(Uco.Models.Resources.SystemModels), ErrorMessageResourceName = "TitleRequired")]
        public string FormRoolTitle { get; set; }

        [Display(Name = "Email", Order = 10, ResourceType = typeof(Uco.Models.Resources.SystemModels))]
        [Required(ErrorMessageResourceType = typeof(Uco.Models.Resources.SystemModels), ErrorMessageResourceName = "EmailRequired")]
        public string FormRoolEmail { get; set; }

        [Display(Name = "And", Order = 10, ResourceType = typeof(Uco.Models.Resources.SystemModels))]
        public bool FormRoolAnd { get; set; }

        [Display(Name = "Item1", Order = 10, ResourceType = typeof(Uco.Models.Resources.SystemModels))]
        public string FormRoolItem1 { get; set; }

        [Display(Name = "Value1", Order = 10, ResourceType = typeof(Uco.Models.Resources.SystemModels))]
        public string FormRoolValue1 { get; set; }

        [Display(Name = "Item2", Order = 10, ResourceType = typeof(Uco.Models.Resources.SystemModels))]
        public string FormRoolItem2 { get; set; }

        [Display(Name = "Value2", Order = 10, ResourceType = typeof(Uco.Models.Resources.SystemModels))]
        public string FormRoolValue2 { get; set; }

        [Display(Name = "Value3", Order = 10, ResourceType = typeof(Uco.Models.Resources.SystemModels))]
        public string FormRoolValue3 { get; set; }

        [Display(Name = "Item3", Order = 10, ResourceType = typeof(Uco.Models.Resources.SystemModels))]
        public string FormRoolItem3 { get; set; }

        [Display(Name = "RoleDefault", Order = 10, ResourceType = typeof(Uco.Models.Resources.SystemModels))]
        [UIHint("RoleDefault")]
        public string FormRoolRole { get; set; }

        public FormRool()
        {
            this.FormRoolRole = "Admin";
        }
    }

    [NotMapped]
    public class ImageGalleryItem
    {
        public int Order { get; set; }
        public string Title { get; set; }
        public string BigImageUrl { get; set; }
        public string SmallImageUrl { get; set; }
        public string FileName
        {
            get
            {
                if (string.IsNullOrEmpty(this.BigImageUrl)) return string.Empty;
                return this.BigImageUrl.Split('/').ToList().LastOrDefault();
            }
        }

        public ImageGalleryItem()
        {
            this.Order = 100;
        }
    }
}
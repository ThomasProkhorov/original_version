using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Configuration;
using System.Web.Mvc;
using Uco.Infrastructure;
using Uco.Infrastructure.Repositories;
using System.Linq;
using System.Data.Entity;
using System.Web;
using Uco.Infrastructure.Livecycle;

namespace Uco.Models
{
    public abstract class AbstractPage
    {
        [HiddenInput(DisplayValue = false)]
        [Key]
        public int ID { get; set; }
        [Display(Name = "Title", Order = 100, ResourceType = typeof(Uco.Models.Resources.SystemModels), Prompt = "TabContent")]
        [Required(ErrorMessageResourceType = typeof(Uco.Models.Resources.SystemModels), ErrorMessageResourceName = "TitleRequired")]
        public virtual string Title { get; set; }
        [Display(Name = "Text", Order = 150, ResourceType = typeof(Uco.Models.Resources.SystemModels), Prompt = "TabContent")]
        [UIHint("Html")]
        [AllowHtml]
        public virtual string Text { get; set; }

        [Display(Name = "Layout", Order = 200, ResourceType = typeof(Uco.Models.Resources.SystemModels), Prompt = "TabSettings")]
        [UIHint("Layouts")]
        public virtual string Layout { get; set; }
        [Display(Name = "Order", Order = 210, ResourceType = typeof(Uco.Models.Resources.SystemModels), Prompt = "TabSettings")]
        public int Order { get; set; }
        [Display(Name = "Visible", Order = 220, ResourceType = typeof(Uco.Models.Resources.SystemModels), Prompt = "TabSettings")]
        public bool Visible { get; set; }
        [Display(Name = "ShowInMenu", Order = 230, ResourceType = typeof(Uco.Models.Resources.SystemModels), Prompt = "TabSettings")]
        public bool ShowInMenu { get; set; }
        [Display(Name = "ShowInSitemap", Order = 240, ResourceType = typeof(Uco.Models.Resources.SystemModels), Prompt = "TabSettings")]
        public bool ShowInSitemap { get; set; }
        [HiddenInput(DisplayValue = false)]
        public bool ShowInAdminMenu { get; set; }

        [Display(Name = "CreateTime", Order = 260, ResourceType = typeof(Uco.Models.Resources.SystemModels), Prompt = "TabSettings")]
        [UIHint("DateTime")]
        [Column(TypeName = "datetime2")]
        public DateTime CreateTime { get; set; }
        [Display(Name = "ChangeTime", Order = 270, ResourceType = typeof(Uco.Models.Resources.SystemModels), Prompt = "TabSettings")]
        [UIHint("DateTime")]
        [Column(TypeName = "datetime2")]
        public DateTime ChangeTime { get; set; }

        [Display(Name = "SeoUrlName", Order = 300, ResourceType = typeof(Uco.Models.Resources.SystemModels), Prompt = "TabSeo")]
        [MaxLength(4000)]
        [RegularExpression("^[a-zA-Zא-ת0-9-]+$", ErrorMessageResourceType = typeof(Uco.Models.Resources.SystemModels), ErrorMessageResourceName = "SeoUrlNameError")]
        public string SeoUrlName { get; set; }
        [Display(Name = "SeoTitle", Order = 310, ResourceType = typeof(Uco.Models.Resources.SystemModels), Prompt = "TabSeo")]
        public string SeoTitle { get; set; }
        [Display(Name = "SeoH1", Order = 320, ResourceType = typeof(Uco.Models.Resources.SystemModels), Prompt = "TabSeo")]
        public string SeoH1 { get; set; }
        [Display(Name = "SeoInLinkName", Order = 330, ResourceType = typeof(Uco.Models.Resources.SystemModels), Prompt = "TabSeo")]
        public string SeoInLinkName { get; set; }
        [Display(Name = "SeoKywords", Order = 340, ResourceType = typeof(Uco.Models.Resources.SystemModels), Prompt = "TabSeo")]
        [DataType(DataType.MultilineText)]
        [MaxLength(4000)]
        public string SeoKywords { get; set; }
        [Display(Name = "SeoDescription", Order = 340, ResourceType = typeof(Uco.Models.Resources.SystemModels), Prompt = "TabSeo")]
        [DataType(DataType.MultilineText)]
        [MaxLength(4000)]
        public string SeoDescription { get; set; }

        [Display(Name = "PermissionsView", Order = 340, ResourceType = typeof(Uco.Models.Resources.SystemModels), Prompt = "TabPermissions")]
        [UIHint("PermissionsView")]
        public string PermissionsView { get; set; }

        [Display(Name = "PermissionsEdit", Order = 340, ResourceType = typeof(Uco.Models.Resources.SystemModels), Prompt = "TabPermissions")]
        [UIHint("PermissionsEdit")]
        public string PermissionsEdit { get; set; }

        [Display(Name = "PermissionsUpdateChildPages", Order = 340, ResourceType = typeof(Uco.Models.Resources.SystemModels), Prompt = "TabPermissions")]
        public bool PermissionsUpdateChildPages { get; set; }

        [Display(Name = "RedirectTo", Order = 270, ResourceType = typeof(Uco.Models.Resources.SystemModels), Prompt = "TabSettings")]
        public virtual string RedirectTo { get; set; }

        [Display(Name = "ParentID", ResourceType = typeof(Uco.Models.Resources.SystemModels))]
        [HiddenInput(DisplayValue = false)]
        public int ParentID { get; set; }

        [HiddenInput(DisplayValue = false)]
        [MaxLength(10)]
        public virtual string RouteUrl { get; set; }

        [NotMapped]
        [HiddenInput(DisplayValue = false)]
        public virtual string Url
        {
            get
            {
                return "~/" + SF.GetLangRoute(this.LanguageCode) + this.RouteUrl + "/" + this.SeoUrlName;
            }
        }

        [Display(Name = "Link", Order = 200, ResourceType = typeof(Uco.Models.Resources.SystemModels), Prompt = "TabSettings")]
        [UIHint("Link")]
        [NotMapped]
        public string Link
        {
            get
            {
                return this.Url;
            }
        }

        //Added Page Name and PageIcon by Jit
        [Display(Prompt = "Hidden")]
        public virtual string PageName { get; set; }

        [Display(Prompt = "Hidden")]
        public virtual string PageIcon { get; set; }


        [Display(Prompt = "Hidden")]
        public virtual string LanguageCode { get; set; }

        [Display(Prompt = "Hidden")]
        public virtual string ShortDescription { get; set; }
        [Display(Prompt = "Hidden")]
        public virtual string Pic { get; set; }
        [Display(Prompt = "Hidden")]
        public virtual string Text2 { get; set; }
        [Display(Prompt = "Hidden")]
        public virtual string Text3 { get; set; }

        //XML
        [HiddenInput(DisplayValue = false)]
        [AllowHtml]
        [Column(TypeName = "xml")]
        public virtual string XML1 { get; set; }

        public List<T> GetDataFromXML1<T>()
        {
            if (string.IsNullOrEmpty(this.XML1)) return new List<T>();
            System.Xml.Serialization.XmlSerializer x = new System.Xml.Serialization.XmlSerializer(typeof(List<T>));
            return x.Deserialize(new System.IO.StringReader(this.XML1)) as List<T>;
        }
        public void SetDataToXML1<T>(List<T> DataToXML)
        {
            if (DataToXML == null) return;
            System.Xml.Serialization.XmlSerializer x = new System.Xml.Serialization.XmlSerializer(DataToXML.GetType());
            System.IO.StringWriter sw = new System.IO.StringWriter();
            x.Serialize(sw, DataToXML);
            this.XML1 = sw.ToString();
        }

        //XML
        [HiddenInput(DisplayValue = false)]
        [AllowHtml]
        [Column(TypeName = "xml")]
        public virtual string XML2 { get; set; }

        public List<T> GetDataFromXML2<T>()
        {
            if (string.IsNullOrEmpty(this.XML2)) return new List<T>();
            System.Xml.Serialization.XmlSerializer x = new System.Xml.Serialization.XmlSerializer(typeof(List<T>));
            return x.Deserialize(new System.IO.StringReader(this.XML2)) as List<T>;
        }
        public void SetDataToXML2<T>(List<T> DataToXML)
        {
            if (DataToXML == null) return;
            System.Xml.Serialization.XmlSerializer x = new System.Xml.Serialization.XmlSerializer(DataToXML.GetType());
            System.IO.StringWriter sw = new System.IO.StringWriter();
            x.Serialize(sw, DataToXML);
            this.XML2 = sw.ToString();
        }

        [NotMapped]
        public List<string> RolesViewList
        {
            get
            {
                return SF.RolesStringToList(this.PermissionsView);
            }
        }

        [NotMapped]
        public List<string> RolesEditList
        {
            get
            {
                return SF.RolesStringToList(this.PermissionsEdit);
            }
        }

        public virtual void BeforeCreate(int ParentID)
        {
            AbstractPage Parent = LS.CurrentEntityContext.AbstractPages.FirstOrDefault(r => r.ID == ParentID);
            if (Parent == null)
            {
                List<string> GetRoleListView = SF.GetRoleList();
                List<string> GetRoleListEdit = SF.GetRoleList();
                GetRoleListView.Remove("Admin");
                GetRoleListEdit.Remove("Admin");
                GetRoleListEdit.Remove("Anonymous");
                this.PermissionsView = SF.RolesListToString(GetRoleListView);
                this.PermissionsEdit = SF.RolesListToString(GetRoleListEdit);
            }
            else
            {
                this.PermissionsView = Parent.PermissionsView;
                this.PermissionsEdit = Parent.PermissionsEdit;
            }
        }

        public virtual void OnCreate()
        {
            if (SF.UsePermissions())
            {
                List<string> PermissionsView = new List<string>();
                PermissionsView.Add("Admin");
                if (LS.CurrentHttpContext.Request["PermissionsView"] != null)
                {
                    PermissionsView.AddRange(LS.CurrentHttpContext.Request.Form.GetValues("PermissionsView").ToList());
                }
                this.PermissionsView = SF.RolesListToString(PermissionsView);

                List<string> PermissionsEdit = new List<string>();
                PermissionsEdit.Add("Admin");
                if (LS.CurrentHttpContext.Request["PermissionsEdit"] != null)
                {
                    PermissionsEdit.AddRange(LS.CurrentHttpContext.Request.Form.GetValues("PermissionsEdit").ToList());
                }
                this.PermissionsEdit = SF.RolesListToString(PermissionsEdit);
            }
        }

        public virtual void OnCreated()
        {
            List<string> c = this.RolesEditList;

        }

        public virtual void OnCreateChild() { }

        public virtual void OnEdit()
        {

            if (SF.UsePermissions())
            {
                if (LS.CurrentUser.IsInRole("Admin"))
                {
                    List<string> PermissionsView = new List<string>();
                    PermissionsView.Add("Admin");
                    if (LS.CurrentHttpContext.Request["PermissionsView"] != null)
                    {
                        PermissionsView.AddRange(LS.CurrentHttpContext.Request.Form.GetValues("PermissionsView").ToList());
                    }
                    this.PermissionsView = SF.RolesListToString(PermissionsView);

                    List<string> PermissionsEdit = new List<string>();
                    PermissionsEdit.Add("Admin");
                    if (LS.CurrentHttpContext.Request["PermissionsEdit"] != null)
                    {
                        PermissionsEdit.AddRange(LS.CurrentHttpContext.Request.Form.GetValues("PermissionsEdit").ToList());
                    }
                    this.PermissionsEdit = SF.RolesListToString(PermissionsEdit);
                }
                else
                {
                    AbstractPage OldPage = LS.CurrentEntityContext.AbstractPages.FirstOrDefault(r => r.ID == ID);
                    LS.CurrentEntityContext.Entry(OldPage).State = EntityState.Detached;
                    this.PermissionsView = OldPage.PermissionsView;
                    this.PermissionsEdit = OldPage.PermissionsEdit;

                }

            }
        }

        public virtual void OnEdited()
        {
            if (SF.UsePermissions() && this.PermissionsUpdateChildPages)
            {
                if (LS.CurrentUser.IsInRole("Admin"))
                {
                    SF.UpdateChildPermissions(this.PermissionsView, this.PermissionsEdit, this.ID);
                }
            }
        }

        public virtual void OnDelete() { }

        public virtual void OnDeleted() { }

        public virtual void OnMove() { }

        public virtual void OnMoved() { }

        public virtual void OnCopy() { }

        public virtual void OnCopyed() { }

        [HiddenInput(DisplayValue = false)]
        public int DomainID { get; set; }

        public AbstractPage()
        {
            Order = 100;
            Layout = "_Layout.cshtml";
            Visible = true;
            ShowInMenu = true;
            ShowInSitemap = true;
            ShowInAdminMenu = true;
            CreateTime = DateTime.Now;
            ChangeTime = DateTime.Now;
            PermissionsUpdateChildPages = true;
            LanguageCode = SF.GetLangCodeWebconfig();
        }
    }

    [RestrictParents(new string[] { })]
    [RouteUrl("d")]
    [PageIcon("~/Areas/Admin/Content/pages/route_d.png")]
    [PageName("DomainPage", ResourceType = typeof(Uco.Models.Resources.SystemModels))]
    public class DomainPage : AbstractPage
    {
        [HiddenInput(DisplayValue = false)]
        public override string RouteUrl { get { return "d"; } set { } }

        [NotMapped]
        [HiddenInput(DisplayValue = false)]
        public override string Url
        {
            get
            {
                return "~/";
            }
        }

        public DomainPage()
        {

        }
    }

    [RestrictParents(new string[] { "DomainPage" })]
    [RouteUrl("l")]
    [PageIcon("~/Areas/Admin/Content/pages/route_l.png")]
    [PageName("LanguagePage", ResourceType = typeof(Uco.Models.Resources.SystemModels))]
    public class LanguagePage : AbstractPage
    {
        [HiddenInput(DisplayValue = false)]
        public override string RouteUrl { get { return "l"; } set { } }

        [Display(Name = "LanguageCode", Order = 900, ResourceType = typeof(Uco.Models.Resources.SystemModels), Prompt = "TabDomain")]
        [UIHint("Languages")]
        public override string LanguageCode { get; set; }

        [Display(Name = "HeaderHtml", Order = 900, ResourceType = typeof(Uco.Models.Resources.SystemModels), Prompt = "TabDomain")]
        [DataType(DataType.MultilineText)]
        [MaxLength(4000)]
        [AllowHtml]
        public override string Text2 { get; set; }

        [Display(Name = "FotterHtml", Order = 900, ResourceType = typeof(Uco.Models.Resources.SystemModels), Prompt = "TabDomain")]
        [DataType(DataType.MultilineText)]
        [MaxLength(4000)]
        [AllowHtml]
        public override string Text3 { get; set; }

        public override void OnEdited()
        {
            SF.SetLanguageCode(this, 100, this.LanguageCode);

            base.OnEdited();
        }

        public LanguagePage()
        {

        }
    }

    [RestrictParents(new string[] { "DomainPage", "LanguagePage", "RedirectPage", "ContentPage" })]
    [RouteUrl("m")]
    [PageIcon("~/Areas/Admin/Content/pages/route_m.png")]
    [PageName("SiteMapPage", ResourceType = typeof(Uco.Models.Resources.SystemModels))]
    public class SiteMapPage : AbstractPage
    {
        [HiddenInput(DisplayValue = false)]
        public override string RouteUrl { get { return "m"; } set { } }
    }

    [RestrictParents(new string[] { "DomainPage", "LanguagePage", "RedirectPage", "ContentPage" })]
    [RouteUrl("arl")]
    [PageIcon("~/Areas/Admin/Content/pages/route_arl.png")]
    [PageName("ArticleListPage", ResourceType = typeof(Uco.Models.Resources.SystemModels))]
    public class ArticleListPage : AbstractPage
    {
        [HiddenInput(DisplayValue = false)]
        public override string RouteUrl { get { return "arl"; } set { } }
    }

    [RestrictParents(new string[] { "ArticleListPage" })]
    [RouteUrl("a")]
    [PageIcon("~/Areas/Admin/Content/pages/route_a.png")]
    [PageName("ArticlePage", ResourceType = typeof(Uco.Models.Resources.SystemModels))]
    public class ArticlePage : AbstractPage
    {
        [Display(Name = "ShortDescription", Order = 115, ResourceType = typeof(Uco.Models.Resources.SystemModels), Prompt = "TabContent")]
        [Column("ShortDescription", TypeName = "nvarchar(MAX)")]
        [DataType(DataType.MultilineText)]
        public override string ShortDescription { get; set; }

        [Display(Name = "Pic", Order = 115, ResourceType = typeof(Uco.Models.Resources.SystemModels), Prompt = "TabContent")]
        [UIHint("Image")]
        public override string Pic { get; set; }

        [HiddenInput(DisplayValue = false)]
        public override string RouteUrl { get { return "a"; } set { } }
    }

    [RestrictParents(new string[] { "DomainPage", "LanguagePage", "RedirectPage", "ContentPage" })]
    [RouteUrl("f")]
    [PageIcon("~/Areas/Admin/Content/pages/route_f.png")]
    [PageName("FormPage", ResourceType = typeof(Uco.Models.Resources.SystemModels))]
    public class FormPage : AbstractPage
    {
        [Display(Name = "RoleDefault", Order = 110, Prompt = "TabContent", ResourceType = typeof(Uco.Models.Resources.SystemModels))]
        [UIHint("RoleDefault")]
        public string RoleDefault { get; set; }

        [Display(Name = "FormTextAfter", Order = 300, Prompt = "TabTextAfter", ResourceType = typeof(Uco.Models.Resources.SystemModels))]
        [UIHint("Html")]
        [AllowHtml]
        [Column(TypeName = "nvarchar(MAX)")]
        public override string Text2 { get; set; }

        [Display(Name = "FormTextEmail", Order = 300, Prompt = "TabTextAfter", ResourceType = typeof(Uco.Models.Resources.SystemModels))]
        [UIHint("Html")]
        [AllowHtml]
        [Column(TypeName = "nvarchar(MAX)")]
        public override string Text3 { get; set; }

        [NotMapped]
        public IEnumerable<FormField> Fields { get; set; }

        [NotMapped]
        public IEnumerable<FormRool> Rools { get; set; }

        public override void OnCreate()
        {
            base.OnCreate();


            if (Fields != null)
            {
                foreach (FormField item in Fields) { if (item.FormFieldID <= 0) item.FormFieldID = Fields.Max(r => r.FormFieldID) + 1; }
                this.SetDataToXML1<FormField>(Fields.OrderBy(r => r.FormFieldOrder).ToList());
            }
            else this.SetDataToXML1<FormField>(new List<FormField>());

            if (Rools != null)
            {
                foreach (FormRool item in Rools) { if (item.FormRoolID <= 0) item.FormRoolID = Rools.Max(r => r.FormRoolID) + 1; }
                this.SetDataToXML2<FormRool>(Rools.OrderBy(r => r.FormRoolOrder).ToList());
            }
            else this.SetDataToXML2<FormRool>(new List<FormRool>());
        }

        public override void OnEdit()
        {
            base.OnEdit();

            if (Fields != null)
            {
                foreach (FormField item in Fields) { if (item.FormFieldID <= 0) item.FormFieldID = Fields.Max(r => r.FormFieldID) + 1; }
                this.SetDataToXML1<FormField>(Fields.OrderBy(r => r.FormFieldOrder).ToList());
            }
            else this.SetDataToXML1<FormField>(new List<FormField>());

            if (Rools != null)
            {
                foreach (FormRool item in Rools) { if (item.FormRoolID <= 0) item.FormRoolID = Rools.Max(r => r.FormRoolID) + 1; }
                this.SetDataToXML2<FormRool>(Rools.OrderBy(r => r.FormRoolOrder).ToList());
            }
            else this.SetDataToXML2<FormRool>(new List<FormRool>());
        }

        [HiddenInput(DisplayValue = false)]
        public override string RouteUrl { get { return "f"; } set { } }
    }

    [RestrictParents(new string[] { "DomainPage", "LanguagePage", "RedirectPage", "ContentPage" })]
    [RouteUrl("nwl")]
    [PageIcon("~/Areas/Admin/Content/pages/route_nwl.png")]
    [PageName("NewsListPage", ResourceType = typeof(Uco.Models.Resources.SystemModels))]
    public class NewsListPage : AbstractPage
    {
        [HiddenInput(DisplayValue = false)]
        public override string RouteUrl { get { return "nwl"; } set { } }
    }

    [RestrictParents(new string[] { "NewsListPage" })]
    [RouteUrl("n")]
    [PageIcon("~/Areas/Admin/Content/pages/route_n.png")]
    [PageName("NewsPage", ResourceType = typeof(Uco.Models.Resources.SystemModels))]
    public class NewsPage : AbstractPage
    {
        [Display(Name = "ShortDescription", Order = 105, ResourceType = typeof(Uco.Models.Resources.SystemModels), Prompt = "TabContent")]
        [Column("ShortDescription", TypeName = "nvarchar(MAX)")]
        [DataType(DataType.MultilineText)]
        public override string ShortDescription { get; set; }

        [HiddenInput(DisplayValue = false)]
        public override string RouteUrl { get { return "n"; } set { } }
    }

    [RestrictParents(new string[] { "DomainPage", "LanguagePage", "RedirectPage", "ContentPage" })]
    [RouteUrl("r")]
    [PageIcon("~/Areas/Admin/Content/pages/route_r.png")]
    [PageName("RedirectPage", ResourceType = typeof(Uco.Models.Resources.SystemModels))]
    public class RedirectPage : AbstractPage
    {
        [Display(Prompt = "Hidden")]
        public override string Text { get; set; }

        [Display(Name = "RedirectTo", Order = 104, ResourceType = typeof(Uco.Models.Resources.SystemModels), Prompt = "Content")]
        [Required()]
        public override string RedirectTo { get; set; }

        [HiddenInput(DisplayValue = false)]
        public override string RouteUrl { get { return "r"; } set { } }
    }

    [RestrictParents(new string[] { "DomainPage", "LanguagePage", "RedirectPage", "ContentPage" })]
    [RouteUrl("c")]
    [PageIcon("~/Areas/Admin/Content/pages/route_c.png")]
    [PageName("ContentPage", ResourceType = typeof(Uco.Models.Resources.SystemModels))]
    public class ContentPage : AbstractPage
    {
        [HiddenInput(DisplayValue = false)]
        public override string RouteUrl { get { return "c"; } set { } }
    }

}


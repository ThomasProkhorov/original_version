using System;
using System.Web;
using Uco.Infrastructure;

namespace Uco.Models
{
    public class CustomMenuItem
    {
        public CustomMenuItem()
        {
        }
        public int ID { get; set; }
        public int ParentID { get; set; }
        public string Title { get; set; }

        public int Level { get; set; }
        public bool IsLangRoot { get; set; }

        public string LangCode { get; set; }
        public string RouteUrl { get; set; }
        public string SeoUrlName { get; set; }

        public DateTime ChangeTime { get; set; }

        public bool Visible { get; set; }
        public bool ShowInMenu { get; set; }
        public bool ShowInSitemap { get; set; }
        public bool ShowInAdminMenu { get; set; }

        public string RedirectTo { get; set; }

        public string Url
        {
            get
            {
                if (!string.IsNullOrEmpty(this.RedirectTo)) return this.RedirectTo;

                if (this.LangCode == null || this.LangCode == SF.GetLangCodeWebconfig()) return VirtualPathUtility.ToAbsolute("~/" + this.RouteUrl + "/" + this.SeoUrlName);
                else return VirtualPathUtility.ToAbsolute("~/" + this.LangCode + "/" + this.RouteUrl + "/" + this.SeoUrlName);

            }
        }

        public string Image
        {
            get
            {
                string PageIcon = SF.GetPageIcon(this.RouteUrl);
                if (!string.IsNullOrEmpty(PageIcon)) return PageIcon;
                else return "~/Areas/Admin/Content/pages/route_" + this.RouteUrl + ".png";
            }
        }
    }

}
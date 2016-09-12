using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;
using Uco.Infrastructure;
using Uco.Models;

namespace Uco.PluginExample.Models
{
    [AddToEntityModel]
    [RestrictParentsAttribute(new string[] { "DomainPage", "LanguagePage" })]
    [RouteUrlAttribute("ttt")]
    [PageIcon("~/Plugins/Uco.PluginExample/Content/DesignFiles/route_n.png")]
    [PageName("PluginExampleTestPage", ResourceType = typeof(Uco.PluginExample.Models.Resources.Models))]
    public class PluginExampleTestPage : AbstractPage
    {
        [HiddenInput(DisplayValue = false)]
        public override string RouteUrl { get { return "ttt"; } set { } }

        [Display(Name = "PluginExampleData", Order = 105, ResourceType = typeof(Uco.PluginExample.Models.Resources.Models), Prompt = "TabContent")]
        public string PluginExampleData { get; set; }
    }
}
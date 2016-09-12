using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;
using Uco.Infrastructure;

namespace Uco.PluginExample.Models
{
    [AddToEntityModel]
    public class PluginExampleSomeData
    {
        [Key]
        [HiddenInput(DisplayValue = false)]
        public int ID { get; set; }
        [Display(Name = "SystemName", ResourceType = typeof(Uco.Models.Resources.SystemModels))]
        public string SystemName { get; set; }

        [HiddenInput(DisplayValue = false)]
        public int DomainID { get; set; }

        [Display(Name = "Text", ResourceType = typeof(Uco.Models.Resources.SystemModels))]
        [UIHint("Html")]
        [AllowHtml]
        public string Text { get; set; }
    }
}
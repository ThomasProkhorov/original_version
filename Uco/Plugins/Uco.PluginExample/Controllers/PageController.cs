using System.Web.Mvc;
using Uco.Models;

namespace Uco.PluginExample.Controllers
{
    public class PageController : BasePageController
    {
        public ActionResult PluginExampleTestPage()
        {
            return View(CurrentPage);
        }

    }
}

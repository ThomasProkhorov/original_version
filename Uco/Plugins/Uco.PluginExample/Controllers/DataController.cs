using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using Uco.Models;
using Uco.PluginExample.Models;

namespace Uco.PluginExample.Controllers
{
    public class DataController : BasePluginDataController
    {
        public ActionResult Index()
        {
            PluginExampleSomeData t = new PluginExampleSomeData();
            t.SystemName = "dd";
            t.DomainID = 0;
            t.Text = "rr";

            _db.Set<PluginExampleSomeData>().Add(t);
            _db.SaveChanges();

            List<PluginExampleSomeData> l = _db.Set<PluginExampleSomeData>().ToList();
            return View(l);
        }

        [ChildActionOnly]
        public ActionResult _TestAction()
        {
            return View();
        }
    }
}

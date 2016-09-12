using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Uco.Infrastructure;
using Uco.Models;
using System.Data;
using System.Data.Entity;
using Uco.PluginExample.Models;

namespace Uco.PluginExample.Areas.Admin.Controllers
{
    public class TestController : BaseAdminController
    {
        #region Index
        public ActionResult Index()
        {
            return View();
        }
        #endregion

    }
}

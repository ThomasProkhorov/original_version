using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Kendo.Mvc.UI;
using Kendo.Mvc.Extensions;
using Uco.Infrastructure;
using Uco.Models;
using System.Data.Entity;
using System.IO;
using System.Text;
using System.Linq.Expressions;
using Uco.Infrastructure.Livecycle;
using Uco.Infrastructure.HtmlHelpers;

namespace Uco.Areas.Admin.Controllers
{
    [AuthorizeAdmin]
    public class GenTestController : BaseAdminController
    {
        public ActionResult Index()
        {
            ViewBag.MessageRed = TempData["MessageRed"];
            ViewBag.MessageYellow = TempData["MessageYellow"];
            ViewBag.MessageGreen = TempData["MessageGreen"];

            return View();
        }



    }

}

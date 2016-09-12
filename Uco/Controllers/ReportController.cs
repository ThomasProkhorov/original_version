using System.Web.Mvc;
using Uco.Infrastructure;
using Uco.Models;
using System.Linq;
using System.Collections.Generic;
using Uco.Infrastructure.Livecycle;
using Uco.Models.Overview;
using Kendo.Mvc.UI;
using Kendo.Mvc.Extensions;
using Kendo.Mvc;
using Uco.Infrastructure.Repositories;
using System;
using System.Web.Security;
using System.Threading.Tasks;
using MongoDB.Bson;
using MongoDB.Driver;
using MongoDB.Driver.Core;
using Uco.Models.Mongo;
using Uco.Infrastructure.Services;

namespace Uco.Controllers
{


    [Localization]
    public partial class ReportController : BaseController
    {
        public async Task<ActionResult> Index()
        {


            return Content("Events test");
        }


    }
}

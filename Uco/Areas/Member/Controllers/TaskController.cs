using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Uco.Infrastructure.Tasks;
using Uco.Models;

namespace Uco.Areas.Member.Controllers
{
    [Authorize(Roles = "Member")]
    public class TaskController : BaseMemberController
    {
        // GET: Member/Task
        public ActionResult RunCategoryUpdate()
        {
            var task = new CategoryHideTask();
            task.Execute();
            return View();
        }
    }
}
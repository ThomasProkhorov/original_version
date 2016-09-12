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
using System.Reflection;
using Uco.Infrastructure.Livecycle;
using System.Collections;

namespace Uco.Areas.Member.Controllers
{
    /// <summary>
    /// Generic controller, used for model from namespace Uco.Models
    /// </summary>
    /// <typeparam name="T">Entity from namespace Uco.Models</typeparam>
    [Authorize(Roles = "Member")]
    public class GenericController<T> : BaseMemberGemericController<T> where T : class
    {

    }
}

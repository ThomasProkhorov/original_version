using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Kendo.Mvc.UI;
using Kendo.Mvc.Extensions;
using Uco.Models;
using System.Data;
using System.Web.Security;
using Uco.Infrastructure;
using System.Data.Entity;

namespace Uco.Areas.Admin.Controllers
{
    [AuthorizeAdmin]
    public class UserController : BaseAdminController
    {
        public ActionResult Index()
        {
            ViewBag.MessageRed = TempData["MessageRed"];
            ViewBag.MessageYellow = TempData["MessageYellow"];
            ViewBag.MessageGreen = TempData["MessageGreen"];
            return View();
        }

        public ActionResult _AjaxIndex([DataSourceRequest]DataSourceRequest request)
        {
            IQueryable<User> items = _db.Users;
            DataSourceResult result = items.ToDataSourceResult(request);
            return Json(result);
        }

        [AcceptVerbs(HttpVerbs.Post)]
        public ActionResult _AjaxDelete(User item, [DataSourceRequest]DataSourceRequest request)
        {
            if (ModelState.IsValid)
            {
                _db.Users.Remove(_db.Users.First(r => r.ID == item.ID));
                _db.SaveChanges();
                CleanCache.CleanOutputCache();
            }
            return Json(new[] { item }.ToDataSourceResult(request, ModelState));
        }

        public ActionResult Create()
        {
            return View(new Uco.Models.User() { CreationDate = DateTime.Now, LastModified = DateTime.Now });
        }

        [HttpPost]
        public ActionResult Create(User u, List<string> Roles)
        {
            if (ModelState.IsValid)
            {
                if (Roles.Count() == 0)
                {
                    ModelState.AddModelError("Role", "Select role.");
                    return View(u);
                }

                MembershipCreateStatus createStatus;
                MembershipUser newUser = Membership.CreateUser(u.UserName, u.Password, u.Email, "-", "-", u.IsApproved, out createStatus);
                switch (createStatus)
                {
                    case MembershipCreateStatus.Success:
                        User u1 = _db.Users.First(r => r.UserName == u.UserName);
                        u1.Roles = SF.RolesListToString(Roles);
                        u1.FirstName = u.FirstName;
                        u1.LastName = u.LastName;
                        u1.Comment = u.Comment;
                        u1.RoleDefault = u.RoleDefault;
                        _db.Entry(u1).State = EntityState.Modified;
                        _db.SaveChanges();
                        return RedirectToAction("Index");
                    case MembershipCreateStatus.DuplicateUserName:
                        ModelState.AddModelError(string.Empty, "There already exists a user with this username.");
                        break;
                    case MembershipCreateStatus.DuplicateEmail:
                        ModelState.AddModelError(string.Empty, "There already exists a user with this email address.");
                        break;
                    case MembershipCreateStatus.InvalidEmail:
                        ModelState.AddModelError(string.Empty, "There email address you provided in invalid.");
                        break;
                    case MembershipCreateStatus.InvalidAnswer:
                        ModelState.AddModelError(string.Empty, "There security answer was invalid.");
                        break;
                    case MembershipCreateStatus.InvalidPassword:
                        ModelState.AddModelError(string.Empty, "The password you provided is invalid. It must be 6 characters long.");
                        break;
                    default:
                        ModelState.AddModelError(string.Empty, "There was an unknown error; the user account was NOT created.");
                        break;
                }
                return View(u);
            }
            return View(u);
        }

        public ActionResult Edit(Guid id)
        {
            return View(_db.Users.First(r => r.ID == id));
        }
        public ActionResult Orders(Guid id)
        {
            return View(_db.Users.First(r => r.ID == id));
        }
        [HttpPost, FormValueRequired("Save")]
        public ActionResult Edit(Guid id, User u, List<string> Roles)
        {
            if (ModelState.IsValid)
            {
                u.Roles = SF.RolesListToString(Roles);
                _db.Entry(u).State = EntityState.Modified;
                _db.SaveChanges();
                return RedirectToAction("Index");
            }
            u.Roles = SF.RolesListToString(Roles);
            return View(u);
        }
        [HttpPost, FormValueRequired("impressionate")]
        [ValidateInput(false)]
        [ActionName("Edit")]
        public ActionResult ImpressionateUser(Guid ID, User u)
        {
            FormsAuthentication.SetAuthCookie(u.UserName, true);

            return Redirect("/");
        }

        public ActionResult DiscountHistoryAjax([DataSourceRequest]DataSourceRequest request)
        {
            //redirect to generic auto ready action
            return new BaseGenericController<DiscountUsage>()._AjaxRead(request);

        }
    }
}

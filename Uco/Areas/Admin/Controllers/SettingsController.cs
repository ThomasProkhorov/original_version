using System.Configuration;
using System.Data.Entity;
using System.Linq;
using System.Web.Mvc;
using Uco.Infrastructure;
using Uco.Infrastructure.HtmlHelpers;
using Uco.Infrastructure.Livecycle;
using Uco.Infrastructure.Repositories;
using Uco.Models;

namespace Uco.Areas.Admin.Controllers
{
    [AuthorizeAdmin]
    public class SettingsController : BaseAdminController
    {
        public ActionResult Index(int? DomainPageID)
        {
            ViewBag.MessageRed = TempData["MessageRed"];
            ViewBag.MessageYellow = TempData["MessageYellow"];
            ViewBag.MessageGreen = TempData["MessageGreen"];

            if (DomainPageID == 0 || DomainPageID == null)
            {
                DomainPageID = RP.GetAdminCurrentSettingsRepository().DomainPageID;
            }
            Settings s = _db.SettingsAll.FirstOrDefault(r => r.DomainPageID == DomainPageID);
            if (s != null) return View(s);
            else
            {
                s = new Settings();
                s.DomainPageID = (int)DomainPageID;
                _db.SettingsAll.Add(s);
                _db.SaveChanges();
                return View(s);
            }
        }

        [HttpPost]
        public ActionResult Index(int ID, Settings s, int DomainPageID)
        {
            if (ModelState.IsValid)
            {
                if (SF.UseMultiDomain()) s.Domain = "Default";
                _db.Entry(s).State = EntityState.Modified;
                _db.SaveChanges();

                CleanCache.CleanSettingsCache();

                return RedirectToAction("Index", "Main");
            }
            else return View(s);
        }

        [HttpGet]
        public ActionResult AplicationClear()
        {
            return View();
        }

        [HttpPost]
        public ActionResult AplicationClear(bool Memory, bool Search, bool Images, bool Scripts, bool Styles)
        {
            if (LS.CurrentUser.RolesList.Contains("DomainAdmin"))
            {
                if (Memory)
                {
                    CleanCache.CleanOutputCache();
                    CleanCache.CleanSettingsCache();
                }
            }
            else
            {
                if (Memory) CleanCache.RestartApplication();
            }
            if (Search) CleanCache.CleanSearchCache();
            if (Images) CleanCache.CleanImageCache();
            if (Scripts) { ScriptHelper.ClearCache(); }
            if (Styles) { StyleHelper.ClearCache(); }
            return RedirectToAction("AplicationClearDone");
        }

        [HttpGet]
        public ActionResult AplicationClearDone()
        {
            return View();
        }

        [HttpPost]
        public ActionResult _AjaxUserList(string UserName)
        {
            return new JsonResult { Data = new SelectList(_db.Users.ToList(), "UserName", "UserName", UserName) };
        }
    }
}

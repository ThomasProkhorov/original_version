using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using Uco.Infrastructure;
using Uco.Infrastructure.Repositories;
using Uco.Models;

namespace Uco.Controllers
{
    [Localization]
    public partial class PagePartController : BaseController
    {
        #region ChildAction

        [ChildActionOnly]
        [OutputCache(Duration = 3600, VaryByCustom = "LangCodeDomainAndCleanCacheGuid")]
        public ActionResult _FooterScripts()
        {
            object r = RP.GetCurrentSettings().FotterHtml;
            return View(r);
        }

        [ChildActionOnly]
        [OutputCache(Duration = 3600, VaryByCustom = "LangCodeDomainAndCleanCacheGuid")]
        public ActionResult _HeaderScripts()
        {
            object r = RP.GetCurrentSettings().HeaderHtml;
            return View(r);
        }

        [ChildActionOnly]
        [OutputCache(Duration = 3600, VaryByCustom = "LangCodeDomainAndCleanCacheGuid")]
        public ActionResult _TextComponent(string SystemName)
        {
            object r = RP.GetTextComponent(SystemName);
            return View(r);
        }

        [ChildActionOnly]
        public ActionResult _Menu(int maxLevel, int CurrentPageID, string UlID, string UlClass)
        {
            object r = RP.GetMenuFormated(maxLevel, CurrentPageID, UlID, UlClass);
            return View(r);
        }

        [ChildActionOnly]
        public ActionResult _SubMenu(int startLevel, int maxLevel, int CurentPageID)
        {
            object r = RP.GetSubMenuFormated(startLevel, maxLevel, CurentPageID);
            return View(r);
        }

        [ChildActionOnly]
        [OutputCache(Duration = 3600, VaryByCustom = "LangCodeDomainAndCleanCacheGuid")]
        public ActionResult _NewsBox()
        {
            return View(_db.NewsPages.Where(r => r.DomainID == CurrentSettings.ID).OrderByDescending(r => r.CreateTime).ToList());
        }

        [ChildActionOnly]
        public ActionResult _SmallForm(int FormPageID)
        {
            return View();
        }

        [ChildActionOnly]
        public ActionResult _Breadcrumbs(int ID)
        {
            ViewBag.ID = ID;
            Stack<CustomMenuItem> Items = new Stack<CustomMenuItem>();
            CreateBreadcrumbs(Items, ID, 10);

            return View(Items);
        }

        private void CreateBreadcrumbs(Stack<CustomMenuItem> Items, int PageID, int MaxLoop)
        {
            MaxLoop = MaxLoop - 1;
            if (MaxLoop <= 0) return;

            CustomMenuItem Page = RP.GetDisplayMenuRepository().FirstOrDefault(r => r.ID == PageID);
            if (Page == null) return;
            if (Page.RouteUrl == "d" || Page.RouteUrl == "l") return;
            Items.Push(Page);
            CreateBreadcrumbs(Items, Page.ParentID, MaxLoop);
        }

        #endregion

        #region Ajax

        [HttpPost]
        public ActionResult _SmallFormAdd(ContactSmallForm csf, string InvisibleCaptchaValue)
        {
            if (!CaptchaController.IsInvisibleCaptchaValid(InvisibleCaptchaValue))
            {
                ModelState.AddModelError(string.Empty, "Captcha error.");
                SF.LogError("_SmallFormAdd Captcha error");
                return Content("Error: Captcha", "text/html");
            }

            if (ModelState.IsValid)
            {
                Contact c = new Contact();

                c.ContactEmail = csf.ContactEmail;
                c.ContactName = csf.ContactName;
                c.ContactPhone = csf.ContactPhone;

                c.ContactReferal = SF.GetCookie("Referal");
                string UrlReferrer = UcoString.GetUtf8String(Request.UrlReferrer.ToString());
                c.ContactUrl = "<a target='_blank' href='" + UrlReferrer + "'>" + UrlReferrer + "</a>";
                c.ContactDate = DateTime.Now;

                _db.Contacts.Add(c);
                _db.SaveChanges();

                foreach (string item in RP.GetCurrentSettings().AdminEmail.Split(','))
                {
                    if (!SF.isEmail(item.Trim())) continue;

                    _db.OutEmails.Add(new OutEmail
                    {
                        MailTo = item.Trim(),
                        Subject = Uco.Models.Resources.SystemModels.ContactMailSubject
                            .Replace("{ID}", c.ID.ToString())
                            .Replace("{HTTP_HOST}", Request.ServerVariables["HTTP_HOST"]),
                        Body = Uco.Models.Resources.SystemModels.ContactMailBody
                            .Replace("{ID}", c.ID.ToString())
                            .Replace("{HTTP_HOST}", Request.ServerVariables["HTTP_HOST"])
                            .Replace("{ContactName}", c.ContactName)
                            .Replace("{ContactEmail}", c.ContactEmail)
                            .Replace("{ContactPhone}", c.ContactPhone)
                            .Replace("{ContactData}", c.ContactData)
                            .Replace("{ContactReferal}", c.ContactReferal)
                            .Replace("{ContactUrl}", c.ContactUrl),
                        TimesSent = 0,
                        LastTry = DateTime.Now
                    });
                }

                _db.SaveChanges();

                return Content(RP.GetTextComponent("_SmallFormAdd"), "text/html");
            }
            else
            {
                SF.LogError("_SmallFormAdd Model not valid");
                return Content("Error: Model not valid", "text/html");
            }
        }

        [HttpPost]
        public ActionResult _NewsletterAdd(Newsletter n, string InvisibleCaptchaValue)
        {
            if (!CaptchaController.IsInvisibleCaptchaValid(InvisibleCaptchaValue))
            {
                ModelState.AddModelError(string.Empty, "Captcha error.");
                SF.LogError("_NewsletterAdd Captcha error");
                return Content("Error: Captcha", "text/html");
            }

            if (ModelState.IsValid)
            {
                n.NewsletterDate = DateTime.Now;
                _db.Newsletters.Add(n);
                _db.SaveChanges();
                SF.AddToNewsletter(n);

                return Content(RP.GetTextComponent("_NewsletterAdd"), "text/html");
            }
            else
            {
                SF.LogError("_NewsletterAdd Model not valid");
                return Content("Error: Model not valid", "text/html");
            }
        }

        [HttpPost]
        public ActionResult _FormAdd(string InvisibleCaptchaValue, int FormID)
        {
            if (!CaptchaController.IsInvisibleCaptchaValid(InvisibleCaptchaValue))
            {
                ModelState.AddModelError(string.Empty, "Captcha error.");
                SF.LogError("_FormAdd Captcha error");
                return Content("Error: Captcha", "text/html");
            }

            AbstractPage CurrentPage = _db.FormPages.FirstOrDefault(r => r.ID == FormID);
            if (CurrentPage == null)
            {
                ModelState.AddModelError(string.Empty, "CurrentPage error");
                SF.LogError("_FormAdd CurrentPage error");
                return Content("Error: CurrentPage error", "text/html");
            }

            Contact c = new Contact();

            List<FormField> l = CurrentPage.GetDataFromXML1<FormField>().OrderBy(r => r.FormFieldOrder).ToList();
            foreach (Uco.Models.FormField item in l.OrderBy(r => r.FormFieldOrder))
            {
                if (string.IsNullOrEmpty(Request["form_item_" + item.FormFieldID.ToString()])) continue;
                string t = Request["form_item_" + item.FormFieldID.ToString()];

                if (item.FormFieldType == FormField.FormFildType.Name && string.IsNullOrEmpty(c.ContactName)) c.ContactName = t;
                else if (item.FormFieldType == FormField.FormFildType.PhoneNumber && string.IsNullOrEmpty(c.ContactPhone)) c.ContactPhone = t;
                else if (item.FormFieldType == FormField.FormFildType.EmailAddress && string.IsNullOrEmpty(c.ContactEmail)) c.ContactEmail = t;
                else c.ContactData = c.ContactData + item.FormFieldTitle + ": " + t + "<br />";
            }

            c.ContactReferal = SF.GetCookie("Referal");
            c.ContactUrl = "<a target='_blank' href='http://" + Request.ServerVariables["HTTP_HOST"] + Request.UrlReferrer.ToString() + "'>" + Request.UrlReferrer.ToString() + "</a>";
            c.ContactDate = DateTime.Now;

            _db.Contacts.Add(c);
            _db.SaveChanges();

            foreach (string item in RP.GetCurrentSettings().AdminEmail.Split(','))
            {
                if (!SF.isEmail(item.Trim())) continue;

                _db.OutEmails.Add(new OutEmail
                {
                    MailTo = item.Trim(),
                    Subject = Uco.Models.Resources.SystemModels.ContactMailSubject
                        .Replace("{ID}", c.ID.ToString())
                        .Replace("{HTTP_HOST}", Request.ServerVariables["HTTP_HOST"]),
                    Body = Uco.Models.Resources.SystemModels.ContactMailBody
                        .Replace("{ID}", c.ID.ToString())
                        .Replace("{HTTP_HOST}", Request.ServerVariables["HTTP_HOST"])
                        .Replace("{ContactName}", c.ContactName)
                        .Replace("{ContactEmail}", c.ContactEmail)
                        .Replace("{ContactPhone}", c.ContactPhone)
                        .Replace("{ContactData}", c.ContactData)
                        .Replace("{ContactReferal}", c.ContactReferal)
                        .Replace("{ContactUrl}", c.ContactUrl),
                    TimesSent = 0,
                    LastTry = DateTime.Now
                });
            }

            _db.SaveChanges();

            return Content(RP.GetTextComponent("_FormAdd"), "text/html");

        }

        #endregion
    }
}

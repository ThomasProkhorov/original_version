using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web.Mvc;
using System.Web.Security;
using Uco.Infrastructure;
using Uco.Infrastructure.Livecycle;
using Uco.Infrastructure.Repositories;
using Uco.Models;

namespace Uco.Controllers
{



    public class AccountController : BaseController
    {
        public ActionResult LogOn(string returnUrl)
        {
            if (TempData["Error"] != null) { ModelState.AddModelError("", TempData["Error"] as string); }
            ViewBag.ReturnUrl = returnUrl;
            return View();
        }


        [HttpPost]
        public ActionResult SendPasswordAjx(string Model, string InvisibleCaptchaValue, string CaptchaValue)
        {
            if (!CaptchaController.IsCaptchaValid(CaptchaValue) || !CaptchaController.IsInvisibleCaptchaValid(InvisibleCaptchaValue))
            {
                ModelState.AddModelError(string.Empty, RP.S("Errors.Common.CaptchaWrong"));
                return Json(new { result = "error", message = ModelState.Values.Select(x => new { Value = x.Value != null ? x.Value.AttemptedValue : "", errors = x.Errors.Select(y => y.ErrorMessage) }) });

            }

            if (ModelState.IsValid)
            {
                User u = _db.Users.FirstOrDefault(r => r.Email == Model);
                if (u == null)
                {
                    ModelState.AddModelError("", RP.S("Errors.Account.UserNameIncorrect"));
                    return Json(new { result = "error", message = ModelState.Values.Select(x => new { Value = x.Value != null ? x.Value.AttemptedValue : "", errors = x.Errors.Select(y => y.ErrorMessage) }) });

                }
                else
                {
                    var messService = new MessageService(_db);
                    messService.SendUserPasswordEmailToUser(u);
                    ModelState.AddModelError("", RP.S("Info.Account.PasswordSended"));
                    return Json(new { result = "error", message = ModelState.Values.Select(x => new { Value = x.Value != null ? x.Value.AttemptedValue : "", errors = x.Errors.Select(y => y.ErrorMessage) }) });



                }
            }
            return Json(new { result = "error", message = ModelState.Values.Select(x => new { Value = x.Value != null ? x.Value.AttemptedValue : "", errors = x.Errors.Select(y => y.ErrorMessage) }) });

        }
        [HttpPost]
        public ActionResult GoogleAjx(string ID, string Name, string Image, string Email)
        {
            // google 111929910329405075066
            // long   9223372036854775807
            if (!string.IsNullOrEmpty(ID) && ID != "0")
            {
                string email = Email;
                if (string.IsNullOrEmpty(email))
                {
                    email = ID + "@gmail.com.com";

                }


                User uLogin = _db.Users.FirstOrDefault(r => r.GoogleID == ID);
                if (uLogin != null)
                {
                    //login
                    LS.Authorize(uLogin);
                    // var messService = new MessageService(_db);
                    // messService.SendUserRegisterEmailToUser(u);
                    return Json(new { result = "ok", url = "" });
                }
                else
                {
                    //register

                    //generate code
                    var chars = "0123456789abcdefghjklmnopqrstuwvxyzQAZWSXEDCRFVTGBYHNUJMIKLOP@$&%";
                    var random = new Random();
                    var password = new string(
                        Enumerable.Repeat(chars, 8)
                                  .Select(s => s[random.Next(s.Length)])
                                  .ToArray());

                    User uExist = _db.Users.FirstOrDefault(r => r.UserName == email || r.Email == email);
                    if (uExist != null)
                    {
                        uExist.GoogleID = ID;

                        _db.SaveChanges();//update facebook ID
                        //login
                        LS.Authorize(uExist);

                        return Json(new { result = "ok", url = "" });
                    }
                    MembershipCreateStatus createStatus;
                    MembershipUser newUser = Membership.CreateUser(email, password, email, "-", "-", true, out createStatus);

                    if (createStatus != MembershipCreateStatus.Success)
                    {
                        ModelState.AddModelError(string.Empty, RP.T("Account.Register.Error." + createStatus.ToString()).ToString());

                    }
                    else
                    {
                        User u = _db.Users.FirstOrDefault(r => r.UserName == email);
                        u.Roles = SF.RolesStringAdd(u.Roles, "Register");
                        u.FirstName = "";
                        u.LastName = "";
                        if (!string.IsNullOrEmpty(Name))
                        {
                            string[] firstLast = Name.Split(new char[] { ' ' }, 2, StringSplitOptions.RemoveEmptyEntries);
                            if (firstLast.Length > 0)
                            {
                                u.FirstName = firstLast[0];
                            }
                            if (firstLast.Length > 1)
                            {
                                u.LastName = firstLast[1];
                            }
                        }
                        u.RoleDefault = "Register";
                        u.GoogleID = ID;
                        u.IsApproved = true;
                        _db.Entry(u).State = EntityState.Modified;
                        _db.SaveChanges();

                        Newsletter n = new Newsletter()
                        {
                            NewsletterAccept = true,
                            NewsletterDate = DateTime.UtcNow,
                            NewsletterEmail = u.Email,
                            NewsletterName = u.FirstName + " " + u.LastName,
                            RoleDefault = "Register"
                        };
                        _db.Newsletters.Add(n);
                        _db.SaveChanges();
                        SF.AddToNewsletter(n);
                                               
                        
                        if (Membership.ValidateUser(u.UserName, u.Password))
                        {
                            LS.Authorize(u);

                            var messService = new MessageService(_db);
                            messService.SendUserRegisterEmailToUser(u);
                            return Json(new { result = "ok", url = "" });
                        }
                        else
                        {
                            ModelState.AddModelError("", RP.T("Account.Logon.PasswordOrUserIncorrect").ToString());
                        }
                    }
                }
                return Json(new { result = "error", message = ModelState.Values.Select(x => new { Value = x.Value != null ? x.Value.AttemptedValue : "", errors = x.Errors.Select(y => y.ErrorMessage) }) });

            }
            return Json(new { result = "error", message = ModelState.Values.Select(x => new { Value = x.Value != null ? x.Value.AttemptedValue : "", errors = x.Errors.Select(y => y.ErrorMessage) }) });

        }

        [HttpPost]
        public ActionResult FacebookAjx(string token)
        {
            
            try
            {
                

                if (!string.IsNullOrEmpty(token))
                {
                    WebClient client = new WebClient();
                    string JsonResult = client.DownloadString(string.Concat(
                           "https://graph.facebook.com/me?access_token=", token));

                    JObject jsonUserInfo = JObject.Parse(JsonResult);

                    FacebookProfile fp = new FacebookProfile();
                    fp.FacebookUsername = jsonUserInfo.Value<string>("username");
                    fp.FacebookEmail = jsonUserInfo.Value<string>("email");
                    fp.FacebookLocale = jsonUserInfo.Value<string>("locale");
                    fp.FacebookID = jsonUserInfo.Value<long>("id");
                    fp.FacebookToken = token;

                    fp.FacebookName = jsonUserInfo.Value<string>("name");
                    fp.FacebookFirstName = jsonUserInfo.Value<string>("first_name");
                    fp.FacebookLastName = jsonUserInfo.Value<string>("last_name");
                    fp.FacebookLink = jsonUserInfo.Value<string>("link");
                    fp.FacebookGender = jsonUserInfo.Value<string>("gender");
                    fp.FacebookTimezone = jsonUserInfo.Value<int>("timezone");
                    fp.FacebookVerified = jsonUserInfo.Value<bool>("verified");
                    fp.FacebookUpdatedTime = jsonUserInfo.Value<DateTime>("updated_time");

                    User uLogin = _db.Users.FirstOrDefault(r => r.FacebookID == fp.FacebookID);
                    if (uLogin != null)
                    {
                        //login
                        LS.Authorize(uLogin);
                        // var messService = new MessageService(_db);
                        // messService.SendUserRegisterEmailToUser(u);
                        return Json(new { result = "ok", url = "" });
                    }
                    else
                    {
                        //register
                        string email = fp.FacebookEmail;
                        //generate code
                        var chars = "0123456789abcdefghjklmnopqrstuwvxyzQAZWSXEDCRFVTGBYHNUJMIKLOP@$&%";
                        var random = new Random();
                        var password = new string(
                            Enumerable.Repeat(chars, 8)
                                      .Select(s => s[random.Next(s.Length)])
                                      .ToArray());
                        if (string.IsNullOrEmpty(email))
                        {
                            if (!string.IsNullOrEmpty(fp.FacebookUsername))
                            {
                                email = fp.FacebookUsername + "@facebook.com";
                            }
                            else
                            {
                                email = fp.FacebookID.ToString() + "@facebook.com";
                            }
                        }
                        User uExist = _db.Users.FirstOrDefault(r => r.UserName == email);
                        if (uExist != null)
                        {
                            uExist.FacebookID = fp.FacebookID;
                            _db.SaveChanges();//update facebook ID
                            //login
                            LS.Authorize(uExist);

                            return Json(new { result = "ok", url = "" });
                        }
                        MembershipCreateStatus createStatus;
                        MembershipUser newUser = Membership.CreateUser(email, password, email, "-", "-", true, out createStatus);

                        if (createStatus != MembershipCreateStatus.Success)
                        {
                            ModelState.AddModelError(string.Empty, RP.T("Account.Register.Error." + createStatus.ToString()).ToString());

                        }
                        else
                        {
                            User u = _db.Users.FirstOrDefault(r => r.UserName == email);
                            u.Roles = SF.RolesStringAdd(u.Roles, "Register");
                            u.FirstName = fp.FacebookFirstName;
                            u.LastName = fp.FacebookLastName;
                            u.RoleDefault = "Register";
                            u.FacebookID = fp.FacebookID;
                            u.IsApproved = true;
                            _db.Entry(u).State = EntityState.Modified;
                            _db.SaveChanges();

                            Newsletter n = new Newsletter()
                            {
                                NewsletterAccept = true,
                                NewsletterDate = DateTime.UtcNow,
                                NewsletterEmail = u.Email,
                                NewsletterName = u.FirstName + " " + u.LastName,
                                RoleDefault = "Register"
                            };
                            _db.Newsletters.Add(n);
                            _db.SaveChanges();
                            SF.AddToNewsletter(n);

                            if (Membership.ValidateUser(u.UserName, u.Password))
                            {
                                LS.Authorize(u);
                                var messService = new MessageService(_db);

                                messService.SendUserRegisterEmailToUser(u);
                                return Json(new { result = "ok", url = "" });
                            }
                            else
                            {
                                ModelState.AddModelError("", RP.T("Account.Logon.PasswordOrUserIncorrect").ToString());
                            }
                        }
                    }
                    return Json(new { result = "error", json = JsonResult, facebook = fp, message = ModelState.Values.Select(x => new { Value = x.Value != null ? x.Value.AttemptedValue : "", errors = x.Errors.Select(y => y.ErrorMessage) }) });

                }
                return Json(new { result = "error", message = ModelState.Values.Select(x => new { Value = x.Value != null ? x.Value.AttemptedValue : "", errors = x.Errors.Select(y => y.ErrorMessage) }) });
            }
            catch (Exception error)
            {
                SF.LogError(error);
                return Json(new { result = "error", message = error.Message });
            }
        }

        [HttpPost]
        public ActionResult RegAjx(RegisterAjxModel model)
        {
            if (ModelState.IsValid)
            {

                MembershipCreateStatus createStatus;
                MembershipUser newUser = Membership.CreateUser(model.Email, model.Password, model.Email, "-", "-", true, out createStatus);

                if (createStatus != MembershipCreateStatus.Success)
                {
                    ModelState.AddModelError(string.Empty, RP.T("Account.Register.Error." + createStatus.ToString()).ToString());

                }
                else
                {
                    User u = _db.Users.FirstOrDefault(r => r.UserName == model.Email);
                    u.Roles = SF.RolesStringAdd(u.Roles, "Register");
                    u.FirstName = model.FirstName.Trim();
                    u.LastName = model.LastName.Trim();
                    u.RoleDefault = "Register";
                    if (Session["address"] != null)
                    {
                        u.AddressMap = (string)Session["address"];
                    }
                    if (Session["longitude"] != null)
                    {
                        u.Longitude = (decimal)Session["longitude"];
                    }
                    if (Session["latitude"] != null)
                    {
                        u.Latitude = (decimal)Session["latitude"];
                    }

                    u.IsApproved = true;
                    _db.Entry(u).State = EntityState.Modified;
                    _db.SaveChanges();
                    if (model.NewsLetter)
                    {
                        Newsletter n = new Newsletter()
                        {
                            NewsletterAccept = true,
                            NewsletterDate = DateTime.UtcNow,
                            NewsletterEmail = u.Email,
                            NewsletterName = u.FirstName + " " + u.LastName,
                            RoleDefault = "Register"
                        };
                        _db.Newsletters.Add(n);
                        _db.SaveChanges();
                        SF.AddToNewsletter(n);
                    }
                    if (Membership.ValidateUser(u.UserName, u.Password))
                    {
                        LS.Authorize(u);
                        var messService = new MessageService(_db);

                        messService.SendUserRegisterEmailToUser(u);
                        return Json(new { result = "ok", url = "" });
                    }
                    else
                    {
                        ModelState.AddModelError("", RP.T("Account.Logon.PasswordOrUserIncorrect").ToString());
                    }
                }


            }
            return Json(new { result = "error", message = ModelState.Values.Select(x => new { Value = x.Value != null ? x.Value.AttemptedValue : "", errors = x.Errors.Select(y => y.ErrorMessage) }) });

            //for future needed
            //   return Json(new { result = "error", message = new Dictionary<string,List<string>>().Select(x=> new { Value = x.Key,errors=x.Value  }) });
        }

        //LogOnAjx
        [HttpPost]
        public ActionResult LogOnAjx(LogOnModel model, string returnUrl)
        {


            if (ModelState.IsValid)
            {
                if (Membership.ValidateUser(model.UserName, model.Password))
                {
                    var u = _db.Users.FirstOrDefault(x => x.UserName == model.UserName);
                    LS.Authorize(u);
                    bool haveOld = false;
                    if (LS.CurrentHttpContext.Request.Cookies["SALcart"] != null)
                    {
                        haveOld = true;
                    }
                    if (Url.IsLocalUrl(returnUrl) && returnUrl.Length > 1 && returnUrl.StartsWith("/")
                        && !returnUrl.StartsWith("//") && !returnUrl.StartsWith("/\\"))
                    {
                        return Json(new { result = "ok", url = returnUrl, haveOld = haveOld });
                    }
                    else
                    {
                        return Json(new { result = "ok", haveOld = haveOld, url = Url.Action("DomainPage", "Page", new { name = "root" }) });
                        //  eturn RedirectToAction("DomainPage", "Page", new { name = "root" });
                    }
                }
                else
                {
                    ModelState.AddModelError("", RP.T("Account.Logon.PasswordOrUserIncorrect").ToString());
                }
            }
            return Json(new { result = "error", message = ModelState.Values.Select(x => new { Value = x.Value != null ? x.Value.AttemptedValue : "", errors = x.Errors.Select(y => y.ErrorMessage) }) });
            //  return View(model);
        }
        [HttpPost]
        public ActionResult LogOn(LogOnModel model, string returnUrl, string InvisibleCaptchaValue)
        {
            if (!CaptchaController.IsInvisibleCaptchaValid(InvisibleCaptchaValue))
            {
                ModelState.AddModelError(string.Empty, "Captcha error.");
                return View();
            }

            if (ModelState.IsValid)
            {
                if (Membership.ValidateUser(model.UserName, model.Password))
                {
                    var u = _db.Users.FirstOrDefault(x => x.UserName == model.UserName);
                    LS.Authorize(u);

                    if (Url.IsLocalUrl(returnUrl) && returnUrl.Length > 1 && returnUrl.StartsWith("/")
                        && !returnUrl.StartsWith("//") && !returnUrl.StartsWith("/\\"))
                    {
                        return Redirect(returnUrl);
                    }
                    else
                    {
                        return RedirectToAction("DomainPage", "Page", new { name = "root" });
                    }
                }
                else
                {
                    ModelState.AddModelError("", LocalizationHelpers.GetLocalResource("~/Views/Account/LogOn.cshtml", "UsernameIncorrect"));
                }
            }

            return View(model);
        }

        public ActionResult SendPassword()
        {
            return View();
        }
        public ActionResult IsLogined()
        {
            return Json(LS.isLogined(), JsonRequestBehavior.AllowGet);
        }
        [HttpPost]
        public ActionResult SendPassword(string Model, string InvisibleCaptchaValue, string CaptchaValue)
        {
            if (!CaptchaController.IsCaptchaValid(CaptchaValue) || !CaptchaController.IsInvisibleCaptchaValid(InvisibleCaptchaValue))
            {
                ModelState.AddModelError(string.Empty, "Captcha error.");
                return View();
            }

            if (ModelState.IsValid)
            {
                if (Model != null)
                {
                    Model = Model.Trim();
                }
                User u = _db.Users.FirstOrDefault(r => r.Email == Model);
                if (u == null) ModelState.AddModelError("", LocalizationHelpers.GetLocalResource("~/Views/Account/SendPassword.cshtml", "UsernameIncorrect"));
                else
                {
                    // _db.OutEmails.Add(new OutEmail() { MailTo = Model, Subject = LocalizationHelpers.GetLocalResource("~/Views/Account/SendPassword.cshtml", "EmailTitle"), Body = LocalizationHelpers.GetLocalResource("~/Views/Account/SendPassword.cshtml", "EmailBody").Replace("{0}", u.UserName).Replace("{1}", u.Password) });
                    var messService = new MessageService(_db);
                    messService.SendUserPasswordEmailToUser(u);
                    return RedirectToAction("SendPasswordSuccess");
                }
            }
            return View();
        }

        public ActionResult SendPasswordSuccess()
        {
            return View();
        }

        public ActionResult LogOff()
        {
            FormsAuthentication.SignOut();
            LS.DeleteCookie(LS.cookiename);
            LS.DeleteCookie(LS.cookieOldCartname);
            Session.Abandon();
            if (Request.UrlReferrer != null)
            {
                return Redirect(Request.UrlReferrer.ToString());
            }
            return RedirectToAction("DomainPage", "Page", new { name = "root" });
        }

        [Authorize]
        public ActionResult ChangePassword()
        {
            return View();
        }

        [Authorize]
        [HttpPost]
        public ActionResult ChangePassword(ChangePasswordModel model)
        {
            if (ModelState.IsValid)
            {

                // ChangePassword will throw an exception rather
                // than return false in certain failure scenarios.
                bool changePasswordSucceeded;
                try
                {
                    MembershipUser currentUser = Membership.GetUser(User.Identity.Name, true /* userIsOnline */);
                    changePasswordSucceeded = currentUser.ChangePassword(model.OldPassword, model.NewPassword);
                }
                catch (Exception)
                {
                    changePasswordSucceeded = false;
                }

                if (changePasswordSucceeded)
                {
                    return RedirectToAction("ChangePasswordSuccess");
                }
                else
                {
                    ModelState.AddModelError("", "The current password is incorrect or the new password is invalid.");
                }
            }

            // If we got this far, something failed, redisplay form
            return View(model);
        }

        public ActionResult ChangePasswordSuccess()
        {
            return View();
        }

        #region Status Codes
        private static string ErrorCodeToString(MembershipCreateStatus createStatus)
        {
            // See http://go.microsoft.com/fwlink/?LinkID=177550 for
            // a full list of status codes.
            switch (createStatus)
            {
                case MembershipCreateStatus.DuplicateUserName:
                    return "User name already exists. Please enter a different user name.";

                case MembershipCreateStatus.DuplicateEmail:
                    return "A user name for that e-mail address already exists. Please enter a different e-mail address.";

                case MembershipCreateStatus.InvalidPassword:
                    return "The password provided is invalid. Please enter a valid password value.";

                case MembershipCreateStatus.InvalidEmail:
                    return "The e-mail address provided is invalid. Please check the value and try again.";

                case MembershipCreateStatus.InvalidAnswer:
                    return "The password retrieval answer provided is invalid. Please check the value and try again.";

                case MembershipCreateStatus.InvalidQuestion:
                    return "The password retrieval question provided is invalid. Please check the value and try again.";

                case MembershipCreateStatus.InvalidUserName:
                    return "The user name provided is invalid. Please check the value and try again.";

                case MembershipCreateStatus.ProviderError:
                    return "The authentication provider returned an error. Please verify your entry and try again. If the problem persists, please contact your system administrator.";

                case MembershipCreateStatus.UserRejected:
                    return "The user creation request has been canceled. Please verify your entry and try again. If the problem persists, please contact your system administrator.";

                default:
                    return "An unknown error occurred. Please verify your entry and try again. If the problem persists, please contact your system administrator.";
            }
        }
        #endregion
    }
}

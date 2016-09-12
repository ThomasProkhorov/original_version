using System;
using System.Configuration;
using System.Data.Entity.Validation;
using System.Linq;
using System.Net.Mail;
using System.Text;
using System.Web;
using Uco.Infrastructure.Livecycle;
using Uco.Models;

namespace Uco.Infrastructure
{
    public static partial class SF
    {
        #region Errors

        public static void LogDBError(DbEntityValidationException dbEx)
        {
            string FinalError = "";
            foreach (var validationErrors in dbEx.EntityValidationErrors)
            {
                foreach (var validationError in validationErrors.ValidationErrors)
                {
                    string error = "Property: " + validationError.PropertyName + " Error: " + validationError.ErrorMessage;

                    if (ConfigurationManager.AppSettings["LogToEmail"] == "true")
                    {
                        MailMessage mm = new MailMessage(ConfigurationManager.AppSettings["MailFrom"], ConfigurationManager.AppSettings["LogToEmailName"]);
                        mm.Subject =
                            (LS.CurrentHttpContext.Request.ServerVariables["HTTP_HOST"] != null ? LS.CurrentHttpContext.Request.ServerVariables["HTTP_HOST"] : "")
                            + " - "
                            + "Db error";
                        mm.Body = mm.Body + error + "<br />";
                        mm.Body = mm.Body + (LS.CurrentHttpContext.User.Identity.IsAuthenticated == true ? LS.CurrentHttpContext.User.Identity.Name : "") + "<br />";
                        mm.Body = mm.Body + (LS.CurrentHttpContext.Request.PhysicalPath != null ? LS.CurrentHttpContext.Request.PhysicalPath : "") + "<br />";
                        mm.Body = mm.Body + (LS.CurrentHttpContext.Request.UserAgent != null ? LS.CurrentHttpContext.Request.UserAgent : "") + "<br />";
                        mm.Body = mm.Body + (LS.CurrentHttpContext.Request.UserHostAddress != null ? LS.CurrentHttpContext.Request.UserHostAddress : "") + "<br />";
                        mm.Body = mm.Body + (LS.CurrentHttpContext.Request.Url != null ? LS.CurrentHttpContext.Request.Url.AbsoluteUri : "") + "<br />";
                        mm.Body = mm.Body + (LS.CurrentHttpContext.Request.UrlReferrer != null ? LS.CurrentHttpContext.Request.UrlReferrer.AbsoluteUri : "") + "<br />";
                        mm.BodyEncoding = Encoding.UTF8;
                        mm.SubjectEncoding = Encoding.UTF8;
                        mm.IsBodyHtml = true;
                        SmtpClient client = new SmtpClient();
                        client.Send(mm);
                    }

                    if (ConfigurationManager.AppSettings["LogToSql"] == "true")
                    {
                        try
                        {
                            using (Db _db = new Db())
                            {
                                if (_db.Errors.Count() >= 300) return;
                                Error e = new Error();
                                e.Date = DateTime.Now;
                                e.InnerException = error;
                                e.Message = error;
                                if (LS.CurrentHttpContext.User.Identity.IsAuthenticated) e.UserName = LS.CurrentHttpContext.User.Identity.Name;
                                if (LS.CurrentHttpContext.Request.PhysicalPath != null) e.PhysicalPath = LS.CurrentHttpContext.Request.PhysicalPath;
                                if (LS.CurrentHttpContext.Request.UserAgent != null) e.UserAgent = LS.CurrentHttpContext.Request.UserAgent;
                                if (LS.CurrentHttpContext.Request.UserHostAddress != null) e.UserHostAddress = LS.CurrentHttpContext.Request.UserHostAddress;
                                if (LS.CurrentHttpContext.Request.Url != null) e.Url = LS.CurrentHttpContext.Request.Url.AbsoluteUri;
                                if (LS.CurrentHttpContext.Request.UrlReferrer != null) e.UrlReferrer = LS.CurrentHttpContext.Request.UrlReferrer.AbsoluteUri;
                                if (LS.CurrentHttpContext.Request.ServerVariables["HTTP_HOST"] != null) e.Host = LS.CurrentHttpContext.Request.ServerVariables["HTTP_HOST"];
                                _db.Errors.Add(e);
                                _db.SaveChanges();
                            }
                        }
                        catch { }
                    }
                    FinalError = FinalError + error + "<br />";
                }
            }
        }

        public static void LogHttpError(HttpException httpEx)
        {
            if (httpEx.GetHttpCode() != 404) LogError((Exception)httpEx);
        }

        public static void LogError(string Ex)
        {
            LogError(new Exception(Ex));
        }

        public static void LogError(Exception Ex)
        {
            if (ConfigurationManager.AppSettings["LogToEmail"] == "true")
            {
                MailMessage mm = new MailMessage(ConfigurationManager.AppSettings["MailFrom"], ConfigurationManager.AppSettings["LogToEmailName"]);
                mm.Subject =
                    (LS.CurrentHttpContext.Request.ServerVariables["HTTP_HOST"] != null ? LS.CurrentHttpContext.Request.ServerVariables["HTTP_HOST"] : "")
                    + " - "
                    + (Ex.Message != null ? Ex.Message : "");
                mm.Body = mm.Body + (Ex.InnerException != null ? Ex.InnerException.ToString() : "") + "<br />";
                mm.Body = mm.Body + (Ex.Message != null ? Ex.Message : "") + "<br />";
                mm.Body = mm.Body + (Ex.Source != null ? Ex.Source : "") + "<br />";
                mm.Body = mm.Body + (Ex.StackTrace != null ? Ex.StackTrace : "") + "<br />";
                mm.Body = mm.Body + (Ex.TargetSite != null ? Ex.TargetSite.ToString() : "") + "<br />";
                mm.Body = mm.Body + (LS.CurrentHttpContext.User.Identity.IsAuthenticated == true ? LS.CurrentHttpContext.User.Identity.Name : "") + "<br />";
                mm.Body = mm.Body + (LS.CurrentHttpContext.Request.PhysicalPath != null ? LS.CurrentHttpContext.Request.PhysicalPath : "") + "<br />";
                mm.Body = mm.Body + (LS.CurrentHttpContext.Request.UserAgent != null ? LS.CurrentHttpContext.Request.UserAgent : "") + "<br />";
                mm.Body = mm.Body + (LS.CurrentHttpContext.Request.UserHostAddress != null ? LS.CurrentHttpContext.Request.UserHostAddress : "") + "<br />";
                mm.Body = mm.Body + (LS.CurrentHttpContext.Request.Url != null ? LS.CurrentHttpContext.Request.Url.AbsoluteUri : "") + "<br />";
                mm.Body = mm.Body + (LS.CurrentHttpContext.Request.UrlReferrer != null ? LS.CurrentHttpContext.Request.UrlReferrer.AbsoluteUri : "") + "<br />";
                mm.BodyEncoding = Encoding.UTF8;
                mm.SubjectEncoding = Encoding.UTF8;
                mm.IsBodyHtml = true;
                SmtpClient client = new SmtpClient();
                client.Send(mm);
            }

            if (ConfigurationManager.AppSettings["LogToSql"] == "true")
            {
                try
                {
                    using (Db _db = new Db())
                    {
                        if (_db.Errors.Count() >= 300) return;
                        Error e = new Error();
                        e.Date = DateTime.Now;
                        if (Ex.InnerException != null) e.InnerException = Ex.InnerException.ToString();
                        if (Ex.Message != null) e.Message = Ex.Message;
                        if (Ex.Source != null) e.Source = Ex.Source;
                        if (Ex.StackTrace != null) e.StackTrace = Ex.StackTrace;
                        if (Ex.TargetSite != null) e.TargetSite = Ex.TargetSite.ToString();
                        if (LS.CurrentHttpContext.User.Identity.IsAuthenticated) e.UserName = LS.CurrentHttpContext.User.Identity.Name;
                        if (LS.CurrentHttpContext.Request.PhysicalPath != null) e.PhysicalPath = LS.CurrentHttpContext.Request.PhysicalPath;
                        if (LS.CurrentHttpContext.Request.UserAgent != null) e.UserAgent = LS.CurrentHttpContext.Request.UserAgent;
                        if (LS.CurrentHttpContext.Request.UserHostAddress != null) e.UserHostAddress = LS.CurrentHttpContext.Request.UserHostAddress;
                        if (LS.CurrentHttpContext.Request.Url != null) e.Url = LS.CurrentHttpContext.Request.Url.AbsoluteUri;
                        if (LS.CurrentHttpContext.Request.UrlReferrer != null) e.UrlReferrer = LS.CurrentHttpContext.Request.UrlReferrer.AbsoluteUri;
                        if (LS.CurrentHttpContext.Request.ServerVariables["HTTP_HOST"] != null) e.Host = LS.CurrentHttpContext.Request.ServerVariables["HTTP_HOST"];
                        _db.Errors.Add(e);
                        _db.SaveChanges();
                    }
                }
                catch { }
            }
        }

        #endregion
    }
}
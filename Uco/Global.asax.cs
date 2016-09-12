using System;
using System.Configuration;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using Uco.Infrastructure;
using Uco.Infrastructure.Livecycle;
using Uco.Infrastructure.ViewEngine;
using Uco.Models;
using System.Linq;
using Uco.Infrastructure.Repositories;
using System.Data.Entity.Validation;
using System.Data.Entity;
using StackExchange.Profiling.EntityFramework6;
using StackExchange.Profiling;
using System.Collections.Generic;
using Uco.Infrastructure.Providers;


namespace Uco
{
    public class MvcApplication : System.Web.HttpApplication
    {
        private bool DbConnection
        {
            get { if (Application["DbConnection"] == null) return true; return (bool)Application["DbConnection"]; }
            set { Application["DbConnection"] = value; }
        }

        protected void Application_Start()
        {
            if (ConfigurationManager.AppSettings["Profiler"].ToString() == "true")
            {
                MiniProfilerEF6.Initialize();
            }

            //set controller factory for generic controller
            ControllerBuilder.Current.SetControllerFactory(typeof(My_Controller_Factory)); 
            DbConfig.InitDB();
            AreaRegistration.RegisterAllAreas();
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            AttributeConfig.RegisterGlobalAttribute();
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            CustomViewEngine.RegisterViewEngine(ViewEngines.Engines);
            ModelMetadataProviders.Current = new MyLocalizationProvider();
            ModelBinders.Binders.Add(typeof(AbstractPage), new AbstractPageBinder());
            TasksConfig.StartTasks();
            SF.EvaluateDisplayMode();
        }

        protected void Session_Start(object sender, EventArgs e)
        {
            if (Request.UrlReferrer != null) SF.SetCookie("Referal", Request.UrlReferrer.ToString());
        }

        protected void Application_BeginRequest(Object source, EventArgs e)
        {
            if (ConfigurationManager.AppSettings["Profiler"].ToString() == "true")
            {
                MiniProfiler.Start();
            }

            //Application initialisation whth HttpContext
            HttpApplication app = (HttpApplication)source;
            HttpContext context = app.Context;
            //First Request Initialization
            FirstRequestInitialization.Initialize(context);

            //check db connection
            if (DbConnection == false)
            {
                DbConnection = DbConfig.TestConnection();
                if ((bool)DbConnection == false) context.Response.Redirect("~/DbError.html");
            }

            //Start Livecycle session
            var routeData = RouteTable.Routes.GetRouteData(new HttpContextWrapper(context));
            if (routeData != null && routeData.RouteHandler is MvcRouteHandler)
            {
                HttpContext.Current.Items["_EntityContext"] = new Db();
                HttpContext.Current.Items["_HttpContext"] = context;
            }
        }

        protected void Application_PostAuthenticateRequest()
        {
            
            if (LS.CurrentEntityContext != null && HttpContext.Current.User.Identity.IsAuthenticated)
            {
                User CurrentUser = LS.CurrentEntityContext.Users.FirstOrDefault(r => r.UserName == HttpContext.Current.User.Identity.Name);
                if (CurrentUser != null)
                {
                    LS.CurrentEntityContext.Entry(CurrentUser).State = EntityState.Detached;
                    HttpContext.Current.Items["_CurrentUser"] = CurrentUser;
                }
            }
        }

        protected void Application_EndRequest()
        {
            //End entity session
            var entityContext = HttpContext.Current.Items["_EntityContext"] as Db;
            if (entityContext != null)
            {
                entityContext.Dispose();
                HttpContext.Current.Items["_EntityContext"] = null;
            }

            HttpContext.Current.Items["_CurrentUser"] = null;

            if (ConfigurationManager.AppSettings["Profiler"].ToString() == "true")
            {
                MiniProfiler.Stop();
            }
        }

        protected void Application_Error(object sender, EventArgs e)
        {
            Exception objErr = Server.GetLastError().GetBaseException();
            if (objErr is DbEntityValidationException) SF.LogDBError((DbEntityValidationException)objErr);
            else if (objErr is HttpException) SF.LogHttpError((HttpException)objErr);
            else SF.LogError(objErr);
        }

        public override string GetVaryByCustomString(HttpContext context, string arg)
        {
            if (arg == "LangCode")
            {
                return SF.GetLangCodeThreading();
            }
            else if (arg == "CleanCacheGuid")
            {
                string Token = "CleanCacheGuid";
                object o = context.Application[Token];
                if (o == null)
                {
                    o = Guid.NewGuid();
                    context.Application[Token] = o;
                }
                return o.ToString();
            }
            else if (arg == "LangCodeAndCleanCacheGuid")
            {
                string CurrentCulture = SF.GetLangCodeThreading();
                string Token = string.Format("CleanCacheGuid_{0}", CurrentCulture);

                object o = context.Application[Token];
                if (o == null)
                {
                    o = CurrentCulture + Guid.NewGuid();
                    context.Application[Token] = o;
                }
                return o.ToString();
            }
            else if (arg == "LangCodeDomainAndCleanCacheGuid")
            {
                string Domain = SF.GetCurrentDomain();
                string CurrentCulture = SF.GetLangCodeThreading();
                string Token = string.Format("LangCodeDomainAndCleanCacheGuid_{0}_{1}", CurrentCulture, Domain);

                object o = context.Application[Token];
                if (o == null)
                {
                    o = Guid.NewGuid();
                    context.Application[Token] = o;
                }
                return o.ToString();
            }

            return base.GetVaryByCustomString(context, arg);
        }
    }
}
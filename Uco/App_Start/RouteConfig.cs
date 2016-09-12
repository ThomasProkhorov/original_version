using System;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using Uco.Infrastructure;
using Uco.Infrastructure.Livecycle;
using Uco.Infrastructure.Repositories;
using Uco.Models;

namespace Uco
{
    public class RouteConfig
    {
        public static void RegisterRoutes(RouteCollection routes)
        {
            routes.IgnoreRoute("{resource}.axd/{*pathInfo}");
            routes.IgnoreRoute("{*favicon}", new { favicon = @"(.*/)?favicon.ico(/.*)?" });
            routes.IgnoreRoute("{*robotstxt}", new { robotstxt = @"(.*/)?robots.txt(/.*)?" });

            routes.MapRoute("ImagesThumbnail", "Image", new { controller = "Images", action = "GetImage" });
            routes.MapRoute("Captcha", "Image", new { controller = "Captcha", action = "Get" });

            //generic URLs
            routes.MapGenericPathRoute("GenericUrl",
                                       "{generic_se_name}",
                                       new { controller = "Error", action = "Error404" },
                                       new[] { "Uco.Controllers" });

            //main site page
            routes.MapRoute(
               name: "HomePage",
               url: "",
               defaults: new { controller = "Page", action = "DomainPage", name = "root" },
               namespaces: new string[] { "Uco.Controllers" }
           );

            //Uco.dll pages
            foreach (Type item in RP.GetPageTypesReprository())
            {
                routes.MapRoute(
                   name: item.Name,
                   url: SF.GetTypeRouteUrl(item) + "/{name}",
                   defaults: new { controller = "Page", action = item.Name },
                   namespaces: new string[] { "Uco.Controllers" }
               );

                routes.MapRoute(
                   name: item.Name + "_Lang",
                   url: "{lang}/" + SF.GetTypeRouteUrl(item) + "/{name}",
                   defaults: new { controller = "Page", action = item.Name },
                   namespaces: new string[] { "Uco.Controllers" }
                );
            }

            //if (SF.UsePlugins())
            //{

                //plugin dll pages
                //foreach (Type item in RP.GetPlugingsAbstractPageChildClasses())
                //{
                //    string PageTypeName = item.Name;
                //    string PluginNamespace = item.Namespace.Replace(".Models", "");

                //    routes.MapRoute(
                //       name: PageTypeName,
                //       url: SF.GetTypeRouteUrl(item) + "/{name}",
                //       defaults: new { controller = "Page", action = PageTypeName },
                //       namespaces: new string[] { PluginNamespace + ".Controllers" }
                //    );

                //    routes.MapRoute(
                //       name: PageTypeName + "_Lang",
                //       url: "{lang}/" + SF.GetTypeRouteUrl(item) + "/{name}",
                //       defaults: new { controller = "Page", action = PageTypeName },
                //       namespaces: new string[] { PluginNamespace + ".Controllers" }
                //    );
                //}

                ////plugin dll data
                //foreach (string item in RP.GetPluginsReprository())
                //{
                //    string Namespace = item + ".Controllers";
                //    routes.MapRoute(
                //       name: Namespace,
                //       url: item + "/{controller}/{action}/{id}",
                //       defaults: new { id = UrlParameter.Optional },
                //       namespaces: new string[] { Namespace }
                //    );

                //    routes.MapRoute(
                //       name: "lang_" + Namespace,
                //       url: "{lang}/" + item + "/{controller}/{action}/{id}",
                //       defaults: new { id = UrlParameter.Optional },
                //       namespaces: new string[] { Namespace }
                //    );
                //}
            //}

            //Uco.dll data
            routes.MapRoute(
               name: "Default",
               url: "{controller}/{action}/{id}",
               defaults: new { id = UrlParameter.Optional },
               namespaces: new string[] { "Uco.Controllers" }
            );

            routes.MapRoute(
               name: "Default_Lang",
               url: "{lang}/{controller}/{action}/{id}",
               defaults: new { id = UrlParameter.Optional },
               namespaces: new string[] { "Uco.Controllers" }
            );
        }
    }
    public static class GenericPathRouteExtensions
    {
        //Override for localized route
        public static Route MapGenericPathRoute(this RouteCollection routes, string name, string url)
        {
            return MapGenericPathRoute(routes, name, url, null /* defaults */, (object)null /* constraints */);
        }
        public static Route MapGenericPathRoute(this RouteCollection routes, string name, string url, object defaults)
        {
            return MapGenericPathRoute(routes, name, url, defaults, (object)null /* constraints */);
        }
        public static Route MapGenericPathRoute(this RouteCollection routes, string name, string url, object defaults, object constraints)
        {
            return MapGenericPathRoute(routes, name, url, defaults, constraints, null /* namespaces */);
        }
        public static Route MapGenericPathRoute(this RouteCollection routes, string name, string url, string[] namespaces)
        {
            return MapGenericPathRoute(routes, name, url, null /* defaults */, null /* constraints */, namespaces);
        }
        public static Route MapGenericPathRoute(this RouteCollection routes, string name, string url, object defaults, string[] namespaces)
        {
            return MapGenericPathRoute(routes, name, url, defaults, null /* constraints */, namespaces);
        }
        public static Route MapGenericPathRoute(this RouteCollection routes, string name, string url, object defaults, object constraints, string[] namespaces)
        {
            if (routes == null)
            {
                throw new ArgumentNullException("routes");
            }
            if (url == null)
            {
                throw new ArgumentNullException("url");
            }

            var route = new GenericPathRoute(url, new MvcRouteHandler())
            {
                Defaults = new RouteValueDictionary(defaults),
                Constraints = new RouteValueDictionary(constraints),
                DataTokens = new RouteValueDictionary()
            };

            if ((namespaces != null) && (namespaces.Length > 0))
            {
                route.DataTokens["Namespaces"] = namespaces;
            }

            routes.Add(name, route);

            return route;
        }
    }

    /// <summary>
    /// Provides properties and methods for defining a SEO friendly route, and for getting information about the route.
    /// </summary>
    public partial class GenericPathRoute : Route
    {
        #region Constructors

        /// <summary>
        /// Initializes a new instance of the System.Web.Routing.Route class, using the specified URL pattern and handler class.
        /// </summary>
        /// <param name="url">The URL pattern for the route.</param>
        /// <param name="routeHandler">The object that processes requests for the route.</param>
        public GenericPathRoute(string url, IRouteHandler routeHandler)
            : base(url, routeHandler)
        {
        }

        /// <summary>
        /// Initializes a new instance of the System.Web.Routing.Route class, using the specified URL pattern, handler class and default parameter values.
        /// </summary>
        /// <param name="url">The URL pattern for the route.</param>
        /// <param name="defaults">The values to use if the URL does not contain all the parameters.</param>
        /// <param name="routeHandler">The object that processes requests for the route.</param>
        public GenericPathRoute(string url, RouteValueDictionary defaults, IRouteHandler routeHandler)
            : base(url, defaults, routeHandler)
        {
        }

        /// <summary>
        /// Initializes a new instance of the System.Web.Routing.Route class, using the specified URL pattern, handler class, default parameter values and constraints.
        /// </summary>
        /// <param name="url">The URL pattern for the route.</param>
        /// <param name="defaults">The values to use if the URL does not contain all the parameters.</param>
        /// <param name="constraints">A regular expression that specifies valid values for a URL parameter.</param>
        /// <param name="routeHandler">The object that processes requests for the route.</param>
        public GenericPathRoute(string url, RouteValueDictionary defaults, RouteValueDictionary constraints, IRouteHandler routeHandler)
            : base(url, defaults, constraints, routeHandler)
        {
        }

        /// <summary>
        /// Initializes a new instance of the System.Web.Routing.Route class, using the specified URL pattern, handler class, default parameter values, 
        /// constraints,and custom values.
        /// </summary>
        /// <param name="url">The URL pattern for the route.</param>
        /// <param name="defaults">The values to use if the URL does not contain all the parameters.</param>
        /// <param name="constraints">A regular expression that specifies valid values for a URL parameter.</param>
        /// <param name="dataTokens">Custom values that are passed to the route handler, but which are not used to determine whether the route matches a specific URL pattern. The route handler might need these values to process the request.</param>
        /// <param name="routeHandler">The object that processes requests for the route.</param>
        public GenericPathRoute(string url, RouteValueDictionary defaults, RouteValueDictionary constraints, RouteValueDictionary dataTokens, IRouteHandler routeHandler)
            : base(url, defaults, constraints, dataTokens, routeHandler)
        {
        }

        #endregion

        #region Methods

        /// <summary>
        /// Returns information about the requested route.
        /// </summary>
        /// <param name="httpContext">An object that encapsulates information about the HTTP request.</param>
        /// <returns>
        /// An object that contains the values from the route definition.
        /// </returns>
        public override RouteData GetRouteData(HttpContextBase httpContext)
        {
            RouteData data = base.GetRouteData(httpContext);
            if (data != null
                //  && DataSettingsHelper.DatabaseIsInstalled()
                )
            {
                // var urlRecordService = EngineContext.Current.Resolve<IUrlRecordService>();
                var slug = data.Values["generic_se_name"] as string;
                var urlRecord = LS.Get<UrlRecord>().FirstOrDefault(x => x.Slug.ToLower() == slug.ToLower());
                if(urlRecord!= null)
                {

                    data.Values["controller"] = urlRecord.EntityName;
                      data.Values["action"] = "Index";
                      data.Values["ID"] = urlRecord.EntityID;
                      data.Values["SeName"] = urlRecord.Slug;
                }
                //performance optimization.
                //we load a cached verion here. it reduces number of SQL requests for each page load
                //  var urlRecord = urlRecordService.GetBySlugCached(slug);
                //comment the line above and uncomment the line below in order to disable this performance "workaround"
                //var urlRecord = urlRecordService.GetBySlug(slug);


                //  data.Values["controller"] = "Common";
                //   data.Values["action"] = "PageNotFound";
                // return data;

                // data.Values["controller"] = "Product";
                //  data.Values["action"] = "ProductDetails";
                //  data.Values["productid"] = urlRecord.EntityId;
                //  data.Values["SeName"] = urlRecord.Slug;
            }
            return data;
        }

        #endregion
    }
}

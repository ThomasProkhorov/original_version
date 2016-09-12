namespace Uco.Infrastructure.ViewEngine
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Linq;
    using System.Web;
    using System.Web.Hosting;
    using System.Web.Mvc;
    using System.Web.Routing;
    using System.Web.WebPages;
    using Uco.Infrastructure.Livecycle;
    using Uco.Models;

    public abstract class CustomVirtualPathProviderViewEngine : IViewEngine
    {
        // format is ":ViewCacheEntry:{cacheType}:{theme}:{plugin}:{prefix}:{name}:{controllerName}:{areaName}:"
        private const string _cacheKeyFormat = ":ViewCacheEntry:{0}:{1}:{2}:{3}:{4}:{5}:{6}:";
        private const string _cacheKeyPrefix_Master = "Master";
        private const string _cacheKeyPrefix_Partial = "Partial";
        private const string _cacheKeyPrefix_View = "View";
        private static readonly string[] _emptyLocations = new string[0];

        private VirtualPathProvider _vpp;
        private DisplayModeProvider _displayModeProvider;
        protected CustomVirtualPathProviderViewEngine()
        {
            if (LS.CurrentHttpContext == null || LS.CurrentHttpContext.IsDebuggingEnabled)
            {
                ViewLocationCache = DefaultViewLocationCache.Null;
            }
            else
            {
                ViewLocationCache = new DefaultViewLocationCache();
            }
        }

        public string[] AreaMasterLocationFormats { get; set; }

        public string[] AreaPartialViewLocationFormats { get; set; }

        public string[] AreaViewLocationFormats { get; set; }

        public string[] MasterLocationFormats { get; set; }
        public string[] FileExtensions { get; set; }
        public string[] PartialViewLocationFormats { get; set; }

        public string[] ViewLocationFormats { get; set; }

        public IViewLocationCache ViewLocationCache { get; set; }

        protected VirtualPathProvider VirtualPathProvider
        {
            get
            {
                return _vpp ?? (_vpp = HostingEnvironment.VirtualPathProvider);
            }

            set
            {
                _vpp = value;
            }
        }

        public virtual ViewEngineResult FindView(ControllerContext controllerContext, string viewName
            , string masterName, bool useCache)
        {
            if (controllerContext == null)
            {
                throw new ArgumentNullException("controllerContext");
            }

            if (string.IsNullOrEmpty(viewName))
            {
                throw new ArgumentException("Value cannot be null or empty.", "viewName");
            }

            string[] viewLocationsSearched;
            string[] masterLocationsSearched;


            string controllerName = controllerContext.RouteData.GetRequiredString("controller");
            string viewPath = GetPath(controllerContext, ViewLocationFormats, AreaViewLocationFormats, "ViewLocationFormats", viewName, controllerName, _cacheKeyPrefix_View, useCache, out viewLocationsSearched);
            string masterPath = GetPath(controllerContext, MasterLocationFormats, AreaMasterLocationFormats, "MasterLocationFormats", masterName, controllerName, _cacheKeyPrefix_Master, useCache, out masterLocationsSearched);

            if (String.IsNullOrEmpty(viewPath) || (String.IsNullOrEmpty(masterPath) && !String.IsNullOrEmpty(masterName)))
            {
                return new ViewEngineResult(viewLocationsSearched.Union(masterLocationsSearched));
            }

            return new ViewEngineResult(CreateView(controllerContext, viewPath, masterPath), this);
        }

        public virtual ViewEngineResult FindPartialView(ControllerContext controllerContext, string partialViewName, bool useCache)
        {
            if (controllerContext == null)
            {
                throw new ArgumentNullException("controllerContext");
            }

            if (string.IsNullOrEmpty(partialViewName))
            {
                throw new ArgumentException("Value cannot be null or empty.", "partialViewName");
            }

            string[] searched;
            string controllerName = controllerContext.RouteData.GetRequiredString("controller");
            string partialPath = GetPath(controllerContext, PartialViewLocationFormats, AreaPartialViewLocationFormats, "PartialViewLocationFormats", partialViewName, controllerName, _cacheKeyPrefix_Partial, useCache, out searched);

            if (String.IsNullOrEmpty(partialPath))
            {
                return new ViewEngineResult(searched);
            }

            return new ViewEngineResult(CreatePartialView(controllerContext, partialPath), this);
        }

        public virtual void ReleaseView(ControllerContext controllerContext, IView view)
        {
            var disposable = view as IDisposable;

            if (disposable != null)
            {
                disposable.Dispose();
            }
        }

        protected virtual bool FileExists(ControllerContext controllerContext, string virtualPath)
        {
            return VirtualPathProvider.FileExists(virtualPath);
        }

        protected virtual bool? IsValidPath(ControllerContext controllerContext, string virtualPath)
        {
            return null;
        }

        protected abstract IView CreatePartialView(ControllerContext controllerContext, string partialPath);

        protected abstract IView CreateView(ControllerContext controllerContext, string viewPath, string masterPath);

        private static List<ViewLocation> GetViewLocations(IEnumerable<string> viewLocationFormats, IEnumerable<string> areaViewLocationFormats)
        {
            var allLocations = new List<ViewLocation>();

            if (areaViewLocationFormats != null)
            {
                allLocations.AddRange(areaViewLocationFormats.Select(areaViewLocationFormat => new AreaAwareViewLocation(areaViewLocationFormat)));
            }

            if (viewLocationFormats != null)
            {
                allLocations.AddRange(viewLocationFormats.Select(viewLocationFormat => new ViewLocation(viewLocationFormat)));
            }

            return allLocations;
        }

        private static bool IsSpecificPath(string name)
        {
            char c = name[0];

            return (c == '~' || c == '/');
        }

        private static string GetAreaName(RouteData routeData)
        {
            object area;

            if (routeData.DataTokens.TryGetValue("area", out area))
            {
                return area as string;
            }

            return GetAreaName(routeData.Route);
        }

        private static string GetAreaName(RouteBase route)
        {
            IRouteWithArea routeWithArea = route as IRouteWithArea;

            if (routeWithArea != null)
            {
                return routeWithArea.Area;
            }

            Route castRoute = route as Route;

            if (castRoute != null && castRoute.DataTokens != null)
            {
                return castRoute.DataTokens["area"] as string;
            }

            return null;
        }
        protected internal DisplayModeProvider DisplayModeProvider
        {
            get { return _displayModeProvider ?? DisplayModeProvider.Instance; }
            set { _displayModeProvider = value; }
        }
        private string CreateCacheKey(string theme, string plugin, string prefix, string name, string controllerName, string areaName)
        {
            return string.Format(CultureInfo.InvariantCulture, _cacheKeyFormat, GetType().AssemblyQualifiedName, theme, plugin, prefix, name, controllerName, areaName);
        }
        internal static string AppendDisplayModeToCacheKey(string cacheKey, string displayMode)
        {
            // key format is ":ViewCacheEntry:{cacheType}:{prefix}:{name}:{controllerName}:{areaName}:"
            // so append "{displayMode}:" to the key
            return cacheKey + displayMode + ":";
        }
        private string GetPath(ControllerContext controllerContext, IEnumerable<string> locations, IEnumerable<string> areaLocations
            , string locationsPropertyName, string name, string controllerName, string cacheKeyPrefix, bool useCache
            ,  out string[] searchedLocations)
        {
            searchedLocations = _emptyLocations;

            if (string.IsNullOrEmpty(name))
            {
                return string.Empty;
            }

            string areaName = GetAreaName(controllerContext.RouteData);
            bool usingAreas = !string.IsNullOrEmpty(areaName);

            string plugin = (string)controllerContext.RouteData.Values["plugin"];
            string theme = LS.Get<Settings>().FirstOrDefault().Themes;

            if (controllerContext.HttpContext.Items["ShopTheme"] != null)
            {
                theme = (string)controllerContext.HttpContext.Items["ShopTheme"];
            }

            List<ViewLocation> viewLocations = GetViewLocations(locations, (usingAreas) ? areaLocations : null);

            if (viewLocations.Count == 0)
            {
                throw new InvalidOperationException(string.Format(CultureInfo.CurrentCulture, "The property '{0}' cannot be null or empty.", locationsPropertyName));
            }

            bool nameRepresentsPath = IsSpecificPath(name);

            string cacheKey = CreateCacheKey(theme, plugin, cacheKeyPrefix, name, (nameRepresentsPath) ? string.Empty : controllerName, areaName);

            if (useCache)
            {
                // Only look at cached display modes that can handle the context.
                IEnumerable<IDisplayMode> possibleDisplayModes = DisplayModeProvider.GetAvailableDisplayModesForContext(controllerContext.HttpContext, controllerContext.DisplayMode);
                foreach (IDisplayMode displayMode in possibleDisplayModes)
                {
                    if (controllerContext.DisplayMode == null)
                    {
                        controllerContext.DisplayMode = displayMode;
                    }
                    string cachedLocation = ViewLocationCache.GetViewLocation(controllerContext.HttpContext, AppendDisplayModeToCacheKey(cacheKey, displayMode.DisplayModeId));

                    if (cachedLocation != null)
                    {
                        

                        return cachedLocation;
                    }
                }

                // GetPath is called again without using the cache.
                return null;
            }
            else
            {
                return nameRepresentsPath
                         ? GetPathFromSpecificName(controllerContext, name, cacheKey, ref searchedLocations)
                         : GetPathFromGeneralName(controllerContext, viewLocations, name, controllerName, areaName,theme,plugin, cacheKey, ref searchedLocations);
            }
        }

        private string GetPathFromGeneralName(ControllerContext controllerContext, IList<ViewLocation> locations, string name, string controllerName, string areaName, string theme, string plugin, string cacheKey, ref string[] searchedLocations)
        {
            string result = string.Empty;
            searchedLocations = new string[locations.Count];

            for (int i = 0; i < locations.Count; i++)
            {
                ViewLocation location = locations[i];
                string virtualPath = location.Format(name, controllerName, areaName, theme, plugin);
                DisplayInfo virtualPathDisplayInfo = DisplayModeProvider.GetDisplayInfoForVirtualPath(virtualPath, controllerContext.HttpContext, path => FileExists(controllerContext, path), controllerContext.DisplayMode);
                if (virtualPathDisplayInfo != null)
                {
                    string resolvedVirtualPath = virtualPathDisplayInfo.FilePath;

                    searchedLocations = _emptyLocations;
                    result = resolvedVirtualPath;
                    ViewLocationCache.InsertViewLocation(controllerContext.HttpContext, AppendDisplayModeToCacheKey(cacheKey, virtualPathDisplayInfo.DisplayMode.DisplayModeId), result);

                    if (controllerContext.DisplayMode == null)
                    {
                        controllerContext.DisplayMode = virtualPathDisplayInfo.DisplayMode;
                    }

                    // Populate the cache with the existing paths returned by all display modes.
                    // Since we currently don't keep track of cache misses, if we cache view.aspx on a request from a standard browser
                    // we don't want a cache hit for view.aspx from a mobile browser so we populate the cache with view.Mobile.aspx.
                    IEnumerable<IDisplayMode> allDisplayModes = DisplayModeProvider.Modes;
                    foreach (IDisplayMode displayMode in allDisplayModes)
                    {
                        if (displayMode.DisplayModeId != virtualPathDisplayInfo.DisplayMode.DisplayModeId)
                        {
                            DisplayInfo displayInfoToCache = displayMode.GetDisplayInfo(controllerContext.HttpContext, virtualPath, virtualPathExists: path => FileExists(controllerContext, path));

                            if (displayInfoToCache != null && displayInfoToCache.FilePath != null)
                            {
                                ViewLocationCache.InsertViewLocation(controllerContext.HttpContext, AppendDisplayModeToCacheKey(cacheKey, displayInfoToCache.DisplayMode.DisplayModeId), displayInfoToCache.FilePath);
                            }
                        }
                    }
                    break;
                }
                //if (FileExists(controllerContext, virtualPath))
                //{
                //    searchedLocations = _emptyLocations;
                //    result = virtualPath;
                //    ViewLocationCache.InsertViewLocation(controllerContext.HttpContext, cacheKey, result);
                //    break;
                //}

                searchedLocations[i] = virtualPath;
            }

            return result;
        }

        private string GetPathFromSpecificName(ControllerContext controllerContext, string name, string cacheKey, ref string[] searchedLocations)
        {
            string result = name;

            if (!(FilePathIsSupported(name) && FileExists(controllerContext, name)))
            {
                result = String.Empty;
                searchedLocations = new[] { name };
            }

            ViewLocationCache.InsertViewLocation(controllerContext.HttpContext, cacheKey, result);
            return result;
        }
        internal Func<string, string> GetExtensionThunk = VirtualPathUtility.GetExtension;
        private bool FilePathIsSupported(string virtualPath)
        {
            if (FileExtensions == null)
            {
                // legacy behavior for custom ViewEngine that might not set the FileExtensions property
                return true;
            }
            else
            {
                // get rid of the '.' because the FileExtensions property expects extensions withouth a dot.
                string extension = GetExtensionThunk(virtualPath).TrimStart('.');
                return FileExtensions.Contains(extension, StringComparer.OrdinalIgnoreCase);
            }
        }

        private class AreaAwareViewLocation : ViewLocation
        {
            public AreaAwareViewLocation(string virtualPathFormatString) : base(virtualPathFormatString)
            {
            }

            public override string Format(string viewName, string controllerName, string areaName, string theme, string plugin)
            {
                return string.Format(CultureInfo.InvariantCulture, _virtualPathFormatString, viewName, controllerName, areaName, theme, plugin);
            }
        }

        private class ViewLocation
        {
            protected readonly string _virtualPathFormatString;

            public ViewLocation(string virtualPathFormatString)
            {
                _virtualPathFormatString = virtualPathFormatString;
            }

            public virtual string Format(string viewName, string controllerName, string areaName, string theme, string plugin)
            {
                return string.Format(CultureInfo.InvariantCulture, _virtualPathFormatString, viewName, controllerName, theme, plugin);
            }
        }
    }
}
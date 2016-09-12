namespace Uco.Infrastructure.ViewEngine
{
    using System;
    using System.Web.Mvc;

    public abstract class CustomBuildManagerViewEngine : CustomVirtualPathProviderViewEngine
    {
        protected CustomBuildManagerViewEngine() : this(new BuildManagerWrapper())
        {
        }

        protected CustomBuildManagerViewEngine(IBuildManager buildManager)
        {
            if (buildManager == null)
            {
                throw new ArgumentNullException("buildManager");
            }

            BuildManager = buildManager;
        }

        protected IBuildManager BuildManager { get; private set; }

        protected override sealed bool FileExists(ControllerContext controllerContext, string virtualPath)
        {
            return BuildManager.FileExists(virtualPath);
        }

        protected override sealed bool? IsValidPath(ControllerContext controllerContext, string virtualPath)
        {
            Type compiledType = BuildManager.GetCompiledType(virtualPath);

            return compiledType == null ? (bool?)null : 
                true;
        }

        protected abstract bool IsValidCompiledType(ControllerContext controllerContext, string virtualPath, Type compiledType);
    }
}
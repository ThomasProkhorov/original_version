namespace Uco.Infrastructure.ViewEngine
{
    using System;
    using System.Web.Compilation;
    using System.Web.Mvc;

    public static class CustomViewEngine
    {
        public static void RegisterViewEngine(ViewEngineCollection viewEngines)
        {
            viewEngines.Clear();
            var CustomRazorViewEngine = new CustomRazorViewEngine();
            viewEngines.Add(CustomRazorViewEngine);
        }
       
    }
}
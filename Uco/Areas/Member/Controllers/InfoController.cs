using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Uco.Infrastructure;
using Uco.Models;
using System.Diagnostics;
using System.Reflection;
using System.Web.Configuration;
using System.Configuration;

namespace Uco.Areas.Shop.Controllers
{
    public class InfoController : BaseAdminController
    {
        public ActionResult Index()
        {

            //Check if DEBUG
            object[] attribs = Assembly.GetAssembly(typeof(Uco.Areas.Admin.Controllers.InfoController)).GetCustomAttributes(typeof(System.Diagnostics.DebuggableAttribute), false);
            // If the 'DebuggableAttribute' is not found then it is definitely an OPTIMIZED build
            if (attribs.Length > 0)
            {
                // Just because the 'DebuggableAttribute' is found doesn't necessarily mean
                // it's a DEBUG build; we have to check the JIT Optimization flag
                // i.e. it could have the "generate PDB" checked but have JIT Optimization enabled
                DebuggableAttribute debuggableAttribute = attribs[0] as DebuggableAttribute;
                if (debuggableAttribute != null)
                {
                    ViewBag.HasDebuggableAttribute = true;
                    ViewBag.IsJITOptimized = !debuggableAttribute.IsJITOptimizerDisabled;
                    ViewBag.BuildType = debuggableAttribute.IsJITOptimizerDisabled ? "Debug" : "Release";
                    // check for Debug Output "full" or "pdb-only"
                    ViewBag.DebugOutput = (debuggableAttribute.DebuggingFlags & DebuggableAttribute.DebuggingModes.Default) != DebuggableAttribute.DebuggingModes.None ? "Full" : "pdb-only";
                }
            }
            else
            {
                ViewBag.IsJITOptimized = true;
                ViewBag.BuildType = "Release";
            }



            //CHECK Content/UserFiles WRITE
            try
            {
                string testFilePath = Server.MapPath("~/Content/UserFiles/test.txt");
                if (System.IO.File.Exists(testFilePath))
                {
                    System.IO.File.Delete(testFilePath);
                }
                System.IO.StreamWriter fs = new System.IO.StreamWriter(testFilePath);
                fs.WriteLine("test");
                fs.Close();
                System.IO.File.Delete(testFilePath);
                ViewBag.HaveWriteAccessUserFiles = true;
            }
            catch
            {
                ViewBag.HaveWriteAccessUserFiles = false;
            }



            //CHECK App_Data/cache/images WRITE
            try
            {
                string testFilePath = Server.MapPath("~/App_Data/cache/images/test.txt");
                if (System.IO.File.Exists(testFilePath))
                {
                    System.IO.File.Delete(testFilePath);
                }
                System.IO.StreamWriter fs = new System.IO.StreamWriter(testFilePath);
                fs.WriteLine("test");
                fs.Close();
                System.IO.File.Delete(testFilePath);
                ViewBag.HaveWriteAccessImages = true;
            }
            catch
            {
                ViewBag.HaveWriteAccessImages = false;
            }



            //Check if log is full
            if (_db.Errors.Count() >= 300) ViewBag.LogIsFull = true;
            else ViewBag.LogIsFull = false;



            //CHECH GAC
            int i = 0;
            CompilationSection configSection = (CompilationSection)ConfigurationManager.GetSection("system.web/compilation");
            foreach (AssemblyInfo item in configSection.Assemblies)
            {
                if (item.Assembly.ToString() == "MiniProfiler, Version=2.0.2.0, Culture=neutral, PublicKeyToken=b44f9351044011a3"
                    || item.Assembly.ToString() == "MiniProfiler.EntityFramework, Version=2.0.3.0, Culture=neutral, PublicKeyToken=b44f9351044011a3"
                    || item.Assembly.ToString() == "Telerik.Web.Mvc, Version=2012.3.1018.340, Culture=neutral, PublicKeyToken=121fae78165ba3d4"
                    ) i = i + 1;
            }
            ViewBag.GacNum = i;


            //CHECK httpErrors
            ViewBag.httpErrors = "";
            var conf = System.Xml.Linq.XDocument.Load(System.Web.Hosting.HostingEnvironment.ApplicationPhysicalPath + "web.config");
            if (conf != null)
            {
                var errMode = conf.Root.Element("system.webServer").Element("httpErrors");
                if (errMode != null) ViewBag.httpErrors = errMode.ToString();
            }

            return View();
        }

    }
}

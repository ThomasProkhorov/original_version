using System.Web.Mvc;
using Uco.Infrastructure.Repositories;

namespace Uco.Controllers
{
    public class ErrorController : Controller
    {
        public ActionResult Error404()
        {
            string RawUrl = Request.RawUrl.Trim();
            string errorsString = "";
            string errorFileName = "~/App_Data/404.txt";
            string errorFilePath = Server.MapPath(errorFileName);
            if (System.IO.File.Exists(errorFilePath))
            {
                System.IO.StreamReader errorFile = new System.IO.StreamReader(errorFilePath);
                errorsString = errorFile.ReadToEnd();
                errorFile.Close();
            }
            errorsString = errorsString + "\r\n" + RP.GetCurrentSettings().Error404;

            if (!string.IsNullOrEmpty(errorsString) && errorsString.Contains(RawUrl))
            {
                errorsString = errorsString.Replace("\r", "");
                foreach (string item in errorsString.Split('\n'))
                {
                    if (item.Contains(" <-> "))
                    {
                        int i = item.IndexOf(" <-> ");
                        string OldUrl = item.Remove(i).Trim();
                        string NewUrl = item.Remove(0, i + 5).Trim();
                        if (OldUrl == RawUrl) return RedirectPermanent(NewUrl);
                    }
                }
            }

            Response.StatusCode = 404;
            return View();
        }

        public ActionResult Test()
        {
            return View();
        }
    }
}

using System.Web.Mvc;
using Uco.Infrastructure.Livecycle;
using Uco.Infrastructure.Repositories;
using Uco.Models;
using System.Linq;
using System;

namespace Uco.Controllers
{
    public partial class PagePartController : BaseController
    {
        [HttpPost]
        public ActionResult _BannerClick(int ID)
        {
            var banner = _db.Banners.FirstOrDefault(x => x.ID == ID);
            if (banner != null)
            {
                banner.Clicks++;
                _db.SaveChanges();
            }
            return Json(new { result = "done" });
        }
        #region ChildAction

        [ChildActionOnly]
        [OutputCache(Duration = 3600, VaryByCustom = "LangCodeDomainAndCleanCacheGuid")]
        public ActionResult ContentBySeo(string SeoUrl)
        {
            var CurrentSettings = RP.GetCurrentSettings();
            var contentPage = _db.ContentPages.FirstOrDefault(x => x.SeoUrlName == SeoUrl && x.DomainID == CurrentSettings.ID);

            if (contentPage != null)
            {
                return PartialView(contentPage);
            }
            return Content("");
        }

        [ChildActionOnly]
        [OutputCache(Duration = 3600, VaryByCustom = "LangCodeDomainAndCleanCacheGuid")]
        public ActionResult _QuestionAnswer()
        {
            return View(_db.NewsPages.Where(r => r.Visible && r.ParentID == 16).ToList());
        }


        [ChildActionOnly]
        public ActionResult _Banner(int Num, string BannerGroup)
        {
            var l = RP.GetBannersReprository(BannerGroup);
            l = l.OrderBy(r => Guid.NewGuid()).Take(Num).ToList();

            return View(l);
            //   @Html.Action("_Banner", "PagePart", new { Num = 10, BannerGroup = "SearchPage" })
        }
        #endregion

        #region Ajax


        #endregion

        #region Cache


        #endregion
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Kendo.Mvc.UI;
using Kendo.Mvc.Extensions;
using Uco.Infrastructure;
using Uco.Models;
using System.Data.Entity;
using Uco.Infrastructure.Repositories;
using System.IO;
using System.Text;
using Uco.Infrastructure.Livecycle;
using Uco.Models.Overview;


namespace Uco.Areas.Admin.Controllers
{
    [AuthorizeAdmin]
    public class CategoryMenuController : BaseAdminController
    {
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult _GetCategoriesAjax(int shopID)
        {
            var allCategories = LS.Get<Category>();//.Select(x => new { x.ID, x.Name, x.ParentCategoryID })
            var model = allCategories.Where(x => x.ParentCategoryID == 0).Select(x => new TreeModel()
            {
                ID = x.ID,
                Name = x.Name,
                ParentID = x.ParentCategoryID
            }).ToList();
            foreach (var ctree in model)
            {
                ctree.Childrens = allCategories.Where(x => x.ParentCategoryID == ctree.ID).Select(x => new TreeModel()
                {
                    ID = x.ID,
                    Name = x.Name,
                    Path = "  " + ctree.Name + " >> ",
                    ParentID = x.ParentCategoryID
                }).ToList();
            }
            var sid = shopID;
            var currentShopModel = _db.ShopCategoryMenus.Where(x => x.ShopID == sid).ToList();
            return Json(new { categoryTree = model, menu = currentShopModel });
        }

        public ActionResult _SaveMenuAjax(List<CategoryMenuModel> model, int shopID)
        {
            if (model == null)
            {
                model = new List<CategoryMenuModel>();
            }
            var lookup = model.ToLookup(x => x.Order);

            var sid = shopID;
            var currentShopModel = _db.ShopCategoryMenus.Where(x => x.ShopID == sid).ToList();
            _db.ShopCategoryMenus.RemoveRange(currentShopModel);
            if (model != null)
            {
                _db.Configuration.ValidateOnSaveEnabled = false;
                foreach (var group in lookup)
                {
                    int displayOrder = 0;
                    foreach (var item in group)
                    {
                        var shopMenu = new ShopCategoryMenu()
                        {
                            CategoryID = item.CategoryID,
                            DisplayOrder = displayOrder,
                            GroupNumber = group.Key,
                            Published = item.Published,
                            ShopID = sid,
                            Level = 0
                        };
                        if (displayOrder > 0)
                        {
                            shopMenu.Level = 1;
                        }
                        displayOrder++;
                        _db.ShopCategoryMenus.Add(shopMenu);
                    }
                }
                _db.SaveChanges();
            }
            return Json(lookup);
        }
    }


}

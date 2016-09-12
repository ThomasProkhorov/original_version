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


namespace Uco.Areas.Member.Controllers
{
    [Authorize(Roles = "Member")]
    public class CategoryMenuController : BaseMemberController
    {
        public ActionResult Index()
        {
            return View();
        }
        public ActionResult _GetMappedCategoriesAjax()
        {
            var allCategories = LS.Get<Category>();//.Select(x => new { x.ID, x.Name, x.ParentCategoryID })

            var sid = LS.CurrentShop.ID;

            var allmapped = (from ps in _db.ProductShopMap
                             join p in _db.Products on ps.ProductID equals p.ID
                             join c in _db.Categories on p.CategoryID equals c.ID
                             where p.Deleted == false && ps.ShopID == sid
                             select c.ID
                                 ).ToList();
            var preparedMenu = new Dictionary<int, ShopCategoryMenu>();
            foreach (var cid in allmapped)
            {
                var category = allCategories.FirstOrDefault(x => x.ID == cid);
                if (category != null)
                {
                    if (category.ParentCategoryID == 0)
                    {
                        if (!preparedMenu.ContainsKey(cid))
                        {
                            var prodCount = LS.GetCachedFunc(() =>
                            {

                                return (
                                    from ps in _db.ProductShopMap
                                    join p in _db.Products
                                      on ps.ProductID equals p.ID
                                    where ps.ShopID == sid
                                    && p.CategoryID == cid
                                    && p.Deleted == false
                                    select ps.ID).Count();

                            }, string.Format("productsInCategory-{0}-{1}.", cid, sid), 60);
                            preparedMenu[cid] = new ShopCategoryMenu()
                            {
                                CategoryID = cid,
                                ShopID = sid
                                ,
                                CacheProdCount = prodCount
                                ,
                                Published = true,
                                Level = cid,
                                GroupNumber = category.DisplayOrder - 100000

                            };
                        }
                    }
                    else
                    {
                        var pid = category.ParentCategoryID;
                        if (!preparedMenu.ContainsKey(pid))
                        {
                            var categoryParent = allCategories.FirstOrDefault(x => x.ID == pid);
                            if (categoryParent == null)
                            {
                                categoryParent = category;
                            }
                            var prodCount = LS.GetCachedFunc(() =>
                            {

                                return (
                                    from ps in _db.ProductShopMap
                                    join p in _db.Products
                                      on ps.ProductID equals p.ID
                                    where ps.ShopID == sid
                                    && p.CategoryID == pid
                                    && p.Deleted == false
                                    select ps.ID).Count();

                            }, string.Format("productsInCategory-{0}-{1}.", pid, sid), 60);
                            preparedMenu[pid] = new ShopCategoryMenu()
                            {
                                CategoryID = pid,
                                ShopID = sid
                                ,
                                CacheProdCount = prodCount
                                ,
                                Published = true,
                                Level = pid,
                                GroupNumber = categoryParent.DisplayOrder - 100000

                            };


                        }
                        if (!preparedMenu.ContainsKey(cid))
                        {
                            //cached product count
                            var prodCount = LS.GetCachedFunc(() =>
                            {

                                return (
                                    from ps in _db.ProductShopMap
                                    join p in _db.Products
                                      on ps.ProductID equals p.ID
                                    where ps.ShopID == sid
                                    && p.CategoryID == cid
                                    && p.Deleted == false
                                    select ps.ID).Count();

                            }, string.Format("productsInCategory-{0}-{1}.", cid, sid), 60);

                            preparedMenu[cid] = new ShopCategoryMenu()
                            {
                                CategoryID = cid,
                                ShopID = sid
                                ,
                                Published = true,
                                Level = pid,
                                CacheProdCount = prodCount,
                                GroupNumber = category.DisplayOrder + 1000000
                            };
                            preparedMenu[pid].CacheProdCount += prodCount;
                        }
                    }
                }
            }
            // var currentShopModel = _db.ShopCategoryMenus.Where(x => x.ShopID == sid).ToList();
            return Json(new { preparedMenu = preparedMenu.Select(x => x.Value).OrderBy(x => x.GroupNumber).ThenBy(x => x.Level) });
        }
        public ActionResult _GetCategoriesAjax()
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
            var sid = LS.CurrentShop.ID;
            var currentShopModel = _db.ShopCategoryMenus.Where(x => x.ShopID == sid).ToList();
            return Json(new { categoryTree = model, menu = currentShopModel });
        }

        public ActionResult _SaveMenuAjax(List<CategoryMenuModel> model)
        {
            if (model == null)
            {
                model = new List<CategoryMenuModel>();
            }
            var lookup = model.ToLookup(x => x.Order);

            var sid = LS.CurrentShop.ID;
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
                            HeadOfGroup = item.HeadOfGroup,
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

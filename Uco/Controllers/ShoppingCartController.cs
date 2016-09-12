using System.Web.Mvc;
using Uco.Infrastructure;
using Uco.Models;
using System.Linq;
using System.Collections.Generic;
using Uco.Infrastructure.Livecycle;
using Uco.Models.Overview;
using Uco.Infrastructure.Services;
using System;

namespace Uco.Controllers
{
    [Localization]
    public partial class ShoppingCartController : BaseController
    {
        private ShoppingCartService _shoppingCartService;
        public ShoppingCartController()
        {
            _shoppingCartService = new ShoppingCartService(_db);

        }
        public ActionResult Index(int ID = 0)
        {
            if (!LS.isHaveID())
            {
                return Redirect("~/");
            }
            if (ID == 0)
            {
                ID = ShoppingCartService.GetFirstShopID();
                if (ID > 0)
                {
                    return RedirectToAction("Index", new { ID = ID });
                }
            }
            var model = _shoppingCartService.GetShoppingCartModel(ID, feutured: true, withship: true
                , checkQuantity: true, loadComments: true);
            if (model.Shop == null || !model.Shop.Active) { return Redirect("~/"); }


            if (LS.CurrentHttpContext.Request.Cookies["SALcart"] != null)
            {
                //retrieve old cart
                var oldGuid = new Guid(LS.CurrentHttpContext.Request.Cookies["SALcart"].Value);

                var oldModel = _shoppingCartService.GetShoppingCartModel(ID, feutured: true, withship: true
                    , checkQuantity: true, loadComments: true, UserID: oldGuid);

                model.NotAvaliableItems = oldModel.Items;

            }

            if (!string.IsNullOrEmpty(model.Shop.Theme))
            {
                this.HttpContext.Items["ShopTheme"] = model.Shop.Theme;
            }

            return View(model);
        }

        // its can be from child action and ajax loaded
        public ActionResult FooterCart(int ID = 0, bool tableonly = false)
        {
            var model = new ShoppingCartOverviewModel();
            if (LS.isHaveID())
            {
                if (ID == 0)
                {
                    if (LS.CurrentHttpContext.Session["ShopID"] != null)
                    {
                        ID = (int)LS.CurrentHttpContext.Session["ShopID"];
                    }
                    else
                    {
                        ID = ShoppingCartService.GetFirstShopID();
                    }

                }
                model = _shoppingCartService.GetShoppingCartModel(ID, withship: true);

                //_db.ShoppingCartItems.Where(x => x.UserID == LS.CurrentUser.ID).ToList();

            }

            if (!string.IsNullOrEmpty(model.Shop.Theme))
            {
                this.HttpContext.Items["ShopTheme"] = model.Shop.Theme;
            }

            if (tableonly)
            {
                return PartialView("_CartTable", model);
            }
            return PartialView(model);
        }

        #region shopping cart
        public ActionResult ClearCart(int ID)
        {
            if (!LS.isHaveID())
            {
                return Json(new { result = "error", action = "login", message = "You must login first", data = ID });
            }
            ShoppingCartService.ClearCart(LS.CurrentUser.ID, ID);
            return Json(new { result = "ok", data = ID });
        }

        [HttpPost]
        public ActionResult KeepOld(bool keep)
        {
            if (!LS.isHaveID())
            {
                return Json(new { result = "error", action = "login", message = "You must login first" });
            }
            if (LS.CurrentHttpContext.Request.Cookies["SALcart"] != null)
            {
                //retrieve old cart
                var oldGuid = new Guid(LS.CurrentHttpContext.Request.Cookies["SALcart"].Value);
                if (keep)
                {
                    ShoppingCartService.MigrateShoppingCart(oldGuid, LS.CurrentUser.ID);
                }
                else
                {
                    //remove old
                    var forRemove = _db.ShoppingCartItems.Where(x => x.UserID == oldGuid).ToList();
                    _db.ShoppingCartItems.RemoveRange(forRemove);
                    _db.SaveChanges();
                }
                LS.DeleteCookie("SALcart");
            }
            return Json(new { result = "ok" });
        }

        [HttpPost]
        public ActionResult ChangeItem(int ID, decimal? Quantity, decimal? QuantityBit, bool? Delete, int? Attribute, int? qtype)
        {
            if (!LS.isHaveID())
            {
                return Json(new { result = "error", action = "login", message = "You must login first", data = ID });
            }

            var item = new ShoppingCartItem()
            {
                ID = ID
            };
            var addmodel = ShoppingCartService.AddToCart(LS.CurrentUser.ID, item, Quantity, QuantityBit, Delete, Attribute, qtype);
            if (addmodel.errors.Count > 0)
            {
                return Json(new { result = "error", message = addmodel.errors.FirstOrDefault(), data = addmodel.item });

            }
            bool withship = true;
            if (Request.UrlReferrer != null && Request.UrlReferrer.PathAndQuery.ToLower().Contains("shoppingcart"))
            {
                withship = true;
            }
            var model = _shoppingCartService.GetShoppingCartModel(addmodel.item.ShopID, loadattributes: false, withship: withship);

            var curCartItem = model.Items.FirstOrDefault(x => x.ID == ID); //GetShoppingCartItemByID(ID);
            if (curCartItem != null)
            {
                return Json(new { result = "ok", data = curCartItem, cart = model });
            }
            return Json(new { result = "ok", data = addmodel.item, cart = model });
        }

        [HttpPost]
        public ActionResult AddToCartAjx(ShoppingCartItem item)
        {
            if (item.ProductShopID < 1)
            {
                return Json(new { result = "error", message = "Data issuse", data = item });
            }
            if (!LS.isHaveID())
            {
                return Json(new { result = "error", action = "login", message = "You must login first", data = item });
            }
            if (item.Quantity < 1)
            {
                return Json(new { result = "error", message = "Quantity can`t be less then 1", data = item });
            }
            //check existing
            ShoppingCartService.AddToCart(LS.CurrentUser.ID, item);
            return Json(new { result = "ok", data = item });
        }
        #endregion


        public ActionResult _GetShoppingCartTable(int ID)
        {
            if (ID == 0)
            {
                ID = ShoppingCartService.GetFirstShopID();
                if (ID > 0)
                {
                    return RedirectToAction("Index", new { ID = ID });
                }
            }
            var model = _shoppingCartService.GetShoppingCartModel(ID);


            if (!string.IsNullOrEmpty(model.Shop.Theme))
            {
                this.HttpContext.Items["ShopTheme"] = model.Shop.Theme;
            }

            return PartialView("_ShoppingCartTable", model);
        }

    }
}

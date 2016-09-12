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
using System.IO;
using System.Text;
using System.Web.Security;
using Uco.Infrastructure.Livecycle;

namespace Uco.Areas.Admin.Controllers
{
    [AuthorizeAdmin]
    public class ShopController : BaseAdminController
    {
        public ActionResult Index()
        {
            ViewBag.MessageRed = TempData["MessageRed"];
            ViewBag.MessageYellow = TempData["MessageYellow"];
            ViewBag.MessageGreen = TempData["MessageGreen"];
            return View();
        }

        public ActionResult EditAsMember(int ID)
        {
            var shop = _db.Shops.FirstOrDefault(x => x.ID == ID);
            if (shop != null)
            {
                var user = _db.Users.FirstOrDefault(x => x.ID == shop.UserID);
                if (user != null)
                {
                    TempData["_SwitchedShop"] = ID;
                    FormsAuthentication.SetAuthCookie(user.UserName, true);
                    Session["CurrentShopID"] = ID;
                }
            }

            return RedirectToAction("ShopProfile", "Main", new { area = "Member" });
        }
        public ActionResult Copy(int Id)
        {
            var shop = _db.Shops.FirstOrDefault(x => x.ID == Id);
            if (shop != null)
            {
                _db.Configuration.ValidateOnSaveEnabled = false;

                var clonedShopp = new Shop();
                #region copy fileds
                clonedShopp.Active = shop.Active;
                clonedShopp.Address = shop.Address;
                clonedShopp.AddressMap = shop.AddressMap;
                clonedShopp.CreateTime = DateTime.Now;
                clonedShopp.CreditGuardMid = shop.CreditGuardMid;
                clonedShopp.CreditGuardPass = shop.CreditGuardPass;
                clonedShopp.CreditGuardTerminal = shop.CreditGuardTerminal;
                clonedShopp.CreditGuardUrl = shop.CreditGuardUrl;
                clonedShopp.CreditGuardUser = shop.CreditGuardUser;
                clonedShopp.DeliveryManualDescription = shop.DeliveryManualDescription;
                clonedShopp.DeliveryTime = shop.DeliveryTime;
                clonedShopp.DisplayOrder = shop.DisplayOrder;
                clonedShopp.Email = shop.Email;
                clonedShopp.FreeShipFrom = shop.FreeShipFrom;
                clonedShopp.FullDescription = shop.FullDescription;
                clonedShopp.Image = shop.Image;
                clonedShopp.InStorePickUpEnabled = shop.InStorePickUpEnabled;
                clonedShopp.IsShipEnabled = shop.IsShipEnabled;
                clonedShopp.IsToShopOwnerCredit = shop.IsToShopOwnerCredit;
                clonedShopp.Kosher = shop.Kosher;
                clonedShopp.Latitude = shop.Latitude;
                clonedShopp.Longitude = shop.Longitude;
                clonedShopp.MounthlyFee = shop.MounthlyFee;
                clonedShopp.Name = shop.Name;
                clonedShopp.PercentFee = shop.PercentFee;
                clonedShopp.Phone = shop.Phone;
                clonedShopp.Phone2 = shop.Phone2;
                clonedShopp.RadiusLatitude = shop.RadiusLatitude;
                clonedShopp.RadiusLongitude = shop.RadiusLongitude;
                clonedShopp.SeoUrl = shop.SeoUrl;
                clonedShopp.ShipCost = shop.ShipCost;
                clonedShopp.ShipRadius = shop.ShipRadius;
                clonedShopp.ShopTypeIDs = shop.ShopTypeIDs;
                clonedShopp.ShortDescription = shop.ShortDescription;
                clonedShopp.SpecialPercentFee = shop.SpecialPercentFee;
                clonedShopp.UserID = shop.UserID;
                clonedShopp.Youtube = shop.Youtube;
                #endregion
                _db.Shops.Add(clonedShopp);
                _db.SaveChanges();
                // 1) work times
                #region work times
                var worktimes = _db.ShopWorkTimes.Where(x => x.ShopID == shop.ID).ToList();
                foreach (var w in worktimes)
                {
                    var sw = new ShopWorkTime();
                    sw.Active = w.Active;
                    sw.Date = w.Date;
                    sw.Day = w.Day;
                    sw.IsSpecial = w.IsSpecial;
                    sw.ShopID = clonedShopp.ID;
                    sw.TimeFrom = w.TimeFrom;
                    sw.TimeTo = w.TimeTo;
                    _db.ShopWorkTimes.Add(sw);
                }
                _db.SaveChanges();
                #endregion
                // 2) ship times
                #region ship times
                var shipimes = _db.ShopShipTimes.Where(x => x.ShopID == shop.ID).ToList();
                foreach (var w in shipimes)
                {
                    var sw = new ShopShipTime();
                    sw.Active = w.Active;
                    sw.Date = w.Date;
                    sw.Day = w.Day;
                    sw.IsSpecial = w.IsSpecial;
                    sw.ShopID = clonedShopp.ID;
                    sw.TimeFrom = w.TimeFrom;
                    sw.TimeTo = w.TimeTo;

                    _db.ShopShipTimes.Add(sw);
                }
                _db.SaveChanges();
                #endregion
                // 3) products with attr
                #region shop product
                var products = _db.ProductShopMap.Where(x => x.ShopID == shop.ID).ToList();
                List<ProductShop> copied = new List<ProductShop>();
                foreach (var p in products)
                {
                    var sp = new ProductShop();
                    sp.CreateDate = DateTime.Now;
                    sp.DontImportPrice = p.DontImportPrice;
                    sp.HaveDiscount = false;
                    sp.IncludeInShipPrice = p.IncludeInShipPrice;
                    sp.IncludeVat = p.IncludeVat;
                    sp.MaxCartQuantity = p.MaxCartQuantity;
                    sp.NotInCategory = p.NotInCategory;
                    sp.Price = sp.Price;
                    sp.ProductID = p.ProductID;
                    sp.Quantity = p.Quantity;
                    sp.QuantityType = p.QuantityType;
                    sp.ShopID = clonedShopp.ID;

                    _db.ProductShopMap.Add(sp);
                    copied.Add(sp);
                    //attr
                    sp.ProductAttributeOptions = _db.ProductAttributeOptions.Where(x => x.ProductShopID == p.ID).ToList();
                }
                _db.SaveChanges();
                foreach (var sp in copied)
                {
                    foreach (var a in sp.ProductAttributeOptions)
                    {
                        var sa = new ProductAttributeOption();
                        sa.Name = a.Name;
                        sa.OverridenPrice = a.OverridenPrice;
                        sa.OverridenSku = a.OverridenSku;
                        sa.ProductShopID = sp.ID;
                        sa.Quantity = a.Quantity;
                        _db.ProductAttributeOptions.Add(sa);
                    }
                }
                _db.SaveChanges();
                #endregion
                // 4) categories
                #region categories
                var cats = _db.ShopCategories.Where(x => x.ShopID == shop.ID).ToList();
                foreach (var c in cats)
                {
                    var sc = new ShopCategory();
                    sc.CategoryID = c.CategoryID;
                    sc.DisplayOrder = c.DisplayOrder;
                    var cat = LS.Get<Category>().FirstOrDefault(x => x.ID == c.CategoryID);
                    if (cat != null)
                    {
                        sc.DisplayOrder = cat.DisplayOrder;
                    }
                    sc.Published = c.Published;
                    sc.ShopID = clonedShopp.ID;
                    _db.ShopCategories.Add(sc);
                }
                _db.SaveChanges();
                #endregion

                #region times



                #endregion
                return RedirectToAction("Edit", "Generic", new { model = "Shop", ID = clonedShopp.ID });
            }
            return RedirectToAction("Edit", "Generic", new { model = "Shop", ID = Id });
        }

    }
}

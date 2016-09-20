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
using Uco.Infrastructure.Discounts;
using System.Reflection;
using Uco.Infrastructure.Livecycle;

namespace Uco.Areas.Member.Controllers
{

    [Authorize(Roles = "Member")]
    public class DiscountController : BaseMemberController
    {
        private DiscountService _discountService;
        public DiscountController()
        {
            _discountService = new DiscountService(_db);
        }
        public ActionResult Index()
        {
            ViewBag.MessageRed = TempData["MessageRed"];
            ViewBag.MessageYellow = TempData["MessageYellow"];
            ViewBag.MessageGreen = TempData["MessageGreen"];


            return View(new Discount());
        }
        public ActionResult NewRule(string RuleName)
        {
            var rules = DiscountService.GetRules();
            if (string.IsNullOrEmpty(RuleName) || !rules.ContainsKey(RuleName))
            {
                return Content(RP.T("Admin.Discount.RuleNotFound").ToString());
            }
            var rule = rules[RuleName];
            var model = rule.GetConfigItem();
            return PartialView(model);
        }
        [ChildActionOnly]
        public ActionResult Rules(int ID)
        {
            var lst = DiscountService.GetRules();

            var model =
                lst.Select(x => new SelectListItem()
                {
                    Text = _discountService.GetLocalizeRuleName(x.Key),
                    Value = x.Key
                }).ToList()
                ;

            return View(model);
        }
        private void MapModel(object model, string prefix = "")
        {
            var t = model.GetType();

            MethodInfo method = this.GetType().GetMethods(BindingFlags.Instance | BindingFlags.NonPublic)
                .Where(x => x.Name == "TryUpdateModel"
                    && x.GetParameters().Count() == 2
                    && x.GetParameters()[1].ParameterType == typeof(string)
                    )
                .FirstOrDefault();
            MethodInfo genericMethod = method.MakeGenericMethod(t);
            var refObject = genericMethod.Invoke(this, new object[] { model, prefix });
        }
        public ActionResult DiscountRuleList(int ID)
        {
            var discount = _db.Discounts.FirstOrDefault(x => x.ID == ID);
            if (discount != null)
            {
                var list = new List<JsonKeyObject>();
                var rules = DiscountService.GetRules();
                System.Web.Script.Serialization.JavaScriptSerializer js = new System.Web.Script.Serialization.JavaScriptSerializer();

                foreach (var r in discount.RuleList)
                {
                    if (rules.ContainsKey(r.Name))
                    {
                        var ruleprovider = rules[r.Name];
                        var confitem = ruleprovider.GetConfigItem();
                        var o = js.Deserialize(r.Value, confitem.GetType());
                        list.Add(new JsonKeyObject()
                        {
                            Name = r.Name,
                            Object = o
                        });
                    }
                }

                return PartialView(list);
            }
            return Content(RP.T("Admin.Discount.NotFound").ToString());
        }
        [HttpPost]
        public ActionResult UpdateRule(int ID, int Num, string systemName, string prefix)
        {
            var rules = DiscountService.GetRules();
            if (string.IsNullOrEmpty(systemName) || !rules.ContainsKey(systemName))
            {
                return Content(RP.T("Admin.Discount.RuleNotFound").ToString());
            }
            var rule = rules[systemName];
            var saved = rule.GetConfigItem();


            System.Web.Script.Serialization.JavaScriptSerializer js = new System.Web.Script.Serialization.JavaScriptSerializer();

            var discount = _db.Discounts.FirstOrDefault(x => x.ID == ID);
            if (discount != null)
            {

                for (var i = 1; i <= discount.RuleList.Count; i++)
                {
                    if (i == Num)
                    {
                        MapModel(saved, prefix);
                        discount.RuleList[i - 1] = new JsonKeyValue()
                        {
                            Name = systemName,
                            Value = js.Serialize(saved)
                        };
                        break;
                    }
                }
            }
            //js.Serialize();
            discount.RulesData = js.Serialize(discount.RuleList);
            _db.SaveChanges();
            return Json(new { result = "ok" });
        }
        [HttpPost]
        public ActionResult RemoveRule(int ID, int Num, string systemName, string prefix)
        {
            var rules = DiscountService.GetRules();
            if (string.IsNullOrEmpty(systemName) || !rules.ContainsKey(systemName))
            {
                return Content(RP.T("Admin.Discount.RuleNotFound").ToString());
            }
            var rule = rules[systemName];
            var saved = rule.GetConfigItem();

            MapModel(saved);
            System.Web.Script.Serialization.JavaScriptSerializer js = new System.Web.Script.Serialization.JavaScriptSerializer();

            var discount = _db.Discounts.FirstOrDefault(x => x.ID == ID);
            if (discount != null)
            {

                for (var i = 1; i <= discount.RuleList.Count; i++)
                {
                    if (i == Num)
                    {

                        discount.RuleList.RemoveAt(i - 1);
                        break;
                    }
                }
            }
            //js.Serialize();
            discount.RulesData = js.Serialize(discount.RuleList);
            _db.SaveChanges();
            return Json(new { result = "ok" });
        }

        [HttpPost]
        public ActionResult SaveNewRule(int ID, string systemName, string prefix)
        {
            var rules = DiscountService.GetRules();
            if (string.IsNullOrEmpty(systemName) || !rules.ContainsKey(systemName))
            {
                return Content(RP.T("Admin.Discount.RuleNotFound").ToString());
            }
            var rule = rules[systemName];
            var saved = rule.GetConfigItem();

            MapModel(saved, prefix);
            System.Web.Script.Serialization.JavaScriptSerializer js = new System.Web.Script.Serialization.JavaScriptSerializer();

            var discount = _db.Discounts.FirstOrDefault(x => x.ID == ID);
            if (discount != null)
            {
                discount.RuleList.Add(new JsonKeyValue()
                {
                    Name = systemName,
                    Value = js.Serialize(saved)
                });
            }
            //js.Serialize();
            discount.RulesData = js.Serialize(discount.RuleList);
            _db.SaveChanges();
            return Json(new { result = "ok" });
        }

        [HttpPost]
        public ActionResult Edit(Discount item)
        {
            var discount = _db.Discounts.FirstOrDefault(x => x.ID == item.ID);
            if (discount != null && discount.ShopID == CurrentShop.ID)
            {
                discount.Amount = item.Amount;
                discount.Name = item.Name;
                discount.DiscountCode = item.DiscountCode;
                discount.DiscountType = item.DiscountType;
                discount.DisplayOrder = item.DisplayOrder;
                discount.EndDate = item.EndDate;
                discount.IsCodeRequired = item.IsCodeRequired;
                discount.IsPercent = item.IsPercent;
                discount.Limit = item.Limit;
                discount.LimitType = item.LimitType;
                discount.Percent = item.Percent;
                discount.DiscountCartItemType = item.DiscountCartItemType;
                discount.PreventNext = item.PreventNext;
                discount.ShopID = item.ShopID;
                discount.StartDate = item.StartDate;
                discount.Active = item.Active;
                var oldProductShopIds = discount.ProductShopIDs;
                discount.ProductShopIDs = item.ProductShopIDs;

                if (oldProductShopIds != null)
                {
                    foreach (var prid in oldProductShopIds.Split(new string[] { "," }, StringSplitOptions.RemoveEmptyEntries))
                    {
                        int id = Convert.ToInt32(prid);
                        var prod = _db.ProductShopMap.FirstOrDefault(x => x.ID == id);
                        if (prod != null)
                            prod.HaveDiscount = false;
                    }
                }

                if (discount.ProductShopIDs != null)
                {
                    foreach (var prid in discount.ProductShopIDs.Split(new string[] { "," }, StringSplitOptions.RemoveEmptyEntries))
                    {
                        int id = Convert.ToInt32(prid);                        
                        var prod = _db.ProductShopMap.FirstOrDefault(x => x.ID == id);
                        if (prod != null)
                            prod.HaveDiscount = discount.Active;
                    }
                }

                _db.SaveChanges();
                LS.Clear<Discount>();
                discount.ProductShopIDs = discount.ProductShopIDs ?? "";
                oldProductShopIds = oldProductShopIds ?? "";
                foreach (var prodShopId in discount.ProductShopIDs.Split(new string[] { "," }, StringSplitOptions.RemoveEmptyEntries))
                    LS.Clear("discount_info_" + prodShopId);
                foreach (var prodShopId in oldProductShopIds.Split(new string[]{","},StringSplitOptions.RemoveEmptyEntries))
                    LS.Clear("discount_info_" + prodShopId); 

            }
            return Json(new { result = "ok", discount });
        }
    }
}

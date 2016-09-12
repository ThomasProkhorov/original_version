using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using System.Web.Mvc;
using Uco.Infrastructure;
using Uco.Infrastructure.Livecycle;

namespace Uco.Models
{
    [ModelGeneral(AjaxEdit = false, Edit = true, Create = true, CreateAjax = false, SubmitButton = false, CanBack = false)]
    [ModelGeneral(Role = "Member", Acl = true, Edit = true,SubmitButton=false, CanBack = false, AjaxEdit = false, Create = true, CreateAjax = false, Delete = true, Show = true, DependedShow = true)]
    
   public partial class Discount
    {
        
       [HiddenInput(DisplayValue = false)]
       [Key]
       public int ID { get; set; }

      // [Model(Show = false, Edit = false)]
      // public int ProductShopID { get; set; }

       //[NotMapped]
       //[Model(Show = false, Edit = true, AjaxEdit = false)]
       //public virtual ProductShop ProductShop { get; set; }

       //[Model(Show = false, Edit = false)]
       //public int ProductID { get; set; }

       [Model(Show = false, Edit = false)]
       public int ShopID { get; set; }

       [NotMapped]
       
       [Model(Role = "Admin", Show = true, Edit = true, AjaxEdit = false, FilterOnTop = true)]
       [Model(Show = false, Edit = false)]
       public Shop Shop { get; set; }

       //[Model(Show = false, Edit = false)]
       //public int CategoryID { get; set; }

       //[NotMapped]
       //[Model(Show = false, Edit = true, AjaxEdit = false)]
       //public virtual Category Category { get; set; }




        [Model(Filter = true)]
       public string Name { get; set; }

          [Model(Filter = true)]
       public bool Active { get; set; }

          [Model(Filter = true)]
       public bool IsPercent { get; set; }
       
        public decimal Amount { get; set; }
      
        public int Percent { get; set; }

          [Model(Filter = true)]
        public bool IsCodeRequired { get; set; }

          [Model(Filter = true)]
        public string DiscountCode { get; set; }

        public DiscountLimitType LimitType { get; set; }

        [Model(Show = false)]
        public int Limit { get; set; }

        [Model(Filter = true)]
        public DiscountType DiscountType { get; set; }

        [Model(Show = false)]
        public DiscountCartItemType DiscountCartItemType { get; set; }
        //public string DiscountRuleSystemName { get; set; }

        [Model(Show = false,AjaxEdit=false)]
        [UIHint("ProductShopSearch")]
        public string ProductShopIDs { get; set; }

        

      

        [Model(Show = false)]
        public DateTime? StartDate { get;set; }

        [Model(Show = false)]
        public DateTime? EndDate { get; set; }

        public int DisplayOrder { get; set; }

        [Model(Show = false)]
        public bool PreventNext { get; set; }

        [Model(Role = "Admin", Show = true, Edit = true, AjaxEdit = false)]
        [Model(Show = false, Edit = false)]
        public bool LessShopFee { get; set; }
        //ALTER TABLE [dbo].[Discounts]
        //ADD [LessShopFee] BIT DEFAULT 0 NOT NULL;


        #region Rulescollection

       // [Model(Role="Admin",Show = true)]
        [Model(Show = false, Edit = false)]
        [DataType(DataType.MultilineText)]
        [AllowHtml]
        public string RulesData { get; set; }
        
        [NotMapped]
        [Model(Show = false, Edit = false)]
        private List<JsonKeyValue> _RuleList { get; set; }

        [NotMapped]
        [Model(Show=false,Edit=false)]
        public List<JsonKeyValue> RuleList { 
            get{
                if (_RuleList == null)
                {
                    if(!string.IsNullOrEmpty(RulesData))
                    {
                        System.Web.Script.Serialization.JavaScriptSerializer js = new System.Web.Script.Serialization.JavaScriptSerializer();
                        try
                        {
                            _RuleList = js.Deserialize<List<JsonKeyValue>>(RulesData);
                        }catch(Exception e)
                        {

                        }
                    }
                    
                }
                if (_RuleList == null)
                {
                    _RuleList = new List<JsonKeyValue>();
                }
                return _RuleList;
        }
           
        }

        [NotMapped]
        private Dictionary<string, object> _privateCache = new Dictionary<string, object>();
        
        public object GetRuleConfigObject(string key,Func<object> func)
        {
            if (!_privateCache.ContainsKey(key))
            {
                var res = func();
                _privateCache[key] = res;
                return res;
            }
            return _privateCache[key];
        }

        public List<string> GetProductsList()
        {
           return (List<string>)this.GetRuleConfigObject("", () =>
            {
                if (this.ProductShopIDs == null) { return new List<string>(); }
                return this.ProductShopIDs.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
.Select(x => x.Trim())
.ToList();
            });
        }

        #endregion
        #region cache

        public static void CacheItem(Discount discount,bool isUpdate=true, bool isInsert=false, bool isDelete=false)
        {
            if (LS.Cache["Discount"] != null)
            {
               
                if (!isInsert)
                {
                    foreach (var c in ((Dictionary<string, List<Discount>>)LS.Cache["Discount"]))
                    {
                        var v = c.Value.FirstOrDefault(x => x.ID == discount.ID);
                        if (v != null)
                        {
                            var index = c.Value.IndexOf(v);
                            if (index > -1)
                            {
                                c.Value.RemoveAt(index);
                            }
                        }
                    }
                }
                if(!isDelete && discount.Active)
                {
                    string ckey = "Discount"+discount.ShopID.ToString()+"_" + discount.DiscountType.ToString();
                    if (!((Dictionary<string, List<Discount>>)LS.Cache["Discount"]).ContainsKey(ckey))
                    {
                        ((Dictionary<string, List<Discount>>)LS.Cache["Discount"])[ckey] = new List<Discount>();
                    }
                    ((Dictionary<string, List<Discount>>)LS.Cache["Discount"])[ckey].Add(discount);
                    ((Dictionary<string, List<Discount>>)LS.Cache["Discount"])[ckey] = ((Dictionary<string, List<Discount>>)LS.Cache["Discount"])[ckey].OrderBy(x => x.DisplayOrder).ToList();
                }

            }
        }
        public static void CacheList(List<Discount> list)
        {
            if (LS.Cache["Discount"] != null)
            {
               
                var lookup = list.ToLookup(x => (x.ShopID.ToString()+"_" + x.DiscountType.ToString()));
                foreach(var l in lookup)
                {
                    string ckey = "Discount" + l.Key;
                    ((Dictionary<string, List<Discount>>)LS.Cache["Discount"])[ckey] = l.Where(x=>x.Active).OrderBy(x=>x.DisplayOrder).ToList();
                }
            }
        }

        public static void OnCreating(Discount item)
        {
            LS.Clear<Discount>();
            if (string.IsNullOrEmpty(item.ProductShopIDs)) return;
            foreach (var prodShopId in item.ProductShopIDs.Split(new string[] { "," }, StringSplitOptions.RemoveEmptyEntries))
                LS.Clear("discount_info_" + prodShopId); 
        }
        #endregion
        #region ACL
        public static bool AccessTest(Discount discount, ModelGeneralAttribute attr)
        {
            if (attr.Acl)
            {
                if (LS.CurrentShop != null)
                {
                    return discount.ShopID == LS.CurrentShop.ID;
                }
                return false;
            };
            return true;
        }
        public static IQueryable<Discount> AccessList(ModelGeneralAttribute attr, Dictionary<string, object> param = null)
        {
            int ShopID = 0;
            if (param != null && param.ContainsKey("ShopID"))
            {
                ShopID = int.Parse(param["ShopID"].ToString());
            }
            else if (attr.Acl)
            {
                ShopID = LS.CurrentShop.ID;
            }
            else
            {
                return (
                   from oi in LS.CurrentEntityContext.Discounts
                   select oi);
            }
            return (
               from oi in LS.CurrentEntityContext.Discounts
               where oi.ShopID == ShopID
               select oi);
        }
        #endregion

    }
    public class JsonKeyValue
    {
        public string Name { get; set; }
        public string Value { get; set; }
    }
    public class JsonKeyObject
    {
        public string Name { get; set; }
        public object Object { get; set; }
    }
    public enum DiscountLimitType
    {
        NoLimit = 0,
        PerAll = 1,
        PerCustomer = 2
    }
   

    public enum DiscountType
    {

        ForCartItem = 0,
        ForCartSubTotal = 1,
        ForCartTotal = 2,
        ForCartShip =3,
     //   ForCartFee = 4
    }
    public enum DiscountCartItemType
    {
        ForItemPrice = 0,
        ForItemsTotalPrice = 1,
    }
}

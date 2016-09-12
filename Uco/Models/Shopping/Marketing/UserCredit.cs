using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Mvc;
using Uco.Infrastructure;
using Uco.Infrastructure.Livecycle;
using Uco.Infrastructure.Repositories;
using Uco.Models.Overview;


namespace Uco.Models
{
    [ModelGeneral(Role="Admin" ,AjaxEdit = false, Edit = true, Create = true, CreateAjax = false)]
    [ModelGeneral(Role = "Member",Acl=true,  AjaxEdit = false, Edit = true, Create = true, CreateAjax = false)]
   public partial class UserCredit
    {
        public UserCredit()
        {
           
        }
        //[HiddenInput(DisplayValue = false)]
        [Key]
        [Model(Edit = false, Filter = true, Sort = true, Show = false)]
        [Display(Name = "Main", Order = 1)]
        public int ID { get; set; }

        [Model(Edit = false, AjaxEdit = false, Filter = true, Sort = true, Show = false)]
        [Display(Name = "UserID")]
        [Index]
        [Index("UniqForShop", 0,IsUnique=true)]
        public Guid UserID { get; set; }


        
        [NotMapped]
         [Model( Show = true, Edit = true, FilterOnTop = true)]
        [Display(Name = "User", Order = 100, Prompt = "TabMain")]
        public User User { get; set; }


        [Model(Show = false, Edit = false)]
        [Index("UniqForShop", 1, IsUnique = true)]
        public int ShopID { get; set; }

        [NotMapped]

        [Model(Role = "Admin", Show = true, Edit = true, AjaxEdit = false, FilterOnTop = true)]
        [Model(Show = false, Edit = false)]
        [Display(Name = "Shop", Order = 100, Prompt = "TabMain")]
        public Shop Shop { get; set; }

        [Model(Edit = true, Show = true)]
        [Display(Name = "Note", Order = 100, Prompt = "TabMain")]
        [DataType(DataType.MultilineText)]
        [AllowHtml]
        public string Note { get; set; }


        

        [UIHint("DateTime")]
        [Model(Filter = true,Show=false, Sort = true, Edit = false, AjaxEdit = false)]
        [Display(Name = "CreateTime", Order = 100,  Prompt = "TabMain")]
        [Model(Role = "Member", Edit = false, AjaxEdit = false, Show = false)]
        public DateTime CreateDate { get; set; }

        [UIHint("DateTime")]
        [Model(Filter = true, Show = false, Sort = true, Edit = false, AjaxEdit = false)]
        [Display(Name = "CreateTime", Order = 100,  Prompt = "TabMain")]
        [Model(Role = "Member", Edit = false, AjaxEdit = false, Show = false)]
        public DateTime UpdateDate { get; set; }
        

       
        [Model(Show = true, Edit = true)]
        [Display(Name = "Value", Order = 100,  Prompt = "TabMain")]
        [UIHint("Price")]
        public decimal Value { get; set; }
#region events
        public static string DuplicateError(UserCredit credit)
        {
            return RP.S("Admin.UserCredit.Create.ThisUserAndShopAlreadyAdded");
        }
        public static string InsertGeneralError(UserCredit credit)
        {
            return RP.S("Admin.UserCredit.Create.CantCreateCredits");
        }
#endregion
        #region ACL
        public static bool AccessTest(UserCredit discount, ModelGeneralAttribute attr)
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
        public static IQueryable<UserCredit> AccessList(ModelGeneralAttribute attr, Dictionary<string, object> param = null)
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
                if (attr.Acl && ShopID == 0)
                {
                    return (
                   from oi in LS.CurrentEntityContext.UserCredits
                   where false
                   select oi);
                }
                return (
                   from oi in LS.CurrentEntityContext.UserCredits
                   select oi);
            }
            if (attr.Acl && ShopID == 0)
            {
                return (
               from oi in LS.CurrentEntityContext.UserCredits
               where false
               select oi);
            }
            return (
               from oi in LS.CurrentEntityContext.UserCredits
               where oi.ShopID == ShopID
               select oi);
        }
        #endregion
    }
}
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
     
   public partial class DiscountUsage
    {
        
       [HiddenInput(DisplayValue = false)]
       [Key]
       public int ID { get; set; }

     
       [Model(Show = false, Edit = false)]
       [Index]
       public int ShopID { get; set; }

       [NotMapped]
       [Model(Show = true, Edit = false)]
       public Shop Shop { get; set; }

       [Model(Show = false, Edit = false)]
       public int OrderID { get; set; }

       [NotMapped]
       [Model(Show = true, Edit = false)]
       public Order Order { get; set; }

       [Model(Show = false, Edit = false)]
       [Index("DiscountUser",0)]
       [Index]
       public int DiscountID { get; set; }

       [NotMapped]
       [Model(Show = true, Edit = false)]
       public Discount Discount { get; set; }

       [Model(Edit = false, AjaxEdit = false, Filter = true, Sort = false, Show = false)]
       [Index("DiscountUser", 1)]
       public Guid UserID { get; set; }
       public int UsedTimes { get; set; }

       [Model(Show = false, Edit = false)]
       public string UsageData { get; set; }
        
        #region ACL
       public static bool AccessTest(DiscountUsage discount, ModelGeneralAttribute attr)
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
       public static IQueryable<DiscountUsage> AccessList(ModelGeneralAttribute attr, Dictionary<string, object> param = null)
        {
            int ShopID = 0;
            if (param != null && param.ContainsKey("ShopID"))
            {
                ShopID = (int)param["ShopID"];
            }
            else if (attr.Acl)
            {
                ShopID = LS.CurrentShop.ID;
            }
            else
            {
                return (
                   from oi in LS.CurrentEntityContext.DiscountUsages
                   select oi);
            }
            return (
               from oi in LS.CurrentEntityContext.DiscountUsages
               where oi.ShopID == ShopID
               select oi);
        }
        #endregion

    }
   
}

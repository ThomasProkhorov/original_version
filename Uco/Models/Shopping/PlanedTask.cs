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


namespace Uco.Models
{
    [ModelGeneral( AjaxEdit = false, Edit = true, Create = true, CreateAjax = false, Delete = false)]
    public partial class PlanedTask
    {
       
        [HiddenInput(DisplayValue = false)]
        [Key]
        [Model(Show = true, Edit = false)]
        public int ID { get; set; }

        
       
        [Model(FilterOnTop = true, Sort = true)]
        [Index("System"), StringLength(400)]
        public string SystemName { get; set; }

        [Model(Show = true, Edit = true)]
        [AllowHtml]
        [DataType(DataType.MultilineText)]
        public string Message { get; set; }

        [Model(Show = false, Edit = true)]
        [AllowHtml]
        [DataType(DataType.MultilineText)]
        public string ProcessData { get; set; }

        [Model(Show = false, Edit = false)]
        public Guid UserID { get; set; }


        [Model(Show = false, Edit = false)]
        [Index]
        public int ShopID { get; set; }
       
        [Model( Sort = true)]
        public DateTime? LastStart { get; set; }


        [Model(Sort = true)]
        public DateTime? LastEnd { get; set; }


        [Model(Show = true, Edit = true)]
        public int PercentProgress { get; set; }

        public bool Started { get; set; }

        public bool Active { get; set; }
       
        #region ACL
        public static bool AccessTest(PlanedTask productShop, ModelGeneralAttribute attr)
        {
            if (attr.Acl)
            {
                if (LS.CurrentShop != null)
                {
                    return productShop.ShopID == LS.CurrentShop.ID;
                }
                return false;
            };
            return true;
        }
        public static IQueryable<PlanedTask> AccessList(ModelGeneralAttribute attr, Dictionary<string, object> param = null)
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
                   from oi in LS.CurrentEntityContext.PlanedTasks
                   select oi);
            }
            if(ShopID == 0)
            {
                ShopID = -1;
            }
            return (
               from oi in LS.CurrentEntityContext.PlanedTasks
               where oi.ShopID == ShopID
               select oi);
        }
        #endregion
      }

}

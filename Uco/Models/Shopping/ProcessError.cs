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
    [ModelGeneral( AjaxEdit = false, Edit = false, Create = false, CreateAjax = false, Delete = false)]
    public partial class ProcessError
    {
         
        [HiddenInput(DisplayValue = false)]
        [Key]
        [Model(Show = false, Edit = false)]
        public int ID { get; set; }
        [Model(Filter = true,Sort=true)]
        public string FileServiceName { get; set; }
        public int LineNum { get; set; }
        [Model(Filter = true, Sort = true)]

        [Display(Prompt = "Main")]
        [Model(FilterOnTop = true, Sort = true)]
        [Index("sku"), StringLength(400)]
        public string SKU { get; set; }
        public string Message { get; set; }
        [Model(Show = false, Edit = false)]
        public Guid UserID { get; set; }
        [Model(Show = false, Edit = false)]
        [Index]
        public int ShopID { get; set; }
        [NotMapped]

        [Model(Role = "Admin", Show = true, Edit = true, AjaxEdit = false, FilterOnTop = true)]
        [Model(Show = false, Edit = false)]
        public Shop Shop { get; set; }
        [Model( Sort = true)]
        public DateTime CreateOn { get; set; }
        [Model(Show = false, Edit = false)]
        public string PageUrl { get; set; }
        [Model(Show = false, Edit = false)]
        public string RefererUrl { get; set; }
        [Model(Show = false, Edit = false)]
        public string IP { get; set; }
        #region ACL
        public static bool AccessTest(ProcessError productShop, ModelGeneralAttribute attr)
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
        public static IQueryable<ProcessError> AccessList(ModelGeneralAttribute attr, Dictionary<string, object> param = null)
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
                   from oi in LS.CurrentEntityContext.ProcessErrors
                   select oi);
            }
            if(ShopID == 0)
            {
                ShopID = -1;
            }
            return (
               from oi in LS.CurrentEntityContext.ProcessErrors
               where oi.ShopID == ShopID
               select oi);
        }
        #endregion
      }

}

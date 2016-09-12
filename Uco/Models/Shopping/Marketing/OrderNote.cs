using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Uco.Infrastructure;
using Uco.Infrastructure.Livecycle;
using Uco.Infrastructure.Services;

namespace Uco.Models
{

    [ModelGeneral(
      Role = "Admin", AjaxEdit = false, Edit = false, Create = false, CreateAjax = false,Delete = false )]
    [ModelGeneralAttribute(
        Role = "Member", Acl = true,Create = false, Edit = false, AjaxEdit = false, CreateAjax = true, Delete = false, Show = true, DependedShow = true)]
  
    public class OrderNote
    {
        [HiddenInput(DisplayValue = false)]
        [Key]
        [Model(Show = false, Edit = false)]
        public int ID { get; set; }

        [Model(Show = false, Edit = false)]
        public int OrderID { get; set; }

     

        [Model(Edit=false,AjaxEdit=false)]
        public DateTime CreateDate { get; set; }

        [Model(Edit = false, AjaxEdit = false)]
        public string Field { get; set; }


        [DataType(DataType.MultilineText)]
        public string Note { get; set; }

       
        [Model(Edit = false, AjaxEdit = false)]
        public string OldValue { get; set; }
        [Model(Edit = false, AjaxEdit = false)]

        public string NewValue { get; set; }


        public static OrderNote OnCreated(OrderNote item)
        {
            //send email to client
            var dbcontext = new DBContextService();
            var messageService = new MessageService(dbcontext.EntityContext);
            var orderId = item.OrderID;
           var order = dbcontext.EntityContext.Orders.AsNoTracking().FirstOrDefault(x => x.ID == orderId);
           if (order!=null)
           {
               var shop = LS.Get<Shop>().FirstOrDefault(x => x.ID == order.ShopID);
               messageService.SendNewOrderNoteEmailToUser(item, order, shop);
           }
            return item;
        }
    }
}
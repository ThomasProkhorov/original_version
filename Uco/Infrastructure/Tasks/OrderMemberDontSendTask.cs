using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net.Mail;
using System.Text;
using Uco.Infrastructure.Livecycle;
using Uco.Models;

namespace Uco.Infrastructure.Tasks
{
    public class OrderMemberDontSendTask : ITask
    {

        public string Title { get { return "OrderMemberDontSendTask"; } }
        public int StartSeconds { get { return 100; } }
        public int IntervalSecondsFrom { get { return 600; } }
        public int IntervalSecondsTo { get { return 1200; } }
        public void Execute()
        {
            using (Db _db = new Db())
            {
                DateTime date = DateTime.Now.AddMinutes(-60);

                var orders = _db.Orders.Where(x => !x.NotSentMailed && (x.OrderStatus == OrderStatus.New
                    || (x.OrderStatus == OrderStatus.Paid && (x.PaymentMethod == PaymentMethod.Credit || x.PaymentMethod == PaymentMethod.CreditShopOwner))

                    ) && x.CreateOn < date).ToList();
                if (orders.Count > 0)
                {
                    var messService = new MessageService(_db);
                    foreach (var order in orders)
                    {
                        //  messService.SendOrderUserCantPayToUser(order);
                        var shop = ShoppingService.GetShopByID(order.ShopID);
                        order.NotSentMailed = true;

                        messService.OrderNotSendedToAdmin(order); //no need, already sended
                    }
                    _db.SaveChanges();
                }
            }
        }
    }
}
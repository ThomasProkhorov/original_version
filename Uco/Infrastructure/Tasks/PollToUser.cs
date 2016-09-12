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
    public class PollToUserTask : ITask
    {

        public string Title { get { return "PolToUser"; } }
        public int StartSeconds { get { return 200; } }
        public int IntervalSecondsFrom { get { return 800; } }
        public int IntervalSecondsTo { get { return 1400; } }
        public void Execute()
        {
            using (Db _db = new Db())
            {
                DateTime date = DateTime.Now.AddHours(-4);
                var orders = _db.Orders.Where(x => x.OrderStatus == OrderStatus.Delivered
                    && !x.Questioned
                    && x.DeliveredOn < date).ToList();
                if (orders.Count > 0)
                {
                    var messService = new MessageService(_db);
                    foreach (var order in orders)
                    {

                        var shop = ShoppingService.GetShopByID(order.ShopID);
                        messService.SendQuestionsDeliveredToUser(order, shop);
                        order.Questioned = true;
                        // messService.OrderCanceledEmailToUser(order); //no need, already sended
                    }
                    _db.SaveChanges();
                }
            }
        }
    }
}
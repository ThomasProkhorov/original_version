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
    public class OrderChangeToDeliveriTask : ITask
    {

        public string Title { get { return "OrderChangeToDeliveri"; } }
        public int StartSeconds { get { return 30; } }
        public int IntervalSecondsFrom { get { return 360; } }
        public int IntervalSecondsTo { get { return 400; } }
        public void Execute()
        {
            using (Db _db = new Db())
            {
                DateTime date = DateTime.Now.AddMinutes(-10);
                var orders = _db.Orders.Where(x => x.OrderStatus == OrderStatus.Sent).ToList();
                if (orders.Count > 0)
                {
                    var cur = DateTime.Now;
                    foreach (var order in orders)
                    {
                        var shop = ShoppingService.GetShopByID(order.ShopID);
                        if (!order.SentOn.HasValue || order.SentOn.Value.AddMinutes(shop.DeliveryTime) < cur)
                        {
                            order.OrderStatus = OrderStatus.Delivered;

                        }
                    }
                    _db.SaveChanges();
                }
            }
        }
    }
}
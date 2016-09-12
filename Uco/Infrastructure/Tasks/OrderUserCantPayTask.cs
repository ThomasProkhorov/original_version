using System;
using System.Collections.Generic;
using System.Configuration;
using System.Globalization;
using System.Linq;
using System.Net.Mail;
using System.Text;
using Uco.Infrastructure.Livecycle;
using Uco.Infrastructure.Repositories;
using Uco.Models;

namespace Uco.Infrastructure.Tasks
{
    public class OrderUserCantPayTask : ITask
    {

        public string Title { get { return "OrderUserCantPay"; } }
        public int StartSeconds { get { return 60; } }
        public int IntervalSecondsFrom { get { return 120; } }
        public int IntervalSecondsTo { get { return 180; } }
        public void Execute()
        {
            CultureInfo culture = new CultureInfo("he-IL");
            culture.DateTimeFormat.ShortDatePattern = "dd/MM/yyyy";
            System.Threading.Thread.CurrentThread.CurrentCulture = culture;
            System.Threading.Thread.CurrentThread.CurrentUICulture = culture;
            using (Db _db = new Db())
            {
                DateTime date = DateTime.Now.AddMinutes(-10);

                var orders = _db.Orders.Where(x => x.OrderStatus == OrderStatus.New
                    && (x.PaymentMethod == PaymentMethod.Credit || x.PaymentMethod == PaymentMethod.CreditShopOwner)
                    && x.CreateOn < date).ToList();
                if (orders.Count > 0)
                {
                    var messService = new MessageService(_db);
                    foreach (var order in orders)
                    {
                        messService.SendOrderUserCantPayToUser(order);
                        var shop = ShoppingService.GetShopByID(order.ShopID);
                        messService.SendOrderUserCantPayToMember(order, shop);
                        messService.SendOrderUserCantPayToAdmin(order);
                        order.OrderStatus = OrderStatus.Canceled;

                        var logrecord = new OrderNote()
                        {
                            CreateDate = DateTime.Now,
                            OrderID = order.ID,
                            Note = RP.S("Task.Order.Canceled.NotPaid"),

                        };
                        _db.OrderNotes.Add(logrecord);
                        // messService.OrderCanceledEmailToUser(order); //no need, already sended
                    }
                    _db.SaveChanges();
                }
            }
        }
    }
}
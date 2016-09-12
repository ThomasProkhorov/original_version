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
    public class ClearCartsAndCheckoutDataTask : ITask
    {

        public string Title { get { return "ClearCartsAndCheckoutDataTask"; } }
        public int StartSeconds { get { return 10; } }
        public int IntervalSecondsFrom { get { return 43200; } }
        public int IntervalSecondsTo { get { return 43200; } }
        public void Execute()
        {
            using (Db _db = new Db())
            {
                DateTime date = DateTime.Now.AddDays(-10);
                var cartItems = _db.ShoppingCartItems.Where(x => x.LastDate < date).ToList();
                //foreach(var sci in cartItems)
                //{
                //    sci.LastDate = DateTime.UtcNow;
                //}
                //_db.SaveChanges();

                //cartItems = _db.ShoppingCartItems.Where(x => x.LastDate < date).ToList();
                if (cartItems.Count > 0)
                {
                    _db.ShoppingCartItems.RemoveRange(cartItems);
                    _db.SaveChanges();
                }

                var checkoutDatas = _db.CheckoutDatas.Where(x => x.LastAction < date && !x.IsRegistered).ToList();
                //foreach (var cd in checkoutDatas)
                //{
                //    cd.LastAction = DateTime.UtcNow;
                //    cd.IsRegistered = true;
                //}
                //_db.SaveChanges();
                //checkoutDatas = _db.CheckoutDatas.Where(x => x.LastAction < date && !x.IsRegistered).ToList();
                if (checkoutDatas.Count > 0)
                {
                    _db.CheckoutDatas.RemoveRange(checkoutDatas);
                    _db.SaveChanges();
                }
            }
        }
    }
}
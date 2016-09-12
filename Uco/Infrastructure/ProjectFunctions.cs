using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web;
using Uco.ActiveTrailApi.CustomersServiceProxy;
using Uco.Infrastructure.Livecycle;
using Uco.Infrastructure.Repositories;
using Uco.Models;

namespace Uco.Infrastructure
{
    public static partial class SF
    {
        public static void AddToNewsletter(Newsletter item)
        {
            List<int> ListIDs = new List<int>();
            ListIDs.Add(103435);
            string ClientUser = "couponabiz";
            string ClientPassword = "9IMwjSNLQW";

            try
            {

                CustomerServiceSoapClient client = new CustomerServiceSoapClient();
                AuthHeader authHeader = new AuthHeader();
                authHeader.Username = ClientUser;
                authHeader.Password = ClientPassword;
                authHeader.Token = client.Login(authHeader);


                WebCustomer customer = new WebCustomer();
                customer.Email = item.NewsletterEmail;
                customer.FirstName = item.NewsletterName;
                customer.Phone1 = item.NewsletterPhone;

                APIResponse response = client.ImportCustomer(authHeader, customer, ListIDs.ToArray(), new int[0]);
            }
            catch (Exception ex)
            {
                SF.LogError(ex);
            }
        }

        public static DateTime MoveOrderReadyTimeToWorkHours(Order order)
        {
            return order.CreateOn;
        }
        public static DateTime MoveOrderReadyTimeToShippingHours(Order order)
        {
            return order.CreateOn;
        }

    }
}
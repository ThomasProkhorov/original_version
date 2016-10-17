using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Web;
using Uco.Infrastructure.Livecycle;
using Uco.Infrastructure.Repositories;
using Uco.Models;
using System.Linq;
using System.Web.WebPages.Html;
using Uco.Models.Overview;
using Uco.Infrastructure.Services;
using System.Text;
using System.Reflection;
using System.Web.Mvc;

namespace Uco.Infrastructure
{
    public class MessageService
    {
        public MessageService(Db _db = null, bool isGetTokensOnly = false)
        {
            _Context = new DBContextService(_db);
            _isGetTokensOnly = isGetTokensOnly;
        }
        private bool _isGetTokensOnly;
        private DBContextService _Context = null;
        private string _SMSGateway = "@sms.inforu.co.il"; //"@uco.co.il";//"@sms.inforu.co.il";

        #region Util
        public void CreateAllMissingMessageTemplates()
        {
            MethodInfo[] methodInfos = this.GetType().GetMethods(BindingFlags.Public | BindingFlags.Instance);
            foreach (var m in methodInfos)
            {
                if (m.ReturnParameter != null && m.ReturnParameter.ParameterType.Name == "Int32")
                {
                    try
                    {
                        var pinfo = m.GetParameters();
                        var objs = new List<object>();
                        foreach (var pi in pinfo)
                        {
                            var o = Activator.CreateInstance(pi.ParameterType);
                            objs.Add(o);
                        }
                        m.Invoke(this, objs.ToArray());
                    }
                    catch (Exception e)
                    {

                    }

                }
            }

        }
        #endregion


        public static List<string> GetAvaliableTokensForTemplate(string templateName)
        {
            //set attribute for method
            //[MyAttribute("Hello World")]
            //public int MyMethod()
            //{
            //    //Need to read the MyAttribute attribute and get its value
            //}

            //get attribute method 
            //MethodBase method = MethodBase.GetCurrentMethod();
            //MyAttribute attr = (MyAttribute)method.GetCustomAttributes(typeof(MyAttribute), true)[0];
            //string value = attr.Value; 

            var tokensList = new List<string>();
            if (!string.IsNullOrEmpty(templateName))
            {
                MethodInfo m = typeof(MessageService)
                    .GetMethods(BindingFlags.Public | BindingFlags.Instance)
                    .Where(x => x.GetCustomAttributes(typeof(MailTemplateAttribute), false).Length > 0
                    && ((MailTemplateAttribute)x.GetCustomAttributes(typeof(MailTemplateAttribute), false)
                    .FirstOrDefault()).TemplateSystemName == templateName
                    )
                    .FirstOrDefault();

                if (m != null && m.ReturnParameter != null && m.ReturnParameter.ParameterType.Name == "Int32")
                {
                    try
                    {
                        var pinfo = m.GetParameters();
                        var objs = new List<object>();
                        foreach (var pi in pinfo)
                        {
                            var o = Activator.CreateInstance(pi.ParameterType);
                            objs.Add(o);
                        }
                        // m.Invoke(this, objs.ToArray());
                        var messService = new MessageService(null, true);
                        m.Invoke(messService, objs.ToArray());
                        tokensList = messService.GetCurrentTokens();
                    }
                    catch (Exception e)
                    {

                    }

                }

            }
            return tokensList;
        }
        private List<string> tokens;
        public List<string> GetCurrentTokens()
        {
            return tokens;
        }
        private int Tokenize(string SystemName, string ToEmail, IList<object> models)
        {
            if (_isGetTokensOnly)
            {
                tokens = new List<string>();
                foreach (var o in models)
                {
                    if (o == null)
                    {
                        continue;
                    }

                    var t = o.GetType();
                    var properties = t.GetProperties();
                    foreach (var p in properties)
                    {

                        tokens.Add(string.Format("%{0}.{1}%", t.Name, p.Name)); // %Order.Total% -> $345.34

                    }
                }
                return 0;
            }
            var LanguageCode = SF.GetLangCodeThreading();
            var mailTemplate = _Context.EntityContext.MessageTemplates.FirstOrDefault(x => x.SystemName == SystemName && x.LanguageCode == LanguageCode);
            if (mailTemplate != null && mailTemplate.Active && !string.IsNullOrEmpty(ToEmail))
            {
                StringBuilder Subject = new StringBuilder(mailTemplate.Subject);
                StringBuilder Body = new StringBuilder(mailTemplate.Body);

                string log = "";
                if (mailTemplate.SystemName == "Order.UserCantPay.EmailToAdmin")
                {
                    log += @"LOG mail template data
";
                }
                foreach (var o in models)
                {
                    if (o == null)
                    {
                        continue;
                    }

                    var t = o.GetType();
                    var properties = t.GetProperties();
                    foreach (var p in properties)
                    {
                        string repl = "";
                        var value = p.GetValue(o);
                        if (value != null)
                        {
                            repl = value.ToString();
                        }
                        if (mailTemplate.SystemName == "Order.UserCantPay.EmailToAdmin")
                        {
                            log += string.Format("{0}.{1}", t.Name, p.Name) + @" = " + repl + @"
";
                        }
                        Subject.Replace(string.Format("%{0}.{1}%", t.Name, p.Name), repl); // %Order.Total% -> $345.34
                        Body.Replace(string.Format("%{0}.{1}%", t.Name, p.Name), repl); // %Order.Total% -> $345.34
                    }
                }
                if (mailTemplate.SystemName == "Order.UserCantPay.EmailToAdmin")
                {
                    log += @"
template Body:
" + mailTemplate.Body;
                    log += @"
template Subject:
" + mailTemplate.Subject;

                    log += @"
result Body:
" + Body.ToString();
                    log += @"
result Subject:
" + Subject.ToString();

                    var acitvitylog = new ActivityLog()
                    {
                        CreateOn = DateTime.Now,
                        FullText = log,
                        UserID = Guid.Empty,
                        ActivityType = ActivityType.Other

                    };
                    _Context.EntityContext.ActivityLogs.Add(acitvitylog);
                    _Context.EntityContext.SaveChanges();
                }
                var email = new OutEmail()
               {
                   Body = Body.ToString(),
                   Subject = Subject.ToString(),
                   MailTo = ToEmail,

               };

                _Context.EntityContext.OutEmails.Add(email);
                _Context.EntityContext.SaveChanges();
                return email.ID;

            }
            if (mailTemplate == null)
            {
                //add to log or create automatic
                var template = new MessageTemplate()
                {
                    Active = false,
                    Body = "Auto generated, please change",
                    LanguageCode = SF.GetLangCodeThreading(),
                    Subject = "Auto generated",
                    SystemName = SystemName
                };
                _Context.EntityContext.MessageTemplates.Add(template);
                _Context.EntityContext.SaveChanges();
            }
            return 0;
        }

        [MailTemplate("Nurtest")]
        public int SendNurtestToMember(Order order, Shop shop)
        {
            MethodBase method = MethodBase.GetCurrentMethod();
            MailTemplateAttribute attr = (MailTemplateAttribute)method.GetCustomAttributes(typeof(MailTemplateAttribute), true)[0];
            string templateSystemName = attr.TemplateSystemName;

            var virtualUser = new User()
            {
                Email = order.Email,
                FirstName = order.FullName != null ? order.FullName : "",
                ID = order.UserID,
                Phone = order.Phone,
                UserName = order.Email
            };
            var products = new OrderProductTable(order.ID);
            var res = Tokenize(templateSystemName, order.Email, new List<object>() {
            order,
            virtualUser,
            shop,
            products
            });
            return res;
        }

        [MailTemplate("User.Register.EmailToUser")]
        public int SendUserRegisterEmailToUser(User user)
        {
            //get attribute method 
            MethodBase method = MethodBase.GetCurrentMethod();
            MailTemplateAttribute attr = (MailTemplateAttribute)method.GetCustomAttributes(typeof(MailTemplateAttribute), true)[0];
            string templateSystemName = attr.TemplateSystemName;

            var res = Tokenize(templateSystemName, user.Email, new List<object>() { user });
            return res;
        }

        [MailTemplate("User.SendPassword.EmailToUser")]
        public int SendUserPasswordEmailToUser(User user)
        {
            MethodBase method = MethodBase.GetCurrentMethod();
            MailTemplateAttribute attr = (MailTemplateAttribute)method.GetCustomAttributes(typeof(MailTemplateAttribute), true)[0];
            string templateSystemName = attr.TemplateSystemName;

            var res = Tokenize(templateSystemName, user.Email, new List<object>() { user });
            return res;
        }

        [MailTemplate("Order.Questioned.EmailToMember")]
        public int OrderQuestionedToMember(Order order, Shop shop, List<PollAnswer> Answers)
        {
            MethodBase method = MethodBase.GetCurrentMethod();
            MailTemplateAttribute attr = (MailTemplateAttribute)method.GetCustomAttributes(typeof(MailTemplateAttribute), true)[0];
            string templateSystemName = attr.TemplateSystemName;

            var virtualUser = new User()
            {
                Email = order.Email,
                FirstName = order.FullName != null ? order.FullName : "",
                ID = order.UserID,
                Phone = order.Phone,
                UserName = order.Email
            };
            var mailparams = new List<object>() {
            order,
            virtualUser,
            shop
            };
            mailparams.AddRange(Answers);
            var res = Tokenize(templateSystemName, shop.Email, mailparams);
            return res;
        }

        [MailTemplate("Poll.OrderShop.EmailToUser")]
        public int SendQuestionsDeliveredToUser(Order order, Shop shop)
        {
            MethodBase method = MethodBase.GetCurrentMethod();
            MailTemplateAttribute attr = (MailTemplateAttribute)method.GetCustomAttributes(typeof(MailTemplateAttribute), true)[0];
            string templateSystemName = attr.TemplateSystemName;

            //http://localhost:23416/poll/index?OrderID=30&ShopID=3
            PollPageModel model = new PollPageModel();
            var domain = _Context.EntityContext.SettingsAll.Select(x => x.Domain).FirstOrDefault();
            model.PollUrl = string.Format("{0}://{1}/poll/index?OrderID={2}&ShopID={3}",
                "http",
                domain,
                order.ID.ToString(),
                shop.ID.ToString());
            var virtualUser = new User()
            {
                Email = order.Email,
                FirstName = order.FullName != null ? order.FullName : "",
                ID = order.UserID,
                Phone = order.Phone,
                UserName = order.Email
            };
            var res = Tokenize(templateSystemName, order.Email, new List<object>() {
            order,
            virtualUser,
            shop,
            model
            });
            return res;
        }

        [MailTemplate("Order.Payed.EmailToUser")]
        public int SendOrderPayedEmailToUser(Order order, Shop shop)
        {
            MethodBase method = MethodBase.GetCurrentMethod();
            MailTemplateAttribute attr = (MailTemplateAttribute)method.GetCustomAttributes(typeof(MailTemplateAttribute), true)[0];
            string templateSystemName = attr.TemplateSystemName;

            var virtualUser = new User()
            {
                Email = order.Email,
                FirstName = order.FullName != null ? order.FullName : "",
                ID = order.UserID,
                Phone = order.Phone,
                UserName = order.Email
            };
            var res = Tokenize(templateSystemName, order.Email, new List<object>() {
            order,
            virtualUser,
            shop
            });
            return res;
        }

        [MailTemplate("Order.Payed.EmailToMember")]
        public int SendOrderPayedEmailToMember(Order order, Shop shop)
        {
            MethodBase method = MethodBase.GetCurrentMethod();
            MailTemplateAttribute attr = (MailTemplateAttribute)method.GetCustomAttributes(typeof(MailTemplateAttribute), true)[0];
            string templateSystemName = attr.TemplateSystemName;

            var virtualUser = new User()
            {
                Email = order.Email,
                FirstName = order.FullName != null ? order.FullName : "",
                ID = order.UserID,
                Phone = order.Phone,
                UserName = order.Email
            };
            var ordernote = _Context.EntityContext.OrderNotes.FirstOrDefault(o => o.OrderID == order.ID);

            var res = Tokenize(templateSystemName, shop.Email, new List<object>() {
            order,
            virtualUser,
            shop,
            ordernote
            });
            return res;
        }

        [MailTemplate("Order.New.EmailToUser")]
        public int SendNewOrderEmailToUser(Order order, Shop shop)
        {
            MethodBase method = MethodBase.GetCurrentMethod();
            MailTemplateAttribute attr = (MailTemplateAttribute)method.GetCustomAttributes(typeof(MailTemplateAttribute), true)[0];
            string templateSystemName = attr.TemplateSystemName;

            var virtualUser = new User()
            {
                Email = order.Email,
                FirstName = order.FullName != null ? order.FullName : "",
                ID = order.UserID,
                Phone = order.Phone,
                UserName = order.Email
            };
            var products = new OrderProductTable(order.ID);
            var res = Tokenize(templateSystemName, order.Email, new List<object>() {
            order,
            virtualUser,
            shop,
            products
            });
            return res;
        }

        [MailTemplate("Order.NewNote.EmailToUser")]
        public int SendNewOrderNoteEmailToUser(OrderNote note, Order order, Shop shop)
        {
            MethodBase method = MethodBase.GetCurrentMethod();
            MailTemplateAttribute attr = (MailTemplateAttribute)method.GetCustomAttributes(typeof(MailTemplateAttribute), true)[0];
            string templateSystemName = attr.TemplateSystemName;

            var virtualUser = new User()
             {
                 Email = order.Email,
                 FirstName = order.FullName != null ? order.FullName : "",
                 ID = order.UserID,
                 Phone = order.Phone,
                 UserName = order.Email
             };
            var products = new OrderProductTable(order.ID);
            var res = Tokenize(templateSystemName, order.Email, new List<object>() {
                note,
            order,
            virtualUser,
            shop,
            products
            });
            return res;
        }

        [MailTemplate("Order.New.SmsToUser")]
        public int SendNewOrderSMSToUser(Order order, Shop shop, User user)
        {
            MethodBase method = MethodBase.GetCurrentMethod();
            MailTemplateAttribute attr = (MailTemplateAttribute)method.GetCustomAttributes(typeof(MailTemplateAttribute), true)[0];
            string templateSystemName = attr.TemplateSystemName;

            var virtualUser = new User()
              {
                  Email = order.Email,
                  FirstName = order.FullName != null ? order.FullName : "",
                  ID = order.UserID,
                  Phone = order.Phone,
                  UserName = order.Email
              };
            if (user != null && user.Phone != null || _isGetTokensOnly)
            {
                if (user.Phone == null) { user.Phone = ""; }
                var res = Tokenize(templateSystemName, user.Phone.Replace(" ", "").Replace("-", "") + _SMSGateway, new List<object>() {
            order,
            user,
            shop
            });
                return res;
            }
            return 0;
        }

        [MailTemplate("Order.New.EmailToMember")]
        public int SendNewOrderEmailToMember(Order order, Shop shop)
        {
            MethodBase method = MethodBase.GetCurrentMethod();
            MailTemplateAttribute attr = (MailTemplateAttribute)method.GetCustomAttributes(typeof(MailTemplateAttribute), true)[0];
            string templateSystemName = attr.TemplateSystemName;

            var virtualUser = new User()
              {
                  Email = order.Email,
                  FirstName = order.FullName != null ? order.FullName : "",
                  ID = order.UserID,
                  Phone = order.Phone,
                  UserName = order.Email
              };
            var products = new OrderProductTable(order.ID);            

            var res = Tokenize(templateSystemName, shop.Email, new List<object>() {
            order,
            virtualUser,
            shop,
            products
            });
            return res;
        }

        [MailTemplate("Order.New.SmsToMember")]
        public int SendNewOrderSMSToMember(Order order, Shop shop)
        {
            MethodBase method = MethodBase.GetCurrentMethod();
            MailTemplateAttribute attr = (MailTemplateAttribute)method.GetCustomAttributes(typeof(MailTemplateAttribute), true)[0];
            string templateSystemName = attr.TemplateSystemName;

            if (shop.Phone != null || _isGetTokensOnly)
            {
                if (shop.Phone == null) { shop.Phone = ""; }
                var virtualUser = new User()
                {
                    Email = order.Email,
                    FirstName = order.FullName != null ? order.FullName : "",
                    ID = order.UserID,
                    Phone = order.Phone,
                    UserName = order.Email
                };
                var res = Tokenize(templateSystemName, (shop.Phone2 ?? shop.Phone).Replace(" ", "").Replace("-", "") + _SMSGateway, new List<object>() {
            order,
            virtualUser,
            shop
            });
                return res;
            }
            return 0;
        }

        [MailTemplate("Order.Changed.ToUser")]
        public int SendOrderChangedEmailToUser(Order order, List<OrderItem> changed, List<OrderItem> outOfStock)
        {
            MethodBase method = MethodBase.GetCurrentMethod();
            MailTemplateAttribute attr = (MailTemplateAttribute)method.GetCustomAttributes(typeof(MailTemplateAttribute), true)[0];
            string templateSystemName = attr.TemplateSystemName;

            var productChangeds = new OrderChangedProductTable(changed);
            var productRemoved = new OrderRemovedProductTable(outOfStock);
            var shop = LS.Get<Shop>().FirstOrDefault(x => x.ID == order.ShopID);
            var virtualUser = new User()
            {
                Email = order.Email,
                FirstName = order.FullName != null ? order.FullName : "",
                ID = order.UserID,
                Phone = order.Phone,
                UserName = order.Email
            };
            var res = Tokenize(templateSystemName, order.Email, new List<object>() {
            order
            ,shop
            ,virtualUser
            ,productChangeds
            ,productRemoved
            });
            return res;
        }

        [MailTemplate("Order.Changed.SmsToUser")]
        public int SendOrderChangedSmsToUser(Order order, User user)
        {
            MethodBase method = MethodBase.GetCurrentMethod();
            MailTemplateAttribute attr = (MailTemplateAttribute)method.GetCustomAttributes(typeof(MailTemplateAttribute), true)[0];
            string templateSystemName = attr.TemplateSystemName;


            if (user != null && user.Phone != null || _isGetTokensOnly)
            {
                if (user.Phone == null) { user.Phone = ""; }
                var res = Tokenize(templateSystemName, user.Phone.Replace(" ", "").Replace("-", "") + _SMSGateway, new List<object>() {
            order,
            user
            });
                return res;
            }
            return 0;
        }

        [MailTemplate("Checkout.SmsConfirm.SmsToUser")]
        public int CheckoutSmsConfirmSmsToUser(CheckoutData checkooutData, User user)
        {
            MethodBase method = MethodBase.GetCurrentMethod();
            MailTemplateAttribute attr = (MailTemplateAttribute)method.GetCustomAttributes(typeof(MailTemplateAttribute), true)[0];
            string templateSystemName = attr.TemplateSystemName;

            if (user != null && checkooutData.Phone != null || _isGetTokensOnly)
            {
                if (checkooutData.Phone == null) { checkooutData.Phone = ""; }
                var res = Tokenize(templateSystemName, checkooutData.Phone.Replace(" ", "").Replace("-", "") + _SMSGateway, new List<object>() {
                    user,
            checkooutData
            });
                return res;
            }
            return 0;
        }

        [MailTemplate("Order.Canceled.SmsToUser")]
        public int OrderCanceledSmsToUser(Order order, User user)
        {
            MethodBase method = MethodBase.GetCurrentMethod();
            MailTemplateAttribute attr = (MailTemplateAttribute)method.GetCustomAttributes(typeof(MailTemplateAttribute), true)[0];
            string templateSystemName = attr.TemplateSystemName;

            if (user != null && user.Phone != null || _isGetTokensOnly)
            {
                if (user.Phone == null) { user.Phone = ""; }
                var res = Tokenize(templateSystemName, user.Phone.Replace(" ", "").Replace("-", "") + _SMSGateway, new List<object>() {
            order,
            user
            });
                return res;
            }
            return 0;
        }

        [MailTemplate("Order.NotSended.EmailToAdmin")]
        public int OrderNotSendedToAdmin(Order order)
        {
            MethodBase method = MethodBase.GetCurrentMethod();
            MailTemplateAttribute attr = (MailTemplateAttribute)method.GetCustomAttributes(typeof(MailTemplateAttribute), true)[0];
            string templateSystemName = attr.TemplateSystemName;

            var virtualUser = new User()
            {
                Email = order.Email,
                FirstName = order.FullName != null ? order.FullName : "",
                ID = order.UserID,
                Phone = order.Phone,
                UserName = order.Email
            };
            var domain = _Context.EntityContext.SettingsAll.Select(x => x.AdminEmail).FirstOrDefault();
            var shop = LS.Get<Shop>().FirstOrDefault(x => x.ID == order.ShopID);
            var res = Tokenize(templateSystemName, domain, new List<object> { order, shop, virtualUser });
            return res;
        }

        [MailTemplate("Order.Canceled.EmailToUser")]
        public int OrderCanceledEmailToUser(Order order, Shop shop)
        {
            MethodBase method = MethodBase.GetCurrentMethod();
            MailTemplateAttribute attr = (MailTemplateAttribute)method.GetCustomAttributes(typeof(MailTemplateAttribute), true)[0];
            string templateSystemName = attr.TemplateSystemName;

            var virtualUser = new User()
            {
                Email = order.Email,
                FirstName = order.FullName != null ? order.FullName : "",
                ID = order.UserID,
                Phone = order.Phone,
                UserName = order.Email
            };
            var res = Tokenize(templateSystemName, order.Email, new List<object>() {
            order,
            virtualUser,
            shop
            });
            return res;
        }

        [MailTemplate("Order.UserCantPay.ToUser")]
        public int SendOrderUserCantPayToUser(Order order)
        {
            MethodBase method = MethodBase.GetCurrentMethod();
            MailTemplateAttribute attr = (MailTemplateAttribute)method.GetCustomAttributes(typeof(MailTemplateAttribute), true)[0];
            string templateSystemName = attr.TemplateSystemName;
            var shop = LS.Get<Shop>().FirstOrDefault(x => x.ID == order.ShopID);
            if (shop == null)
            {
                shop = new Shop();
            }
            var virtualUser = new User()
            {
                Email = order.Email,
                FirstName = order.FullName != null ? order.FullName : "",
                ID = order.UserID,
                Phone = order.Phone,
                UserName = order.Email
            };
            var res = Tokenize(templateSystemName, order.Email, new List<object>() {
            order
            ,virtualUser
            ,shop
            });
            return res;

        }

        [MailTemplate("Order.UserCantPay.EmailToMember")]
        public int SendOrderUserCantPayToMember(Order order, Shop shop)
        {
            MethodBase method = MethodBase.GetCurrentMethod();
            MailTemplateAttribute attr = (MailTemplateAttribute)method.GetCustomAttributes(typeof(MailTemplateAttribute), true)[0];
            string templateSystemName = attr.TemplateSystemName;

            var virtualUser = new User()
            {
                Email = order.Email,
                FirstName = order.FullName != null ? order.FullName : "",
                ID = order.UserID,
                Phone = order.Phone,
                UserName = order.Email
            };
            var res = Tokenize(templateSystemName, shop.Email, new List<object>() {
            order,
            virtualUser,
            shop
            });
            return res;

        }

        [MailTemplate("Order.UserCantPay.EmailToAdmin")]
        public int SendOrderUserCantPayToAdmin(Order order)
        {
            MethodBase method = MethodBase.GetCurrentMethod();
            MailTemplateAttribute attr = (MailTemplateAttribute)method.GetCustomAttributes(typeof(MailTemplateAttribute), true)[0];
            string templateSystemName = attr.TemplateSystemName;

            var domain = _Context.EntityContext.SettingsAll.Select(x => x.AdminEmail).FirstOrDefault();
            var shop = LS.Get<Shop>().FirstOrDefault(x => x.ID == order.ShopID);
            if (shop == null)
            {
                shop = new Shop();
            }
            var virtualUser = new User()
            {
                Email = order.Email,
                FirstName = order.FullName != null ? order.FullName : "",
                ID = order.UserID,
                Phone = order.Phone,
                UserName = order.Email
            };
            var res = Tokenize(templateSystemName, domain, new List<object>() {
            order      ,shop  
            ,virtualUser
            });
            return res;
        }
    }

}
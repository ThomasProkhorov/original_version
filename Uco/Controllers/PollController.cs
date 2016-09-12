using System.Web.Mvc;
using Uco.Infrastructure;
using Uco.Models;
using System.Linq;
using System.Collections.Generic;
using Uco.Infrastructure.Livecycle;
using Uco.Models.Overview;
using Kendo.Mvc.UI;
using Kendo.Mvc.Extensions;
using Kendo.Mvc;
using Uco.Infrastructure.Repositories;
using System;
using System.Web.Security;

namespace Uco.Controllers
{


    [Localization]
    public partial class PollController : BaseController
    {
        public ActionResult Index(PollPageModel model)
        {
            if ((!model.token.HasValue || model.token == Guid.Empty) && LS.isLogined())
            {
                model.token = LS.CurrentUser.ID;
            }
            if (!model.token.HasValue || model.token == Guid.Empty)
            {
                return RedirectToAction("LogOn", "Account", new { returnUrl = Url.Action("Index", "Poll") });
            }

            return View(model);
        }

        public ActionResult Block(string SystemName, PollPageModel model)
        {
            if (!model.token.HasValue || model.token == Guid.Empty)
            {
                return Content("");
            }
            var poll = _db.Polls.FirstOrDefault(x => x.SystemName == SystemName);
            if (poll == null)
            {
                return Content(""); //no poll founded
            }
            if (poll.AssignType == PollAssignType.Order && model.OrderID == 0)
            {
                return Content("");//no order set
            }
            if (poll.AssignType == PollAssignType.Shop && model.ShopID == 0)
            {
                return Content("");//no shop set
            }
            if (poll.AssignType == PollAssignType.Product && model.ProductID == 0)
            {
                return Content("");//no product set
            }
            if (model.OrderID > 0)
            {
                if (HttpContext.Items["CurrentOrder"] == null)
                {
                    HttpContext.Items["CurrentOrder"] = _db.Orders.FirstOrDefault(x => x.ID == model.OrderID);
                    //saving for next block child action
                }
                var order = HttpContext.Items["CurrentOrder"] as Order;
                if (order == null || order.UserID != model.token)
                {
                    return Content("");// it`s not your order!!!
                }
            }
            poll.Variants = _db.PollVariants.Where(x => x.PollID == poll.ID).OrderBy(x => x.DisplayOrder).ToList();
            return PartialView(poll);

        }
        [HttpPost]
        public ActionResult Vote(PollPageModel model)
        {
            if (!model.token.HasValue || model.token == Guid.Empty)
            {
                return Json(new { result = "error", message = "Please log in" });
            }
            var answers = new List<PollAnswer>();
            Order order = null;
            Shop shop = null;
            if (model.ShopID > 0)
            {
                shop = ShoppingService.GetShopByID(model.ShopID);
            }
            foreach (var fp in Request.Form.AllKeys)
            {
                if (fp.StartsWith("poll"))
                {
                    int pollID = 0;
                    int pollVariantID = 0;
                    int.TryParse(fp.Replace("poll", ""), out pollID);
                    if (pollID > 0)
                    {

                        int.TryParse(Request.Form[fp], out pollVariantID);
                        if (pollVariantID > 0)
                        {
                            //get poll 
                            var poll = _db.Polls.FirstOrDefault(x => x.ID == pollID);
                            if (poll != null)
                            {
                                //validation
                                if (poll.AssignType == PollAssignType.Order && model.OrderID == 0)
                                {
                                    continue;//no order set
                                }
                                if (poll.AssignType == PollAssignType.Shop && model.ShopID == 0)
                                {
                                    continue;//no shop set
                                }
                                if (poll.AssignType == PollAssignType.Product && model.ProductID == 0)
                                {
                                    continue;//no product set
                                }
                                if (model.OrderID > 0)
                                {
                                    if (HttpContext.Items["CurrentOrder"] == null)
                                    {
                                        HttpContext.Items["CurrentOrder"] = _db.Orders.FirstOrDefault(x => x.ID == model.OrderID);
                                        //saving for next block child action
                                    }
                                    order = HttpContext.Items["CurrentOrder"] as Order;
                                    if (poll.AssignType == PollAssignType.Order
                                        && (
                                        order == null || order.UserID != model.token.Value)
                                        )
                                    {
                                        continue;// it`s not your order!!!
                                    }
                                }
                                //create or update poll vote
                                var voteQuery = _db.PollAnswers.Where(x => x.PollID == pollID
                                    && x.UserID == model.token.Value
                                    );
                                if (poll.AssignType == PollAssignType.Order)
                                {
                                    voteQuery = voteQuery.Where(x => x.OrderID == model.OrderID);
                                }
                                if (poll.AssignType == PollAssignType.Shop)
                                {
                                    voteQuery = voteQuery.Where(x => x.ShopID == model.ShopID);
                                }
                                if (poll.AssignType == PollAssignType.Product)
                                {
                                    voteQuery = voteQuery.Where(x => x.ProductID == model.ProductID);
                                }
                                var vote = voteQuery.FirstOrDefault();
                                if (vote != null)
                                {
                                    vote.Rate = pollVariantID;
                                    vote.PollVariantID = pollVariantID;

                                    _db.SaveChanges();
                                }
                                else
                                {
                                    vote = new PollAnswer()
                                    {
                                        PollID = pollID,
                                        PollVariantID = pollVariantID,
                                        Rate = pollVariantID,
                                        UserID = model.token.Value
                                    };
                                    if (poll.AssignType == PollAssignType.Order)
                                    {
                                        vote.OrderID = model.OrderID;
                                    }
                                    if (poll.AssignType == PollAssignType.Shop)
                                    {
                                        vote.ShopID = model.ShopID;
                                    }
                                    if (poll.AssignType == PollAssignType.Product)
                                    {
                                        vote.ProductID = model.ProductID;
                                    }
                                    _db.PollAnswers.Add(vote);
                                    _db.SaveChanges();
                                }
                                vote.Poll = poll;
                                if (!poll.IsRating)
                                {
                                    vote.PollVariant = _db.PollVariants.FirstOrDefault(x => x.ID == vote.PollVariantID);
                                }
                                answers.Add(vote);
                            }
                        }

                    }
                }
            }
            if (answers.Count > 0)
            {
                var messService = new MessageService(_db);
                messService.OrderQuestionedToMember(order, shop, answers);
            }
            return Json(new { result = "ok", message = "Thank you for your vote" });
        }
    }
}

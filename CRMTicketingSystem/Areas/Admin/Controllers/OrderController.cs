using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using CRMTicketingSystem.DataAccess.Data;
using CRMTicketingSystem.DataAccess.Repository.IRepository;
using CRMTicketingSystem.Enum;
using CRMTicketingSystem.Models;
using CRMTicketingSystem.Models.ViewModels;
using CRMTicketingSystem.Utility;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Stripe;
using Twilio;
using Twilio.Rest.Api.V2010.Account;

namespace CRMTicketingSystem.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize]
    public class OrderController : Controller
    {
        private readonly IUnitOfWork _unitofwork;
        private readonly ApplicationDbContext _db;
        [BindProperty]
        public OrderDetailsVM OrderVM { get; set; }
        private TwilioSettings _twilioOptions { get; set; }

        public OrderController(IUnitOfWork unitofwork, IOptions<TwilioSettings> twilioOptions, ApplicationDbContext db)
        {
            _unitofwork = unitofwork;
            _twilioOptions = twilioOptions.Value;
            _db = db;
        }
        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Details(int id)
        {
            OrderVM = new OrderDetailsVM()
            {
                OrderHeader = _unitofwork.OrderHeader.GetFirstOrDefault(u => u.Id == id,
                    includeProperties: "ApplicationUser"),
                OrderDetails = _unitofwork.OrderDetails.GetAll(o => o.OrderId == id, includeProperties: "Product")
            };
            return View(OrderVM);
        }

        [Authorize(Roles = SD.Role_Employee + "," + SD.Role_Admin)]
        public IActionResult StartProcessing(int id)
        {
            OrderHeader orderHeader = _unitofwork.OrderHeader.GetFirstOrDefault(u => u.Id == id);
            orderHeader.OrderStatus = SD.StatusInProcess;
            _unitofwork.Save();

            //code sms in processing
            SmsTemplate smsTemplate = _db.SmsTemplates.Where(e => e.Id == Convert.ToInt32(EnSmsTemplate.OrderInProcessing)).FirstOrDefault();
            smsTemplate.Content = smsTemplate.Content.Replace("###Name###", orderHeader.Name);
            TwilioClient.Init(_twilioOptions.AccountSid, _twilioOptions.AuthToken);
            try
            {
                var message = MessageResource.Create(
                   from: new Twilio.Types.PhoneNumber(_twilioOptions.PhoneNumber),
                   to: new Twilio.Types.PhoneNumber(orderHeader.PhoneNumber),
                   body: smsTemplate.Content + orderHeader.Id);

            }

            catch (Exception x)
            {

            }


            return RedirectToAction("Index");
        }

        [HttpPost]
        [Authorize(Roles = SD.Role_Employee + "," + SD.Role_Admin)]
        public IActionResult ShipOrder()
        {
            OrderHeader orderHeader = _unitofwork.OrderHeader.GetFirstOrDefault(u => u.Id == OrderVM.OrderHeader.Id);
            orderHeader.TrackingNumber = OrderVM.OrderHeader.TrackingNumber;
            orderHeader.Carrier = OrderVM.OrderHeader.Carrier;
            orderHeader.OrderStatus = SD.StatusShipped;
            orderHeader.ShippingDate = DateTime.Now;
            _unitofwork.Save();

            //code SMS for ShipOrder
            SmsTemplate smsTemplate = _db.SmsTemplates.Where(e => e.Id == Convert.ToInt32(EnSmsTemplate.ShipOrder)).FirstOrDefault();
            smsTemplate.Content = smsTemplate.Content.Replace("###Name###", orderHeader.Name);
            TwilioClient.Init(_twilioOptions.AccountSid, _twilioOptions.AuthToken);
            try
            {
                var message = MessageResource.Create(
                   from: new Twilio.Types.PhoneNumber(_twilioOptions.PhoneNumber),
                   to: new Twilio.Types.PhoneNumber(orderHeader.PhoneNumber),
                   body: smsTemplate.Content + orderHeader.Id);

            }

            catch (Exception x)
            {

            }

            return RedirectToAction("Index");
        }

        [Authorize(Roles = SD.Role_Admin + "," + SD.Role_Employee)]
        public IActionResult CancelOrder(int id)
        {
            OrderHeader orderHeader = _unitofwork.OrderHeader.GetFirstOrDefault(u => u.Id == id);
            if (orderHeader.PaymentStatus == SD.StatusApproved)
            {
                var options = new RefundCreateOptions
                {
                    Amount = Convert.ToInt32(orderHeader.OrderTotal * 100),
                    Reason = RefundReasons.RequestedByCustomer,
                    Charge = orderHeader.TransactionId

                };
                try
                {
                    var service = new RefundService();
                    Refund refund = service.Create(options);
                }
                catch(Exception x)
                {

                }
                orderHeader.OrderStatus = SD.StatusRefunded;
                orderHeader.PaymentStatus = SD.StatusRefunded;
            }
            else
            {
                orderHeader.OrderStatus = SD.StatusCancelled;
                orderHeader.PaymentStatus = SD.StatusCancelled;
            }

            _unitofwork.Save();

            //code SMS for CancelOrder
            SmsTemplate smsTemplate = _db.SmsTemplates.Where(e => e.Id == Convert.ToInt32(EnSmsTemplate.CancelOrder)).FirstOrDefault();
            smsTemplate.Content = smsTemplate.Content.Replace("###Name###", orderHeader.Name);
            TwilioClient.Init(_twilioOptions.AccountSid, _twilioOptions.AuthToken);
            try
            {
                var message = MessageResource.Create(
                   from: new Twilio.Types.PhoneNumber(_twilioOptions.PhoneNumber),
                   to: new Twilio.Types.PhoneNumber(orderHeader.PhoneNumber),
                   body : smsTemplate.Content);
            }

            catch (Exception x)
            {

            }
            return RedirectToAction("Index");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [ActionName("Details")]
        public IActionResult Details(string stripeToken)
        {
            OrderHeader orderHeader = _unitofwork.OrderHeader.GetFirstOrDefault(u => u.Id == OrderVM.OrderHeader.Id,
                includeProperties: "ApplicationUser");
            if (stripeToken != null)
            {
                //Process of payment Here
                var options = new ChargeCreateOptions
                {
                    Amount = Convert.ToInt32(orderHeader.OrderTotal * 100),
                    Currency = "inr",
                    Description = "Order ID :" + orderHeader.Id,
                    Source = stripeToken
                };
                var service = new ChargeService();
                Charge charge = service.Create(options);

                if (charge.BalanceTransactionId == null)
                {
                    orderHeader.PaymentStatus = SD.PaymentStatusRejected;
                }
                else
                {
                    orderHeader.TransactionId = charge.BalanceTransactionId;
                }
                if (charge.Status.ToLower() == "succeeded")
                {
                    orderHeader.PaymentStatus = SD.PaymentStatusApproved;
                    orderHeader.PaymentDate = DateTime.Now;
                }

                _unitofwork.Save();
               
            }
            return RedirectToAction("Details", "Order", new { id = orderHeader.Id });
        }

        #region API CALLS
        [HttpGet]
        public IActionResult GetOrderList(string status)
        {
            var claimsIdentity = (ClaimsIdentity)User.Identity;
            var claim = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier);

            IEnumerable<OrderHeader> orderHeaderList;

            if(User.IsInRole(SD.Role_Admin) || User.IsInRole(SD.Role_Employee))
            {
                orderHeaderList = _unitofwork.OrderHeader.GetAll(includeProperties: "ApplicationUser");
            }
            else
            {
                orderHeaderList = _unitofwork.OrderHeader.GetAll(u=>u.ApplicationUserId == claim.Value,
                    includeProperties: "ApplicationUser");
            }

            switch (status)
            {
                case "pending":
                    orderHeaderList = orderHeaderList.Where(i => i.PaymentStatus == SD.PaymentStatusDelayedPayment);
                    break;
                case "inprocess":
                    orderHeaderList = orderHeaderList.Where(i => i.OrderStatus == SD.StatusApproved || 
                                                            i.OrderStatus == SD.StatusInProcess ||
                                                            i.OrderStatus == SD.StatusPending);
                    break;
                case "completed":
                    orderHeaderList = orderHeaderList.Where(i => i.OrderStatus == SD.StatusShipped);
                    break;
                case "rejected":
                    orderHeaderList = orderHeaderList.Where(i => i.OrderStatus == SD.StatusCancelled ||
                                                            i.OrderStatus == SD.StatusRefunded ||
                                                            i.OrderStatus == SD.PaymentStatusRejected);
                    break;
                default:
                    break;
            }

            return Json(new { data = orderHeaderList });
        }

        #endregion
    }
}
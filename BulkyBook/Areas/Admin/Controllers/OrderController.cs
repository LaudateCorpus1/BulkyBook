using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using BulkyBook.DataAccess.Repository.IRepository;
using BulkyBook.Models;
using BulkyBook.Models.ViewModels;
using BulkyBook.Utility;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Stripe;

namespace BulkyBook.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize]
    public class OrderController : Controller
    {
        private readonly IUnitOfWork _uow;
        [BindProperty]
        public OrderDetailsVM DetailsVM { get; set; }

        public OrderController(IUnitOfWork unitOfWork)
        {
            _uow = unitOfWork;
        }
        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Details(int id)
        {
            DetailsVM = new OrderDetailsVM()
            {
                OrderHeader = _uow.OrderHeader.GetFirstOrDefault(a => a.Id == id, includeProperties: "ApplicationUser"),
                OrderDetails = _uow.OrderDetails.GetAll(a => a.OrderId == id, includeProperties: "Product")
            };
            return View(DetailsVM);
        }

        [HttpGet]
        public IActionResult GetOrderList(string status)
        {
            var claimsIdentity = (ClaimsIdentity)User.Identity;
            var claim = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier);

            IEnumerable<OrderHeader> orderHeadersList;

            if (User.IsInRole(SD.Roles.Admin) || User.IsInRole(SD.Roles.Employee))
            {
                orderHeadersList = _uow.OrderHeader.GetAll(includeProperties: "ApplicationUser");
            }
            else
            {
                orderHeadersList = _uow.OrderHeader.GetAll(a => a.ApplicationUserId == claim.Value, includeProperties: "ApplicationUser");
            }

            switch (status)
            {
                case "pending":
                    orderHeadersList = orderHeadersList.Where(a => a.PaymentStatus == SD.PaymentStatus.Pending);
                    break;
                case "completed":
                    orderHeadersList = orderHeadersList.Where(a => a.OrderStatus == SD.OrderStatus.Approved || a.OrderStatus == SD.OrderStatus.Processing || a.OrderStatus == SD.OrderStatus.Pending);
                    break;
                case "inprocess":
                    orderHeadersList = orderHeadersList.Where(a => a.OrderStatus == SD.OrderStatus.Shipped);
                    break;
                case "rejected":
                    orderHeadersList = orderHeadersList.Where(a => a.OrderStatus == SD.OrderStatus.Cancelled || a.OrderStatus == SD.OrderStatus.Refunded || a.PaymentStatus == SD.PaymentStatus.Rejected);
                    break;
                default:
                    break;
            }

            return Json(new { data = orderHeadersList });
        }

        [Authorize(Roles = SD.Roles.Admin + "," + SD.Roles.Employee)]
        public IActionResult StartProcessing(int id)
        {
            OrderHeader orderHeader = _uow.OrderHeader.GetFirstOrDefault(a => a.Id == id);
            orderHeader.OrderStatus = SD.OrderStatus.Processing;
            _uow.Save();
            return RedirectToAction("Index");
        }

        [Authorize(Roles = SD.Roles.Admin + "," + SD.Roles.Employee)]
        [HttpPost]
        public IActionResult ShipOrder()
        {
            OrderHeader orderHeader = _uow.OrderHeader.GetFirstOrDefault(a => a.Id == DetailsVM.OrderHeader.Id);
            orderHeader.TrackingNumber = DetailsVM.OrderHeader.TrackingNumber;
            orderHeader.Carrier = DetailsVM.OrderHeader.Carrier;
            orderHeader.OrderStatus = SD.OrderStatus.Shipped;
            orderHeader.PaymentStatus = SD.PaymentStatus.ApprovedForDelayedPayment;
            orderHeader.ShippingDate = DateTime.Now;
            _uow.Save();
            return RedirectToAction("Index");
        }

        [Authorize(Roles = SD.Roles.Admin + "," + SD.Roles.Employee)]
        public IActionResult CancelOrder(int id)
        {
            OrderHeader orderHeader = _uow.OrderHeader.GetFirstOrDefault(a => a.Id == id);
            if (orderHeader.PaymentStatus == SD.PaymentStatus.Approved)
            {
                var option = new RefundCreateOptions()
                {
                    Amount = Convert.ToInt32(orderHeader.OrderTotal * 100),
                    Reason = RefundReasons.RequestedByCustomer,
                    Charge = orderHeader.TransactionId
                };
                var service = new RefundService();
                Refund refund = service.Create(option);
                orderHeader.OrderStatus = SD.OrderStatus.Refunded;
                orderHeader.PaymentStatus = SD.OrderStatus.Refunded;
                orderHeader.ShippingDate = DateTime.Now;
            }
            else
            {
                orderHeader.OrderStatus = SD.OrderStatus.Cancelled;
                orderHeader.PaymentStatus = SD.OrderStatus.Cancelled;
            }

            _uow.Save();
            return RedirectToAction("Index");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [ActionName("Details")]
        public IActionResult Detail(string stripeToken)
        {
            OrderHeader orderHeader = _uow.OrderHeader.GetFirstOrDefault(a => a.Id == DetailsVM.OrderHeader.Id, includeProperties: "ApplicationUser");
            if (!string.IsNullOrWhiteSpace(stripeToken))
            {
                var option = new ChargeCreateOptions
                {
                    Amount = Convert.ToInt32(orderHeader.OrderTotal * 100),
                    Currency = "usd",
                    Description = "Order ID - " + orderHeader.Id,
                    Source = stripeToken
                };

                var service = new ChargeService();
                Charge charge = service.Create(option);
                if (charge.BalanceTransactionId == null)
                    orderHeader.PaymentStatus = SD.PaymentStatus.Rejected;
                else
                    orderHeader.TransactionId = charge.BalanceTransactionId;

                if (charge.Status.Equals("Succeeded", StringComparison.InvariantCultureIgnoreCase))
                {
                    orderHeader.PaymentStatus = SD.PaymentStatus.Approved;
                    orderHeader.PaymentDate = DateTime.Now;
                }

                _uow.Save();
            }

            return RedirectToAction("Details", "Order", new { id = orderHeader.Id });

        }
    }
}
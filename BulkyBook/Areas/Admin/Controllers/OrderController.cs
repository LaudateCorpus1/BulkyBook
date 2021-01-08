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
    }
}
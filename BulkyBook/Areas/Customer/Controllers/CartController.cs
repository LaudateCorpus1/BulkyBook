using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using AutoMapper;
using BulkyBook.DataAccess.Repository.IRepository;
using BulkyBook.Models;
using BulkyBook.Models.ViewModels;
using BulkyBook.Utility;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;
using Stripe;

namespace BulkyBook.Areas.Customer.Controllers
{
    [Area("Customer")]
    public class CartController : Controller
    {
        private readonly IUnitOfWork _uow;
        private readonly IEmailSender _emailSender;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly IMapper _mapper;

        [BindProperty]
        public ShoppingCartViewModel shoppingCartViewModel { get; set; }

        public CartController(IUnitOfWork uow,
            IEmailSender emailSender,
            UserManager<IdentityUser> userManager,
            IMapper mapper)
        {
            _uow = uow;
            _emailSender = emailSender;
            _userManager = userManager;
            _mapper = mapper;
        }
        public IActionResult Index()
        {
            var claimsIdentity = (ClaimsIdentity)User.Identity;
            var claim = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier);

            shoppingCartViewModel = new ShoppingCartViewModel()
            {
                OrderHeader = new Models.OrderHeader(),
                ShoppingCarts = _uow.ShoppingCart.GetAll(a => a.ApplicationUserId == claim.Value, includeProperties: "Product")
            };

            shoppingCartViewModel.OrderHeader.OrderTotal = 0;
            shoppingCartViewModel.OrderHeader.ApplicationUser = _uow.ApplicationUser.GetFirstOrDefault(a => a.Id == claim.Value, includeProperties: "Company");

            foreach (var cart in shoppingCartViewModel.ShoppingCarts)
            {
                cart.Price = SD.GetPriceBasedOnQuantity(cart.Count, cart.Product.Price, cart.Product.Price50, cart.Product.Price100);
                shoppingCartViewModel.OrderHeader.OrderTotal += cart.Price * cart.Count;
                cart.Product.Description = SD.ConvertToRawHtml(cart.Product.Description);
                if (cart.Product.Description.Length > 100) cart.Product.Description = cart.Product.Description.Substring(0, 99) + "...";
            }

            return View(shoppingCartViewModel);
        }

        [HttpPost]
        [ActionName("Index")]//// This is because the code written in cshtml page is for Submit inside a Form. 
        public async Task<ActionResult> IndexPost()
        {
            var claimsIdentity = (ClaimsIdentity)User.Identity;
            var claim = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier);
            var user = _uow.ApplicationUser.GetFirstOrDefault(a => a.Id == claim.Value);

            if (user == null)
            {
                ModelState.AddModelError(string.Empty, "Verification email is empty!");
            }

            var code = await _userManager.GenerateEmailConfirmationTokenAsync(user);
            code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));
            var callbackUrl = Url.Page(
                "/Account/ConfirmEmail",
                pageHandler: null,
                values: new { area = "Identity", userId = user.Id, code = code },
                protocol: Request.Scheme);

            await _emailSender.SendEmailAsync(user.Email, "Confirm your email",
                $"Please confirm your account by <a href='{HtmlEncoder.Default.Encode(callbackUrl)}'>clicking here</a>.");

            ModelState.AddModelError(string.Empty, "Verification email sent. Please check your email.");
            return RedirectToAction("Index");
        }

        public IActionResult Plus(int cartId)
        {
            var cart = _uow.ShoppingCart.GetFirstOrDefault(a => a.Id == cartId, includeProperties: "Product");

            cart.Count += 1;
            cart.Price = SD.GetPriceBasedOnQuantity(cart.Count, cart.Product.Price, cart.Product.Price50, cart.Product.Price100);
            _uow.Save();

            return RedirectToAction(nameof(Index));
        }

        public IActionResult Minus(int cartId)
        {
            var cart = _uow.ShoppingCart.GetFirstOrDefault(a => a.Id == cartId, includeProperties: "Product");

            if (cart.Count == 1)
            {
                var count = _uow.ShoppingCart.GetAll(a => a.ApplicationUserId == cart.ApplicationUserId).Count();
                _uow.ShoppingCart.Remove(cart);
                _uow.Save();

                HttpContext.Session.SetInt32(SD.Constants.ShoppingCartSession, count - 1);
            }
            else
            {
                cart.Count -= 1;
                cart.Price = SD.GetPriceBasedOnQuantity(cart.Count, cart.Product.Price, cart.Product.Price50, cart.Product.Price100);
                _uow.Save();
            }

            return RedirectToAction(nameof(Index));
        }

        public IActionResult Remove(int cartId)
        {
            var cart = _uow.ShoppingCart.GetFirstOrDefault(a => a.Id == cartId, includeProperties: "Product");

            var count = _uow.ShoppingCart.GetAll(a => a.ApplicationUserId == cart.ApplicationUserId).Count();
            _uow.ShoppingCart.Remove(cart);
            _uow.Save();

            HttpContext.Session.SetInt32(SD.Constants.ShoppingCartSession, count - 1);

            return RedirectToAction(nameof(Index));
        }

        public IActionResult Summary()
        {
            var claimsIdentity = (ClaimsIdentity)User.Identity;
            var claim = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier);

            shoppingCartViewModel = new ShoppingCartViewModel()
            {
                OrderHeader = new Models.OrderHeader(),
                ShoppingCarts = _uow.ShoppingCart.GetAll(a => a.ApplicationUserId == claim.Value, includeProperties: "Product")
            };
            var applicationUser = _uow.ApplicationUser.GetFirstOrDefault(a => a.Id == claim.Value, includeProperties: "Company");
            ////shoppingCartViewModel.OrderHeader = _mapper.Map<Models.OrderHeader>(applicationUser);
            shoppingCartViewModel.OrderHeader.ApplicationUser = applicationUser;

            foreach (var cart in shoppingCartViewModel.ShoppingCarts)
            {
                cart.Price = SD.GetPriceBasedOnQuantity(cart.Count, cart.Product.Price, cart.Product.Price50, cart.Product.Price100);
                shoppingCartViewModel.OrderHeader.OrderTotal += cart.Price * cart.Count;
            }

            shoppingCartViewModel.OrderHeader.Name = shoppingCartViewModel.OrderHeader.ApplicationUser.Name;
            shoppingCartViewModel.OrderHeader.PhoneNumber = shoppingCartViewModel.OrderHeader.ApplicationUser.PhoneNumber;
            shoppingCartViewModel.OrderHeader.StreetAddress = shoppingCartViewModel.OrderHeader.ApplicationUser.StreetAddress;
            shoppingCartViewModel.OrderHeader.City = shoppingCartViewModel.OrderHeader.ApplicationUser.City;
            shoppingCartViewModel.OrderHeader.State = shoppingCartViewModel.OrderHeader.ApplicationUser.State;
            shoppingCartViewModel.OrderHeader.PostalCode = shoppingCartViewModel.OrderHeader.ApplicationUser.PostalCode;

            return View(shoppingCartViewModel);
        }

        [HttpPost]
        [ActionName("Summary")]
        [ValidateAntiForgeryToken]
        public IActionResult SummaryPost(string stripeToken)
        {
            var claimsIdentity = (ClaimsIdentity)User.Identity;
            var claim = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier);

            var applicationUser = _uow.ApplicationUser.GetFirstOrDefault(a => a.Id == claim.Value, includeProperties: "Company");
            shoppingCartViewModel.OrderHeader.ApplicationUser = applicationUser;

            shoppingCartViewModel.ShoppingCarts = _uow.ShoppingCart.GetAll(a => a.ApplicationUserId == claim.Value, includeProperties: "Product");
            shoppingCartViewModel.OrderHeader.PaymentStatus = SD.PaymentStatus.Pending;
            shoppingCartViewModel.OrderHeader.OrderStatus = SD.OrderStatus.Pending;
            shoppingCartViewModel.OrderHeader.ApplicationUserId = claim.Value;
            shoppingCartViewModel.OrderHeader.OrderDate = DateTime.Now;

            _uow.OrderHeader.Add(shoppingCartViewModel.OrderHeader);
            _uow.Save();

            List<OrderDetails> orderDetails = new List<OrderDetails>();
            foreach (var cart in shoppingCartViewModel.ShoppingCarts)
            {
                cart.Price = SD.GetPriceBasedOnQuantity(cart.Count, cart.Product.Price, cart.Product.Price50, cart.Product.Price100);
                OrderDetails order = new OrderDetails()
                {
                    ProductId = cart.ProductId,
                    OrderId = shoppingCartViewModel.OrderHeader.Id,
                    Price = cart.Price,
                    Count = cart.Count
                };
                orderDetails.Add(order);
                shoppingCartViewModel.OrderHeader.OrderTotal += cart.Price * cart.Count;
            }

            _uow.OrderDetails.AddRange(orderDetails);
            _uow.ShoppingCart.RemoveRange(shoppingCartViewModel.ShoppingCarts);

            HttpContext.Session.SetInt32(SD.Constants.ShoppingCartSession, 0);

            if (string.IsNullOrWhiteSpace(stripeToken))
            {
                //order will be created for delayed payment for authorized company.
                shoppingCartViewModel.OrderHeader.PaymentDueDate = DateTime.Now.AddDays(30);
                shoppingCartViewModel.OrderHeader.OrderStatus = SD.OrderStatus.Approved;
                shoppingCartViewModel.OrderHeader.PaymentStatus = SD.PaymentStatus.ApprovedForDelayedPayment;
            }
            else
            {
                var option = new ChargeCreateOptions
                {
                    Amount = Convert.ToInt32(shoppingCartViewModel.OrderHeader.OrderTotal * 100),
                    Currency = "usd",
                    Description = "Order ID - " + shoppingCartViewModel.OrderHeader.Id,
                    Source = stripeToken
                };

                var service = new ChargeService();
                Charge charge = service.Create(option);
                if (charge.BalanceTransactionId == null)
                    shoppingCartViewModel.OrderHeader.PaymentStatus = SD.PaymentStatus.Rejected;
                else
                    shoppingCartViewModel.OrderHeader.TransactionId = charge.BalanceTransactionId;

                if (charge.Status.Equals("Succeeded", StringComparison.InvariantCultureIgnoreCase))
                {
                    shoppingCartViewModel.OrderHeader.PaymentStatus = SD.PaymentStatus.Approved;
                    shoppingCartViewModel.OrderHeader.OrderStatus = SD.OrderStatus.Approved;
                    shoppingCartViewModel.OrderHeader.PaymentDate = DateTime.Now;
                }
            }

            _uow.Save();

            return RedirectToAction("OrderConfirmation", "Cart", new { id = shoppingCartViewModel.OrderHeader.Id });
        }

        public IActionResult OrderConfirmation(int id)
        {
            return View(id);
        }
    }
}
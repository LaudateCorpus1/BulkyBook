using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using BulkyBook.Models.ViewModels;
using BulkyBook.DataAccess.Repository.IRepository;
using BulkyBook.Models;
using System.Collections.Generic;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

namespace BulkyBook.Areas.Customer.Controllers
{
    [Area("Customer")]
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IUnitOfWork _uow;

        public HomeController(ILogger<HomeController> logger, IUnitOfWork uow)
        {
            _logger = logger;
            _uow = uow;
        }

        public IActionResult Index()
        {
            IEnumerable<Product> productList = _uow.Product.GetAll(includeProperties: "Category,CoverType");
            return View(productList);
        }

        public IActionResult Privacy()
        {
            return View();
        }

        public IActionResult Details(int id)
        {
            var productFromDb = _uow.Product.GetFirstOrDefault(a => a.Id == id, includeProperties: "Category,CoverType");

            var shoppingCart = new ShoppingCart()
            {
                Product = productFromDb,
                ProductId = productFromDb.Id
            };

            return View(shoppingCart);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize]
        public IActionResult Details(ShoppingCart cart)
        {
            cart.Id = 0;
            if (ModelState.IsValid)
            {
                var claimsIdentity = (ClaimsIdentity)User.Identity;
                var claim = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier);
                cart.ApplicationUserId = claim.Value;

                ShoppingCart cartFromDb = _uow.ShoppingCart.GetFirstOrDefault(a => a.ApplicationUserId == cart.ApplicationUserId && a.ProductId == cart.ProductId, includeProperties: "Product");

                if (cartFromDb == null)
                    _uow.ShoppingCart.Add(cart);
                else
                {
                    cartFromDb.Count += cart.Count;
                    //_uow.ShoppingCart.Update(cartFromDb);
                    // This line can be removed as entity framework does the tracking and if any object is retrieved from db and then a property is updated and the Save() is called. it will automatically save the changes without .Update method being invoked.
                }
                _uow.Save();

                return RedirectToAction(nameof(Index));
            }
            else
            {
                var productFromDb = _uow.Product.GetFirstOrDefault(a => a.Id == cart.Id, includeProperties: "Category,CoverType");

                var shoppingCart = new ShoppingCart()
                {
                    Product = productFromDb,
                    ProductId = productFromDb.Id
                };

                return View(shoppingCart);
            }

        }


        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}

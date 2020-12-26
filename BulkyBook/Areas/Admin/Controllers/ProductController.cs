using BulkyBook.DataAccess.Repository.IRepository;
using BulkyBook.Models;
using BulkyBook.Models.ViewModels;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using System.Linq;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace BulkyBook.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class ProductController : Controller
    {
        private readonly IUnitOfWork _uow;
        private readonly IWebHostEnvironment _hostEnvironment; // to store our images

        public ProductController(IUnitOfWork uow, IWebHostEnvironment hostEnvironment)
        {
            _uow = uow;
            _hostEnvironment = hostEnvironment;
        }
        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Upsert(int? id)
        {
            ProductViewModel productVM = new ProductViewModel()
            {
                Product = new Product(),
                CategoryList = _uow.Category.GetAll()
                    .Select(a => new SelectListItem
                    {
                        Value = a.Id.ToString(),
                        Text = a.Name
                    }
                    ),
                CoverTypeList = _uow.CoverType.GetAll()
                    .Select(a => new SelectListItem
                    {
                        Value = a.Id.ToString(),
                        Text = a.Name
                    }
                    ),
            };
            if (id == null)
                return View(productVM);
            else
            {
                productVM.Product = _uow.Product.Get(id.GetValueOrDefault());
                if (productVM.Product == null)
                {
                    return NotFound();
                }
                return View(productVM);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Update(Product product)
        {
            if (ModelState.IsValid)
            {
                if (product.Id == 0)
                {
                    _uow.Product.Add(product);
                }
                else
                    _uow.Product.Update(product);

                _uow.Save();

                return RedirectToAction(nameof(Index));
            }

            return View(product);
        }

        [HttpDelete]
        public IActionResult Delete(int id)
        {
            var product = _uow.Product.Get(id);
            if (product != null)
            {
                _uow.Product.Remove(product);
                _uow.Save();
                return Json(new { success = true, message = "Successfully deleted" });
            }
            else
            {
                return Json(new { success = false, message = "Failed to delete" });
            }
        }

        [HttpGet]
        public IActionResult GetAll()
        {
            var result = _uow.Product.GetAll(includeProperties:"Category,CoverType");

            return Json(new { data = result });
        }
    }
}
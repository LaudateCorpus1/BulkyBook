using BulkyBook.DataAccess.Repository.IRepository;
using BulkyBook.Models;
using BulkyBook.Models.ViewModels;
using BulkyBook.Utility;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Linq;
using System.Threading.Tasks;

namespace BulkyBook.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = SD.Roles.Admin)]
    public class CategoryController : Controller
    {
        private readonly IUnitOfWork _uow;

        public CategoryController(IUnitOfWork uow)
        {
            _uow = uow;
        }
        public async Task<IActionResult> Index(int productPage = 1)
        {
            CategoryViewModel categoryVM = new CategoryViewModel()
            {
                Categories = await _uow.Category.GetAllAsync()
            };

            var count = categoryVM.Categories.Count();
            categoryVM.Categories = categoryVM.Categories.OrderBy(a => a.Name).Skip((productPage - 1) * 2).Take(2).ToList();

            categoryVM.PagingInfo = new PagingInfo
            {
                CurrentPage = productPage,
                ItemsPerPage = 2,
                TotalItems = count,
                urlParam = "/Admin/Category/Index?productPage=:"
            };

            return View(categoryVM);
        }

        public async Task<IActionResult> Upsert(int? id)
        {
            Category category = new Category();
            if (id == null)
                return View(category);
            else
            {
                category = await _uow.Category.GetAsync(id.GetValueOrDefault());
                if (category == null)
                {
                    return NotFound();
                }
                return View(category);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Update(Category category)
        {
            if (ModelState.IsValid)
            {
                if (category.Id == 0)
                {
                    await _uow.Category.AddAsync(category);
                }
                else
                    await _uow.Category.UpdateAsync(category);

                _uow.Save();

                return RedirectToAction(nameof(Index));
            }

            return View(category);
        }

        [HttpDelete]
        public async Task<IActionResult> Delete(int id)
        {
            var category = await _uow.Category.GetAsync(id);
            if (category != null)
            {
                await _uow.Category.RemoveAsync(category);
                _uow.Save();
                TempData["Success"] = "Category Successfully deleted";
                return Json(new { success = true, message = "Successfully deleted" });
            }
            else
            {
                TempData["Error"] = "Error Deleting category";
                return Json(new { success = false, message = "Failed to delete" });
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var result = await _uow.Category.GetAllAsync();

            return Json(new { data = result });
        }
    }
}
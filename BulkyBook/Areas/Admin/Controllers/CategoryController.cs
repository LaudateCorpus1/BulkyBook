using BulkyBook.DataAccess.Repository.IRepository;
using BulkyBook.Models;
using Microsoft.AspNetCore.Mvc;

namespace BulkyBook.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class CategoryController : Controller
    {
        private readonly IUnitOfWork _uow;

        public CategoryController(IUnitOfWork uow)
        {
            _uow = uow;
        }
        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Upsert(int? id)
        {
            Category category = new Category();
            if (id == null)
                return View(category);
            else
            {
                category = _uow.Category.Get(id.GetValueOrDefault());
                if (category == null)
                {
                    return NotFound();
                }
                return View(category);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Update(Category category)
        {
            if (ModelState.IsValid)
            {
                if (category.Id == 0)
                {
                    _uow.Category.Add(category);
                }
                else
                    _uow.Category.Update(category);

                _uow.Save();

                return RedirectToAction(nameof(Index));
            }

            return View(category);
        }

        [HttpDelete]
        public IActionResult Delete(int id)
        {
            var category = _uow.Category.Get(id);
            if (category != null)
            {
                _uow.Category.Remove(category);
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
            var result = _uow.Category.GetAll();

            return Json(new { data = result });
        }
    }
}
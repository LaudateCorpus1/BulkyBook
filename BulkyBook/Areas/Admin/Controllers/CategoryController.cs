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

        [HttpGet]
        public IActionResult GetAll()
        {
            var result = _uow.Category.GetAll();

            return Json(new { data = result });
        }
    }
}
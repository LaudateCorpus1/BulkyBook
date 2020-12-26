using BulkyBook.DataAccess.Repository.IRepository;
using BulkyBook.Models;
using Microsoft.AspNetCore.Mvc;

namespace BulkyBook.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class CoverTypeController : Controller
    {
        private readonly IUnitOfWork _uow;

        public CoverTypeController(IUnitOfWork uow)
        {
            _uow = uow;
        }
        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Upsert(int? id)
        {
            CoverType coverType = new CoverType();
            if (id == null)
                return View(coverType);
            else
            {
                coverType = _uow.CoverType.Get(id.GetValueOrDefault());
                if (coverType == null)
                {
                    return NotFound();
                }
                return View(coverType);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Update(CoverType coverType)
        {
            if (ModelState.IsValid)
            {
                if (coverType.Id == 0)
                {
                    _uow.CoverType.Add(coverType);
                }
                else
                    _uow.CoverType.Update(coverType);

                _uow.Save();

                return RedirectToAction(nameof(Index));
            }

            return View(coverType);
        }

        [HttpDelete]
        public IActionResult Delete(int id)
        {
            var coverType = _uow.CoverType.Get(id);
            if (coverType != null)
            {
                _uow.CoverType.Remove(coverType);
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
            var result = _uow.CoverType.GetAll();

            return Json(new { data = result });
        }
    }
}
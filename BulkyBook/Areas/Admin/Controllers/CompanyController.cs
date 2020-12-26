using BulkyBook.DataAccess.Repository.IRepository;
using BulkyBook.Models;
using BulkyBook.Utility;
using Dapper;
using Microsoft.AspNetCore.Mvc;

namespace BulkyBook.Areas.admin.Controllers
{
    [Area("Admin")]
    public class CompanyController : Controller
    {
        private readonly IUnitOfWork _uow;

        public CompanyController(IUnitOfWork uow)
        {
            _uow = uow;
        }
        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Upsert(int? id)
        {
            Company company = new Company();
            if (id == null)
                return View(company);
            else
            {
                company = _uow.Company.Get(id.GetValueOrDefault());
                if (company == null)
                {
                    return NotFound();
                }
                return View(company);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Update(Company company)
        {
            if (ModelState.IsValid)
            {
                if (company.Id == 0)
                {
                    _uow.Company.Add(company);
                }
                else
                    _uow.Company.Update(company);

                _uow.Save();

                return RedirectToAction(nameof(Index));
            }

            return View(company);
        }

        [HttpDelete]
        public IActionResult Delete(int id)
        {
            var company = _uow.Company.Get(id);
            if (company != null)
            {
                _uow.Company.Remove(company);
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
            var result = _uow.Company.GetAll();

            return Json(new { data = result });
        }

    }
}
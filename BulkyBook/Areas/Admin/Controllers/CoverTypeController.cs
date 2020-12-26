using BulkyBook.DataAccess.Repository.IRepository;
using BulkyBook.Models;
using BulkyBook.Utility;
using Dapper;
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

        ////public IActionResult Upsert(int? id)
        ////{
        ////    CoverType coverType = new CoverType();
        ////    if (id == null)
        ////        return View(coverType);
        ////    else
        ////    {
        ////        coverType = _uow.CoverType.Get(id.GetValueOrDefault());
        ////        if (coverType == null)
        ////        {
        ////            return NotFound();
        ////        }
        ////        return View(coverType);
        ////    }
        ////}

        ////[HttpPost]
        ////[ValidateAntiForgeryToken]
        ////public IActionResult Update(CoverType coverType)
        ////{
        ////    if (ModelState.IsValid)
        ////    {
        ////        if (coverType.Id == 0)
        ////        {
        ////            _uow.CoverType.Add(coverType);
        ////        }
        ////        else
        ////            _uow.CoverType.Update(coverType);

        ////        _uow.Save();

        ////        return RedirectToAction(nameof(Index));
        ////    }

        ////    return View(coverType);
        ////}

        ////[HttpDelete]
        ////public IActionResult Delete(int id)
        ////{
        ////    var coverType = _uow.CoverType.Get(id);
        ////    if (coverType != null)
        ////    {
        ////        _uow.CoverType.Remove(coverType);
        ////        _uow.Save();
        ////        return Json(new { success = true, message = "Successfully deleted" });
        ////    }
        ////    else
        ////    {
        ////        return Json(new { success = false, message = "Failed to delete" });
        ////    }
        ////}

        ////[HttpGet]
        ////public IActionResult GetAll()
        ////{
        ////    var result = _uow.CoverType.GetAll();

        ////    return Json(new { data = result });
        ////}

        [HttpGet]
        public IActionResult GetAll()
        {
            var result = _uow.StoredProcedureCall.List<CoverType>(SD.StoreProcedureName.Proc_CoverType_GetAll, null);

            return Json(new { data = result });
        }

        [HttpDelete]
        public IActionResult Delete(int id)
        {
            var parameter = new DynamicParameters();
            parameter.Add("@Id", id);

            var coverType = _uow.StoredProcedureCall.OneRecord<CoverType>(SD.StoreProcedureName.Proc_CoverType_Get, parameter);
            if (coverType != null)
            {
                _uow.StoredProcedureCall.Execute(SD.StoreProcedureName.Proc_CoverType_Delete, parameter);
                ////_uow.Save();
                return Json(new { success = true, message = "Successfully deleted" });
            }
            else
            {
                return Json(new { success = false, message = "Failed to delete" });
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Update(CoverType coverType)
        {
            var parameter = new DynamicParameters();
            parameter.Add("@Name", coverType.Name);
            if (ModelState.IsValid)
            {
                if (coverType.Id == 0)
                {
                    _uow.StoredProcedureCall.Execute(SD.StoreProcedureName.Proc_CoverType_Create, parameter);
                }
                else
                {
                    parameter.Add("@Id", coverType.Id);
                    _uow.StoredProcedureCall.Execute(SD.StoreProcedureName.Proc_CoverType_Update, parameter);
                }
                ////_uow.Save();

                return RedirectToAction(nameof(Index));
            }

            return View(coverType);
        }

        public IActionResult Upsert(int? id)
        {
            CoverType coverType = new CoverType();
            if (id == null)
                return View(coverType);
            else
            {
                var parameter = new DynamicParameters();
                parameter.Add("@Id", id);

                coverType = _uow.StoredProcedureCall.OneRecord<CoverType>(SD.StoreProcedureName.Proc_CoverType_Get, parameter);
                if (coverType == null)
                {
                    return NotFound();
                }
                return View(coverType);
            }
        }
    }
}
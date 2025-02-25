﻿using BulkyBook.DataAccess.Repository.IRepository;
using BulkyBook.Models;
using BulkyBook.Models.ViewModels;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using System.Linq;
using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.IO;
using Microsoft.AspNetCore.Authorization;
using BulkyBook.Utility;
using System.Threading.Tasks;

namespace BulkyBook.Areas.Admin.Controllers
{
    [Area("Admin")]

    [Authorize(Roles = SD.Roles.Admin)]
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

        public async Task<IActionResult> Upsert(int? id)
        {
            var categoryList = await _uow.Category.GetAllAsync();
            ProductViewModel productVM = new ProductViewModel()
            {
                Product = new Product(),
                CategoryList = categoryList
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
        public async Task<IActionResult> Update(ProductViewModel productVM)
        {
            if (ModelState.IsValid)
            {
                string webRootPath = _hostEnvironment.WebRootPath;
                var files = HttpContext.Request.Form.Files;
                if (files != null && files.Any())
                {
                    string filename = Guid.NewGuid().ToString();
                    var uploads = Path.Combine(webRootPath, @"images\products");
                    var extension = Path.GetExtension(files[0].FileName);

                    if (productVM.Product.ImageUrl != null)
                    {
                        var imagePath = Path.Combine(webRootPath, productVM.Product.ImageUrl.TrimStart('\\'));

                        if (System.IO.File.Exists(imagePath))
                            System.IO.File.Delete(imagePath);
                    }

                    using (var fileStream = new FileStream(Path.Combine(uploads, filename + extension), FileMode.Create))
                    {
                        files[0].CopyTo(fileStream);
                    }

                    productVM.Product.ImageUrl = @"\images\products\" + filename + extension;
                }
                else
                {
                    if (productVM.Product.Id != 0)
                    {
                        var product = _uow.Product.Get(productVM.Product.Id);
                        productVM.Product.ImageUrl = product.ImageUrl;
                    }
                }
                if (productVM.Product.Id == 0)
                    _uow.Product.Add(productVM.Product);
                else
                    _uow.Product.Update(productVM.Product);

                _uow.Save();

                return RedirectToAction(nameof(Index));
            }
            else
            {
                var categoryList = await _uow.Category.GetAllAsync();
                productVM.CategoryList = categoryList
                    .Select(a => new SelectListItem
                    {
                        Value = a.Id.ToString(),
                        Text = a.Name
                    });
                productVM.CoverTypeList = _uow.CoverType.GetAll()
                    .Select(a => new SelectListItem
                    {
                        Value = a.Id.ToString(),
                        Text = a.Name
                    });

                if (productVM.Product.Id != 0)
                {
                    var product = _uow.Product.Get(productVM.Product.Id);
                }
            }

            return View(productVM);
        }

        [HttpDelete]
        public IActionResult Delete(int id)
        {
            var product = _uow.Product.Get(id);
            if (product != null)
            {
                string webRootPath = _hostEnvironment.WebRootPath;
                var imagePath = Path.Combine(webRootPath, product.ImageUrl.TrimStart('\\'));

                if (System.IO.File.Exists(imagePath))
                    System.IO.File.Delete(imagePath);

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
            var result = _uow.Product.GetAll(includeProperties: "Category,CoverType");

            return Json(new { data = result });
        }
    }
}
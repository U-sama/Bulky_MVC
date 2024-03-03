﻿using BulkyBook.DataAccess.Repository.IRepository;
using BulkyBook.Models;
using BulkyBook.Models.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore.Metadata.Internal;

namespace BulkyBookWeb.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class ProductController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IWebHostEnvironment _webHostEnvironment; // For accessing the wwwroot file system
        public ProductController(IUnitOfWork unitOfWork, IWebHostEnvironment webHostEnvironment)
        {
            _unitOfWork = unitOfWork;
            _webHostEnvironment = webHostEnvironment;
        }

        public IActionResult Index()
        {
            List<Product> objProductList = _unitOfWork.Product.GetAll().ToList();
            return View(objProductList);
        }

        public IActionResult Upsert(int? Id)
        {
            ProductVM productVM = new ProductVM
            {
                CategoryList = _unitOfWork.Category.GetAll()
                                           .Select(c => new SelectListItem
                                           {
                                               Text = c.Name,
                                               Value = c.Id.ToString()
                                           })
            };

            if(Id == null || Id == 0)
            {
                productVM.Product = new Product();
                // Create
                return View(productVM);
            }
            else
            {
                //Update
                productVM.Product = _unitOfWork.Product.Get(p => p.Id == Id);
                if (productVM.Product == null)
                    return NotFound();
                return View(productVM);
            }
        }

        [HttpPost]
        public IActionResult Upsert(ProductVM productVM, IFormFile? file)
        {
            if(ModelState.IsValid)
            {
                string wwwRootPath = _webHostEnvironment.WebRootPath;
                if (file != null)
                {
                    string fileName = Guid.NewGuid().ToString() + Path.GetExtension(file.FileName);
                    string productPath = Path.Combine(wwwRootPath, @"images\product");
                    using (var filestream = new FileStream(Path.Combine(productPath, fileName), FileMode.Create))
                    {
                        file.CopyTo(filestream);
                    };
                    productVM.Product.ImageUrl = @"\images\product\" + fileName;
                }
                _unitOfWork.Product.Add(productVM.Product);
                _unitOfWork.Save();
                TempData["success"] = "Product created successfuly";
                return RedirectToAction("Index", "Product");
            }
            else
            {
                productVM.CategoryList = _unitOfWork.Category.GetAll()
                                                             .Select(c => new SelectListItem
                                                             {
                                                                Text = c.Name,
                                                                Value = c.Id.ToString()
                                                             });
                return View(productVM);
            }
            
        }


        public IActionResult Delete(int? Id)
        {
            if (Id == null || Id == 0)
                return NotFound();
            Product objProduct = _unitOfWork.Product.Get(p => p.Id == Id);
            if (objProduct == null)
                return NotFound();
            return View(objProduct);
        }

        [HttpPost, ActionName("Delete")]
        public IActionResult DeletePost(int? Id)
        {
            if (Id == null || Id == 0)
                return NotFound();
            Product? productFromDb = _unitOfWork.Product.Get(c => c.Id == Id);
            if (productFromDb == null)
                return NotFound();
            _unitOfWork.Product.Remove(productFromDb);
            _unitOfWork.Save();
            TempData["error"] = "Product Deleted Sucessfuly";
            return RedirectToAction("Index", "Product");
        }
    }
}

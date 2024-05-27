using BulkyBook.DataAccess.Repository.IRepository;
using BulkyBook.Models;
using BulkyBook.Models.ViewModels;
using BulkyBook.Utility;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore.Metadata.Internal;

namespace BulkyBookWeb.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = SD.Role_Admin)]
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
            List<Product> objProductList = _unitOfWork.Product.GetAll(IncludeProperties:"Category").ToList();
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
                productVM.Product = _unitOfWork.Product.Get(p => p.Id == Id, IncludeProperties:"ProductImages");
                if (productVM.Product == null)
                    return NotFound();
                return View(productVM);
            }
        }

        [HttpPost]
        public IActionResult Upsert(ProductVM productVM, List<IFormFile>? files)
        {
            if(ModelState.IsValid)
            {

                if (productVM.Product.Id == 0)
                {
                    //Create
                    _unitOfWork.Product.Add(productVM.Product);
                    //TempData["success"] = "Product created successfuly";
                }
                else
                {
                    //Update
                    _unitOfWork.Product.Update(productVM.Product);
                    //TempData["success"] = "Product updated successfuly";
                }
                _unitOfWork.Save();


                string wwwRootPath = _webHostEnvironment.WebRootPath;
                if (files != null)
                {

                    foreach (var file in files)
                    {
                        //if the user uploaded a new file
                        string fileName = Guid.NewGuid().ToString() + Path.GetExtension(file.FileName);
                        string productPath = @"images\products\product-" + productVM.Product.Id;
                        string finalPath = Path.Combine(wwwRootPath, productPath);

                        if(!Directory.Exists(finalPath))
                            Directory.CreateDirectory(finalPath);

                        using (var filestream = new FileStream(Path.Combine(finalPath, fileName), FileMode.Create))
                        {
                            file.CopyTo(filestream);
                        };

                        ProductImage productImage = new ProductImage()
                        {
                            ProductId = productVM.Product.Id,
                            ImageUrl = @"\" + productPath + @"\" + fileName
                        };

                        if(productVM.Product.ProductImages == null)
                            productVM.Product.ProductImages = new List<ProductImage>();

                        productVM.Product.ProductImages.Add(productImage);
                    }

                    _unitOfWork.Product.Update(productVM.Product);
                    _unitOfWork.Save();

                }

                TempData["success"] = "Product Created/Updated Successfuly";
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

        public IActionResult DeleteImage(int imageId)
        {
            var ImageTobeDeleted = _unitOfWork.ProductImage.Get(i => i.Id == imageId);
            int productId = ImageTobeDeleted.ProductId;
            if(ImageTobeDeleted != null)
            {
                if (!string.IsNullOrEmpty(ImageTobeDeleted.ImageUrl))
                {
                    //Delete Old image if exists
                    var oldImagePath = Path.Combine(_webHostEnvironment.WebRootPath, 
                                                    ImageTobeDeleted.ImageUrl.TrimStart('\\'));
                    if (System.IO.File.Exists(oldImagePath))
                        System.IO.File.Delete(oldImagePath);
                }
                _unitOfWork.ProductImage.Remove(ImageTobeDeleted);
                _unitOfWork.Save();

                TempData["success"] = "Deleted Successfuly";
            }
            return RedirectToAction(nameof(Upsert), new { Id = productId });
        }

        #region API Calls

        //[HttpGet]
        public IActionResult GetAll(int? Id)
        {
            List<Product> objProductList = _unitOfWork.Product.GetAll(IncludeProperties: "Category").ToList();
            return Json(new {data = objProductList});
        }

        [HttpDelete]
        public IActionResult Delete(int? Id)
        {
            if (!Id.HasValue)  return Json(new { success = false, message = "Error while deleting." });

            Product productToDelete =  _unitOfWork.Product.Get(p => p.Id == Id);
            if (productToDelete == null) return Json(new { success = false, message = "Error while deleting." });

            string productPath = @"images\products\product-" + Id;
            string finalPath = Path.Combine(_webHostEnvironment.WebRootPath, productPath);

            if (Directory.Exists(finalPath))
            {
                var filesPaths = Directory.GetFiles(finalPath);
                foreach (var filePath in filesPaths)
                {
                    System.IO.File.Delete(filePath);
                }
                Directory.Delete(finalPath);
            }

            _unitOfWork.Product.Remove(productToDelete);
            _unitOfWork.Save();

            return Json(new { success = true, message = "Deleted Successfuly." });
           
        }

        #endregion
    }
}

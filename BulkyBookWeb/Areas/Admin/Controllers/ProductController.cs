using BulkyBook.DataAccess.Repository.IRepository;
using BulkyBook.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore.Metadata.Internal;

namespace BulkyBookWeb.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class ProductController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
        public ProductController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public IActionResult Index()
        {
            List<Product> objProductList = _unitOfWork.Product.GetAll().ToList();
            return View(objProductList);
        }

        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Create(Product product)
        {
            if(ModelState.IsValid)
            {
                _unitOfWork.Product.Add(product);
                _unitOfWork.Save();
                TempData["success"] = "Product created successfuly";
                return RedirectToAction("Index", "Product");
            }
            return View();
        }

        public IActionResult Edit(int? Id)
        {
            if (Id == null || Id == 0)
                return NotFound();
            Product objProduct =  _unitOfWork.Product.Get(p => p.Id == Id);
            if(objProduct == null)
                return NotFound();
            return View(objProduct);
        }

        [HttpPost]
        public IActionResult Edit(Product product)
        {
            if (ModelState.IsValid)
            {
                _unitOfWork.Product.Update(product);
                _unitOfWork.Save();
                TempData["success"] = "Product Updated successfuly";
                return RedirectToAction("Index", "Product");
            }
            return View();
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

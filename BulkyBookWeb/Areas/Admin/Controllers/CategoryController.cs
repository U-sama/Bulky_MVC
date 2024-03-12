using BulkyBook.DataAccess.Data;
using BulkyBook.DataAccess.Repository.IRepository;
using BulkyBook.Models;
using BulkyBook.Utility;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Configuration;

namespace BulkyBookWeb.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = SD.Role_Admin)]
    public class CategoryController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
        public CategoryController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }
        public IActionResult Index()
        {
            List<Category> objCategoryList = _unitOfWork.Category.GetAll().ToList();
            return View(objCategoryList);
        }

        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Create(Category obj)
        {
            if (obj.Name == obj.DisplayOrder.ToString())
            {
                ModelState.AddModelError("Name", "Category Order can't be the same as Category Name.");
            }
            if (ModelState.IsValid)
            {
                _unitOfWork.Category.Add(obj);
                _unitOfWork.Save();
                TempData["success"] = "Category Created Sucessfuly";
                return RedirectToAction("Index", "Category");
            }
            return View();
        }

        public IActionResult Edit(int? Id)
        {
            if (Id == null || Id == 0)
                return NotFound();
            Category? categoryFromDb = _unitOfWork.Category.Get(c => c.Id == Id);
            //Category? categoryFromDb1 = _db.Categories.FirstOrDefault(c => c.Id == Id);
            //Category? categoryFromDb2 = _db.Categories.Where(c => c.Id == Id).FirstOrDefault();
            if (categoryFromDb == null)
                return NotFound();
            return View(categoryFromDb);
        }
        [HttpPost]
        public IActionResult Edit(Category obj)
        {
            if (ModelState.IsValid)
            {
                _unitOfWork.Category.Update(obj);
                _unitOfWork.Save();
                TempData["success"] = "Category Updated Sucessfuly";
                return RedirectToAction("Index", "Category");
            }
            return View();
        }

        public IActionResult Delete(int? Id)
        {
            if (Id == null || Id == 0)
                return NotFound();
            Category? categoryFromDb = _unitOfWork.Category.Get(c => c.Id == Id);
            //Category? categoryFromDb1 = _db.Categories.FirstOrDefault(c => c.Id == Id);
            //Category? categoryFromDb2 = _db.Categories.Where(c => c.Id == Id).FirstOrDefault();
            if (categoryFromDb == null)
                return NotFound();
            return View(categoryFromDb);
        }
        [HttpPost, ActionName("Delete")]
        public IActionResult DeletePost(int? Id)
        {
            if (Id == null || Id == 0)
                return NotFound();
            Category? categoryFromDb = _unitOfWork.Category.Get(c => c.Id == Id);
            if (categoryFromDb == null)
                return NotFound();
            _unitOfWork.Category.Remove(categoryFromDb);
            _unitOfWork.Save();
            TempData["success"] = "Category Deleted Sucessfuly";
            return RedirectToAction("Index", "Category");
        }

    }
}

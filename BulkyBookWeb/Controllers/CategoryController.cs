using BulkyBook.DataAccess.Data;
using BulkyBook.DataAccess.Repository.IRepository;
using BulkyBook.Models;
using Microsoft.AspNetCore.Mvc;

namespace BulkyBookWeb.Controllers
{
    public class CategoryController : Controller
    {
        private readonly ICategoryRepository _CategoryRepo;
        public CategoryController(ICategoryRepository CategoryRepo)
        {
            _CategoryRepo = CategoryRepo;
        }
        public IActionResult Index()
        {
            List<Category> objCategoryList = _CategoryRepo.GetAll().ToList();
            return View(objCategoryList);
        }

       

        public IActionResult Create()
        {
            return View();
        }
        [HttpPost]
        public IActionResult Create(Category obj)
        {
            if(obj.Name == obj.DisplayOrder.ToString())
            {
                ModelState.AddModelError("Name", "Category Order can't be the same as Category Name.");
            }
            if (ModelState.IsValid)
            {
                _CategoryRepo.Add(obj);
                _CategoryRepo.Save();
                TempData["success"] = "Category Created Sucessfuly";
                return RedirectToAction("Index", "Category");
            }
            return View();
        }

        public IActionResult Edit(int? Id)
        {
            if (Id == null || Id == 0)
                return NotFound();
            Category? categoryFromDb = _CategoryRepo.Get(c => c.Id == Id);
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
                _CategoryRepo.Update(obj);
                _CategoryRepo.Save();
                TempData["success"] = "Category Updated Sucessfuly";
                return RedirectToAction("Index", "Category");
            }
            return View();
        }

        public IActionResult Delete(int? Id)
        {
            if (Id == null || Id == 0)
                return NotFound();
            Category? categoryFromDb = _CategoryRepo.Get(c => c.Id == Id);
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
            Category? categoryFromDb = _CategoryRepo.Get(c => c.Id == Id);
            if (categoryFromDb == null)
                return NotFound();
            _CategoryRepo.Remove(categoryFromDb);
            _CategoryRepo.Save();
            TempData["success"] = "Category Deleted Sucessfuly";
            return RedirectToAction("Index", "Category");
        }

    }
}

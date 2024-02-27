using BulkyWebRazor_Temp.Data;
using BulkyWebRazor_Temp.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace BulkyWebRazor_Temp.Pages.Categories
{
    [BindProperties]
    public class DeleteModel : PageModel
    {
        private readonly ApplicationDbContext _db;
        public Category CategoriesFromDb { get; set; }
        public DeleteModel(ApplicationDbContext db)
        {
            _db = db;
        }
        public void OnGet(int? Id)
        {
            if (Id != null && Id != 0)
                CategoriesFromDb = _db.Categories.Find(Id);
        }
        public IActionResult OnPost()
        {

            CategoriesFromDb = _db.Categories.Find(CategoriesFromDb.Id);
            _db.Categories.Remove(CategoriesFromDb);
            _db.SaveChanges();
            TempData["success"] = "Category Deleted Sucessfuly";
            return RedirectToPage("Index");
        }
    }
}

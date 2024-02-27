using BulkyWebRazor_Temp.Data;
using BulkyWebRazor_Temp.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace BulkyWebRazor_Temp.Pages.Categories
{
    [BindProperties] // To bind the properties with data
    public class CreateModel : PageModel
    {
        private readonly ApplicationDbContext _db;
        public Category CategoriesFromDb { get; set; }
        public CreateModel(ApplicationDbContext db)
        {
            _db = db;
        }
        public void OnGet()
        {
        }
        public IActionResult OnPost()
        {
            _db.Categories.Add(CategoriesFromDb);
            _db.SaveChanges();
            TempData["success"] = "Category Created Sucessfuly";
            return RedirectToPage("Index");
        }
    }
}

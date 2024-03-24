using BulkyBook.DataAccess.Repository.IRepository;
using BulkyBook.Models;
using BulkyBook.Utility;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;

namespace BulkyBookWeb.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = SD.Role_Admin)]
    public class CompanyController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
        public CompanyController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }
        public IActionResult Index()
        {
            List<Company> objCompanyList = _unitOfWork.Company.GetAll().ToList();
            return View(objCompanyList);
        }
        public IActionResult Upsert(int? Id)
        {
            Company company;
            if(Id == null || Id == 0)
            {
                //Create
                company = new();
                return View(company);
            }
            else
            {
                // Update
                company = _unitOfWork.Company.Get(c => c.Id == Id);
                if(company == null)
                {
                    return NotFound();
                }
                return View(company);
            }
        }

        [HttpPost]
        public IActionResult Upsert(Company company)
        {
            if (ModelState.IsValid)
            {
                if(company.Id == 0)
                {
                    // Create
                    _unitOfWork.Company.Add(company);
                    TempData["success"] = "Company created successfuly";
                }
                else
                {
                    // Update
                    _unitOfWork.Company.Update(company);
                    TempData["success"] = "Company Updated successfuly";
                }
                _unitOfWork.Save();
                return RedirectToAction(nameof(Index));
            }
            else
            {
                return View(company);
            }
        }

        #region API Calls

        //[HttpGet]
        public IActionResult GetAll(int? Id)
        {
            List<Company> objCompanyList = _unitOfWork.Company.GetAll().ToList();
            return Json(new { data = objCompanyList });
        }

        [HttpDelete]
        public IActionResult Delete(int? Id)
        {
            if (!Id.HasValue) return Json(new { success = false, message = "Error while deleting." });

            Company companyToDelete = _unitOfWork.Company.Get(p => p.Id == Id);
            if (companyToDelete == null) return Json(new { success = false, message = "Error while deleting." });

           
            _unitOfWork.Company.Remove(companyToDelete);
            _unitOfWork.Save();

            return Json(new { success = true, message = "Deleted Successfuly." });

        }

        #endregion
    }
}

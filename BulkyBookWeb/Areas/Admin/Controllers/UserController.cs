using BulkyBook.DataAccess.Data;
using BulkyBook.DataAccess.Repository.IRepository;
using BulkyBook.Models;
using BulkyBook.Models.ViewModels;
using BulkyBook.Utility;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace BulkyBookWeb.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = SD.Role_Admin)]
    public class UserController : Controller
    {
        private readonly ApplicationDbContext _db;
        
        private readonly UserManager<IdentityUser> _userManager;

        public UserController(ApplicationDbContext db, UserManager<IdentityUser> userManager)
        {
            _db = db;
            _userManager = userManager;
        }
        public IActionResult Index()
        {
            return View();
        }

        public IActionResult RoleMangment(string userId)
        {
            string RoleID = _db.UserRoles.FirstOrDefault(r => r.UserId == userId).RoleId;

            RoleManagmentVM roleVM = new RoleManagmentVM()
            {
                applicationUser = _db.ApplicationUsers.FirstOrDefault(u => u.Id == userId),
                RoleList = _db.Roles.Select(r => new SelectListItem
                {
                    Text = r.Name,
                    Value = r.Name
                }),
                CompanyList = _db.Companies.Select(c => new SelectListItem
                {
                    Text = c.Name,
                    Value = c.Id.ToString()
                })
            };

            roleVM.applicationUser.Role = _db.Roles.FirstOrDefault(r => r.Id == RoleID).Name;

            return View(roleVM);
        }
        [HttpPost]
        public IActionResult RoleMangment(RoleManagmentVM roleManagmentVM)
        {
            string RoleID = _db.UserRoles.FirstOrDefault(r => r.UserId == roleManagmentVM.applicationUser.Id).RoleId;
            string OldRole = _db.Roles.FirstOrDefault(r => r.Id == RoleID).Name;

            if(!(roleManagmentVM.applicationUser.Role == OldRole))
            {
                // Role was Updated
                ApplicationUser applicationUser = _db.ApplicationUsers.FirstOrDefault(u => u.Id == roleManagmentVM.applicationUser.Id);
                if(roleManagmentVM.applicationUser.Role == SD.Role_Company)
                {
                    applicationUser.CompanyId = roleManagmentVM.applicationUser.CompanyId;
                }
                if(OldRole == SD.Role_Company)
                {
                    applicationUser.CompanyId = null;
                }
                _db.SaveChanges();

                _userManager.RemoveFromRoleAsync(applicationUser, OldRole).GetAwaiter().GetResult();
                _userManager.AddToRoleAsync(applicationUser, roleManagmentVM.applicationUser.Role).GetAwaiter().GetResult();
            }

            return RedirectToAction(nameof(Index));
        }



        #region API Calls

        //[HttpGet]
        public IActionResult GetAll(int? Id)
        {
            List<ApplicationUser> objUserList = _db.ApplicationUsers.Include(u => u.Company).ToList();

            var userRoles = _db.UserRoles.ToList();
            var roles = _db.Roles.ToList();

            foreach (var user in objUserList)
            {
                var roleId = userRoles.Where( r => r.UserId == user.Id ).FirstOrDefault().RoleId;
                user.Role = roles.FirstOrDefault(r => r.Id == roleId).Name;
                if(user.Company == null)
                {
                    user.Company = new Company { Name = "" };
                }
            }

            return Json(new { data = objUserList });
        }

        [HttpPost]
        public IActionResult LockUnlock([FromBody] string id)
        {
            var objFromDb = _db.ApplicationUsers.FirstOrDefault(u => u.Id == id);
            if (objFromDb == null)
            {
                return Json(new { success = false, message = "Error While Locking/Unlocking." });
            }

            string message = "";

            if(objFromDb.LockoutEnd != null && objFromDb.LockoutEnd > DateTime.Now)
            {
                // User is currently locked and we need to unlock them
                objFromDb.LockoutEnd = DateTime.Now;
                message = "User Locked Successfuly.";
            }
            else
            {
                objFromDb.LockoutEnd = DateTime.Now.AddYears(1000); // will be locked for the comming 1000 years :D
                message = "User Unlocked Successfuly.";
            }
            _db.SaveChanges();
            return Json(new { success = true, message = message });

        }

        #endregion
    }
}

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
        private readonly IUnitOfWork _unitOfWork;
        
        private readonly UserManager<IdentityUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;

        public UserController(IUnitOfWork unitOfWork, UserManager<IdentityUser> userManager, RoleManager<IdentityRole> roleManager)
        {
            _unitOfWork = unitOfWork;
            _userManager = userManager;
            _roleManager = roleManager;
        }
        public IActionResult Index()
        {
            return View();
        }

        public IActionResult RoleMangment(string userId)
        {
            //string RoleID = _db.UserRoles.FirstOrDefault(r => r.UserId == userId).RoleId;
            ApplicationUser userFromDb = _unitOfWork.ApplicationUser.Get(u => u.Id == userId, IncludeProperties:"Company");
            string RoleName = _userManager.GetRolesAsync(userFromDb).GetAwaiter().GetResult().FirstOrDefault();
            string RoleID = _roleManager.FindByNameAsync(RoleName).GetAwaiter().GetResult().Id;
            
            RoleManagmentVM roleVM = new RoleManagmentVM()
            {
                applicationUser = userFromDb,
                RoleList = _roleManager.Roles.Select(r => new SelectListItem
                {
                    Text = r.Name,
                    Value = r.Name
                }),
                CompanyList = _unitOfWork.Company.GetAll().Select(c => new SelectListItem
                {
                    Text = c.Name,
                    Value = c.Id.ToString()
                })
            };

            roleVM.applicationUser.Role = RoleName;

            return View(roleVM);
        }
        [HttpPost]
        public IActionResult RoleMangment(RoleManagmentVM roleManagmentVM)
        {
            ApplicationUser applicationUser = _unitOfWork.ApplicationUser.Get(u => u.Id == roleManagmentVM.applicationUser.Id);
            string OldRole = _userManager.GetRolesAsync(applicationUser).GetAwaiter().GetResult().FirstOrDefault();

            if (!(roleManagmentVM.applicationUser.Role == OldRole))
            {
                // Role was Updated
                //ApplicationUser applicationUser = _unitOfWork.ApplicationUser.Get(u => u.Id == roleManagmentVM.applicationUser.Id);
                if (roleManagmentVM.applicationUser.Role == SD.Role_Company)
                {
                    applicationUser.CompanyId = roleManagmentVM.applicationUser.CompanyId;
                }
                if (OldRole == SD.Role_Company)
                {
                    applicationUser.CompanyId = null;
                }
                _unitOfWork.ApplicationUser.Update(applicationUser);
                _unitOfWork.Save();

                _userManager.RemoveFromRoleAsync(applicationUser, OldRole).GetAwaiter().GetResult();
                _userManager.AddToRoleAsync(applicationUser, roleManagmentVM.applicationUser.Role).GetAwaiter().GetResult();
            }
            else
            {
                if(OldRole == SD.Role_Company && applicationUser.CompanyId != roleManagmentVM.applicationUser.CompanyId)
                {
                    applicationUser.CompanyId = roleManagmentVM.applicationUser.CompanyId;
                    _unitOfWork.ApplicationUser.Update(applicationUser);
                    _unitOfWork.Save();
                }
            }

            return RedirectToAction(nameof(Index));
        }



        #region API Calls

        //[HttpGet]
        public IActionResult GetAll(int? Id)
        {
            List<ApplicationUser> objUserList = _unitOfWork.ApplicationUser.GetAll(IncludeProperties:"Company").ToList();

            foreach (var user in objUserList)
            {
                user.Role = _userManager.GetRolesAsync(user).GetAwaiter().GetResult().FirstOrDefault();
                if (user.Company == null)
                {
                    user.Company = new Company { Name = "" };
                }
            }

            return Json(new { data = objUserList });
        }

        [HttpPost]
        public IActionResult LockUnlock([FromBody] string id)
        {
            var objFromDb = _unitOfWork.ApplicationUser.Get(u => u.Id == id);
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
            _unitOfWork.ApplicationUser.Update(objFromDb);
            _unitOfWork.Save();
            return Json(new { success = true, message = message });

        }

        #endregion
    }
}

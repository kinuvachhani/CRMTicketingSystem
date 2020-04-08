using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CRMTicketingSystem.DataAccess.Data;
using CRMTicketingSystem.DataAccess.Repository.IRepository;
using CRMTicketingSystem.Models;
using CRMTicketingSystem.Models.ViewModels;
using CRMTicketingSystem.Utility;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace CRMTicketingSystem.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = SD.Role_Admin + "," + SD.Role_Employee)]
    public class UserController : Controller
    {
        private readonly ApplicationDbContext _db;
        private readonly IUnitOfWork _unitofwork;
        RoleManager<IdentityRole> _roleManager;
        private readonly UserManager<IdentityUser> _userManager;

        public UserController(ApplicationDbContext db, IUnitOfWork unitOfwork, RoleManager<IdentityRole> roleManager, UserManager<IdentityUser> userManager)
        {
            _db = db;
            _unitofwork = unitOfwork;
            _roleManager = roleManager;
            _userManager = userManager;
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Upsert(string id)
        {
            ApplicationUser applicationuser = new ApplicationUser();
            //this is for edit
            applicationuser = _unitofwork.ApplicationUser.GetFirstOrDefault(i=>i.Id==id);
            if (applicationuser == null)
            {
                return NotFound();
            }
            return View(applicationuser);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Upsert(ApplicationUser applicationUser)
        {
            var user = _db.ApplicationUsers.FirstOrDefault(s => s.Id == applicationUser.Id);
            var oldrole = _db.UserRoles.FirstOrDefault(s => s.UserId == user.Id);
            var oldrolename = _db.Roles.FirstOrDefault(s => s.Id == oldrole.RoleId);
            var newrole = _db.Roles.FirstOrDefault(s => s.Name == applicationUser.Role);
            if (user != null)
            {
                if (!string.IsNullOrEmpty(oldrole.RoleId))
                {
                    _userManager.RemoveFromRoleAsync(user, oldrolename.Name).Wait();
                }
                _userManager.AddToRoleAsync(user, newrole.Name).Wait();

            }
            return RedirectToAction(nameof(Index));
        }
        
        #region API CALLS
        [HttpGet]
        public IActionResult GetAll()
        {
            var userList = _db.ApplicationUsers.Include(u=>u.Company).ToList();
            var userRole = _db.UserRoles.ToList();
            var roles = _db.Roles.ToList();
            foreach(var user in userList)
            {
                var roleId = userRole.FirstOrDefault(u => u.UserId == user.Id).RoleId;
                user.Role = roles.FirstOrDefault(u => u.Id == roleId).Name;
                if(user.Company==null)
                {
                    user.Company = new Company()
                    {
                        Name = "",
                    };
                }
            }
            return Json(new { data = userList });
        }

        [HttpPost]
        public IActionResult LockUnlock([FromBody] string id)
        {
            var objFromDb = _db.ApplicationUsers.FirstOrDefault(u => u.Id == id);
            var isadmin = _db.UserRoles.FirstOrDefault(u =>u.UserId == objFromDb.Id);
            var isrole = _db.Roles.FirstOrDefault(u => u.Id == isadmin.RoleId);
            if(isrole.Name == "Admin")
            {
                return Json(new { success = false, message = "Admin can't Lock" });
            }
            if (objFromDb == null)
            {
                return Json(new { success = false, message = "Error while Locking/Unlocking" });
            }
            if (objFromDb.LockoutEnd != null && objFromDb.LockoutEnd > DateTime.Now)
            {
                //user is currently locked, we will unlock them
                objFromDb.LockoutEnd = DateTime.Now;
            }
            else
            {
                objFromDb.LockoutEnd = DateTime.Now.AddYears(1000);
            }
            _db.SaveChanges();
            return Json(new { success = true, message = "Operation Successful." });
        }


        #endregion
    }
}
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using BulkyBook.DataAccess.Data;
using BulkyBook.Models;
using System;
using Microsoft.AspNetCore.Authorization;
using BulkyBook.Utility;

namespace BulkyBook.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = SD.Roles.Admin + "," + SD.Roles.Employee)]
    public class UserController : Controller
    {
        private readonly ApplicationDbContext _dbContext;

        public UserController(ApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }
        public IActionResult Index()
        {
            return View();
        }

        [HttpGet]
        public IActionResult GetAll()
        {
            var userList = _dbContext.ApplicationUsers.Include(a => a.Company).ToList();

            var userRoles = _dbContext.UserRoles.ToList();
            var roles = _dbContext.Roles.ToList();
            foreach (var user in userList)
            {
                if (userRoles.FirstOrDefault(a => a.UserId == user.Id) != null)
                {
                    var roleID = userRoles.FirstOrDefault(a => a.UserId == user.Id).RoleId;
                    user.Role = roles.FirstOrDefault(a => a.Id == roleID).Name;

                    if (user.Company == null)
                        user.Company = new Company() { Name = "" };
                }
            }

            return Json(new { data = userList });
        }

        [HttpPost]
        public IActionResult LockUnlockUser([FromBody] string id)
        {
            var obj = _dbContext.ApplicationUsers.FirstOrDefault(a => a.Id == id);
            if (obj == null)
            {
                return Json(new { success = false, message = "Error while locking/unlocking" });
            }
            if (obj.LockoutEnd != null && obj.LockoutEnd > DateTime.Now)
            {
                //user is locked
                obj.LockoutEnd = DateTime.Now;
            }
            else
            {
                obj.LockoutEnd = DateTime.Now.AddYears(1000);
            }

            _dbContext.SaveChanges();

            return Json(new { success = true, message = "Operation successful" });
        }

    }
}
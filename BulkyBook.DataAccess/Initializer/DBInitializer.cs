using BulkyBook.DataAccess.Data;
using BulkyBook.Models;
using BulkyBook.Utility;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BulkyBook.DataAccess.Initializer
{
    public class DBInitializer : IDBInitializer
    {
        private readonly ApplicationDbContext _dbContext;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;

        public DBInitializer(ApplicationDbContext dbContext, UserManager<IdentityUser> userManager, RoleManager<IdentityRole> roleManager)
        {
            _dbContext = dbContext;
            _userManager = userManager;
            _roleManager = roleManager;
        }
        public void Initialize()
        {
            try
            {
                if (_dbContext.Database.GetPendingMigrations().Any())
                {
                    _dbContext.Database.Migrate();
                }
            }
            catch (Exception ex)
            { }

            if (_dbContext.Roles.Any(a => a.Name == SD.Roles.Admin)) return;

            _roleManager.CreateAsync(new IdentityRole(SD.Roles.Admin)).GetAwaiter().GetResult();
            _roleManager.CreateAsync(new IdentityRole(SD.Roles.Employee)).GetAwaiter().GetResult();
            _roleManager.CreateAsync(new IdentityRole(SD.Roles.CompanyCustomer)).GetAwaiter().GetResult();
            _roleManager.CreateAsync(new IdentityRole(SD.Roles.IndividualCustomer)).GetAwaiter().GetResult();

            _userManager.CreateAsync(new ApplicationUser
            {
                UserName = "sonurcks92@gmail.com",
                Email = "sonurcks92@gmail.com",
                EmailConfirmed = true,
                Name = "Sonu Pattanaik"
            }, "Admin123*").GetAwaiter().GetResult();

            ApplicationUser user = _dbContext.ApplicationUsers.Where(a => a.Email == "sonurcks92@gmail.com").FirstOrDefault();

            _userManager.AddToRoleAsync(user, SD.Roles.Admin).GetAwaiter().GetResult();
        }
    }
}

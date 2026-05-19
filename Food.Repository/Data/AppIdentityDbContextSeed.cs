using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Food.Domain.Models.Identity;
using Microsoft.AspNetCore.Identity;

namespace Food.Repository.Data
{
    public static class AppIdentityDbContextSeed
    {
        public static async Task SeedUserAsync(UserManager<AppUser> userManager, RoleManager<IdentityRole> roleManager)
        {
            if (!await roleManager.RoleExistsAsync(UserRoles.Admin))
            {
                await roleManager.CreateAsync(new IdentityRole(UserRoles.Admin));
            }

            if (!await roleManager.RoleExistsAsync(UserRoles.Employee))
            {
                await roleManager.CreateAsync(new IdentityRole(UserRoles.Employee));
            }

            if (!userManager.Users.Any())
            {
                var Admin = new AppUser
                {
                    UserName = "Admin",
                    Email = "abdra1396@gmail.com",
                    PhoneNumber = "01015496488"
                };
                await userManager.CreateAsync(Admin, "Ar115599@");
                await userManager.AddToRoleAsync(Admin, UserRoles.Admin);

                var Employee = new AppUser
                {
                    UserName = "abdelrahman",
                    Email = "abdelrahmanmohamed2293@gmail.com",
                    PhoneNumber = "01015496488"
                };
                await userManager.CreateAsync(Employee, "Ar115599@");
                await userManager.AddToRoleAsync(Employee, UserRoles.Employee);
            }
        }
    }
}

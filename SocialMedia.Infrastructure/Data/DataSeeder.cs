using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using SocialMedia.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SocialMedia.Infrastructure.Data
{
    public static class DataSeeder
    {
        public static async Task SeedRolesAndAdminAsync(IServiceProvider serviceProvider)
        {
            var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();
            var userManager = serviceProvider.GetRequiredService<UserManager<ApplicationUser>>();

           
            string[] roles = { "Admin", "User" };

            foreach (var role in roles)
            {
                if (!await roleManager.RoleExistsAsync(role))
                {
                    await roleManager.CreateAsync(new IdentityRole(role));
                }
            }

         
            var adminEmail = "martinamamdouh97@gmail.com";
            var user = await userManager.FindByEmailAsync(adminEmail);

            if (user == null)
            {
                user = new ApplicationUser
                {
                    UserName = "admin",
                    Email = adminEmail,
                    EmailConfirmed = true
                };

                await userManager.CreateAsync(user, "Admin@123!");
            }

        
            if (!await userManager.IsInRoleAsync(user, "Admin"))
            {
                await userManager.AddToRoleAsync(user, "Admin");
            }
        }
    }
}

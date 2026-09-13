using CycleManager.Domain.Models;
using Microsoft.AspNetCore.Identity;

namespace WebCycleManager
{
    public static class IdentitySeeder
    {
        public static async Task SeedAsync(IServiceProvider services)
        {
            var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();
            var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();

            var configuration = services.GetRequiredService<IConfiguration>();

            var username = configuration["AdminUser:Username"];
            var password = configuration["AdminUser:Password"];

            if (string.IsNullOrWhiteSpace(username))
                throw new InvalidOperationException("AdminUser:Username ontbreekt in de configuratie.");

            if (string.IsNullOrWhiteSpace(password))
                throw new InvalidOperationException("AdminUser:Password ontbreekt in de configuratie.");

            const string roleName = "Admin";

            if(!await roleManager.RoleExistsAsync(roleName))
            {
                var roleResult = await roleManager.CreateAsync(new IdentityRole(roleName));
                if (!roleResult.Succeeded)
                {
                    throw new InvalidOperationException($"Aanmaken van Admin-rol mislukt: " +
                        string.Join(", ", roleResult.Errors.Select(e => e.Description)));
                }
            }

            var user = await userManager.FindByNameAsync(username);

            if(user == null)
            {
                user = new ApplicationUser
                {
                    UserName = username,
                    Email = username,
                    FirstName = "Admin",
                    LastName = "Beheerder",
                    EmailConfirmed = true
                };

                var userResult = await userManager.CreateAsync(user, password);
                if (!userResult.Succeeded)
                {
                    throw new InvalidOperationException($"Aanmaken van admin-gebruiker mislukt: " +
                        string.Join(", ", userResult.Errors.Select(e => e.Description)));
                }
            }

            if(!await userManager.IsInRoleAsync(user, roleName))
            {
                var roleResult = await userManager.AddToRoleAsync(user, roleName);
                
                if (!roleResult.Succeeded)
                {
                    throw new InvalidOperationException($"Toevoegen van Admin-rol mislukt: " +
                        string.Join(", ", roleResult.Errors.Select(e => e.Description)));
                }
            }
        }
    }
}

using Microsoft.AspNetCore.Identity;
using UserRole.Data;
using UserRole.Models;

namespace UserRole.Services
{
    public class SeedService
    {
        public static async Task SeedDatabase(IServiceProvider serviceProvider)
        {
            using var scope = serviceProvider.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
            var userManager = scope.ServiceProvider.GetRequiredService<UserManager<Users>>();
            var logger = scope.ServiceProvider.GetRequiredService<ILogger<SeedService>>();

            try
            {
                //ensure the database is ready
                logger.LogInformation("Ensuring the database is created.");
                await context.Database.EnsureCreatedAsync();

                //add roles
                logger.LogInformation("Seeding roles.");
                await AddRoleAsync(roleManager, "Admin");
                await AddRoleAsync(roleManager, "User");

                //Add admini user
                logger.LogInformation("Seeding admin user.");
                var adminEmail = "admin@gmail.com";
                if(await userManager.FindByEmailAsync(adminEmail) == null)
                {
                    var adminUser = new Users
                    {
                        FullName = "Rifatul",
                        UserName = adminEmail,
                        NormalizedUserName = adminEmail.ToUpper(),
                        Email = adminEmail,
                        NormalizedEmail = adminEmail.ToUpper(),
                        EmailConfirmed = true,
                        SecurityStamp = Guid.NewGuid().ToString()
                    };
                    var result = await userManager.CreateAsync(adminUser, "Admin@123");
                    if (result.Succeeded)
                    {
                        logger.LogInformation("Assigning admin role to the admin user.");
                        await userManager.AddToRoleAsync(adminUser,"Admin");
                    }
                    else
                    {
                        logger.LogError("Faild to create admin user: {Errors}",string.Join(", ",result.Errors.Select(e=>e.Description)));
                    }

                }
            }
            catch(Exception ex)
            {
                logger.LogError(ex, "An error occured while seeding the database.");
            }
        }
        private static async Task AddRoleAsync(RoleManager<IdentityRole> roleManager, string roleName)
        {
            if(!await roleManager.RoleExistsAsync(roleName))
            {
                var result = await roleManager.CreateAsync(new IdentityRole(roleName));
                if (!result.Succeeded)
                {
                    throw new Exception($"Faild to create role '{roleName}': {string.Join(". ",result.Errors.Select(e=>e.Description))}");
                }
            }
        }
    }
}

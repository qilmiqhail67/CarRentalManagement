using Microsoft.AspNetCore.Identity;

namespace CarRentalManagement.Data;

public static class IdentitySeeder
{
    public static async Task SeedAsync(IServiceProvider services, string adminPassword)
    {
        // Get the Identity helpers from Dependency Injection
        var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();
        var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();

        // STEP 1: Create the roles if they don't exist yet
        string[] roles = ["Administrator", "User"];
        foreach (var role in roles)
        {
            if (!await roleManager.RoleExistsAsync(role))
            {
                var result = await roleManager.CreateAsync(new IdentityRole(role));
                EnsureSuccess(result);
            }
        }

        // STEP 2: Create the admin account if it doesn't exist yet
        const string adminEmail = "admin@localhost.com";
        var admin = await userManager.FindByEmailAsync(adminEmail);
        if (admin == null)
        {
            admin = new ApplicationUser
            {
                UserName = adminEmail,
                Email = adminEmail,
                FirstName = "Admin",
                LastName = "User",
                EmailConfirmed = true
            };
            var result = await userManager.CreateAsync(admin, adminPassword);
            EnsureSuccess(result);
        }

        // STEP 3: Put the admin in the Administrator role
        if (!await userManager.IsInRoleAsync(admin, "Administrator"))
        {
            var result = await userManager.AddToRoleAsync(admin, "Administrator");
            EnsureSuccess(result);
        }
    }

    // Stop with a clear error message if any Identity step fails
    private static void EnsureSuccess(IdentityResult result)
    {
        if (!result.Succeeded)
        {
            throw new InvalidOperationException(
                string.Join("; ", result.Errors.Select(e => e.Description)));
        }
    }
}
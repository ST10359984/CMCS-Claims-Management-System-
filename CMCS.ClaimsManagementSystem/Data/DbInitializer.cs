using CMCS.ClaimsManagementSystem.Data;

using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;

public static class DbInitializer
{
    public static async Task InitializeRolesAsync(IServiceProvider serviceProvider)
    {
        var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();
        string[] roles = { "Lecturer", "Coordinator", "Academic Manager", "HR" };

        foreach (var role in roles)
        {
            if (!await roleManager.RoleExistsAsync(role))
                await roleManager.CreateAsync(new IdentityRole(role));
        }

        var userManager = serviceProvider.GetRequiredService<UserManager<IdentityUser>>();

        if (await userManager.FindByEmailAsync("lecturer@example.com") == null)
        {
            var user = new IdentityUser { UserName = "lecturer@example.com", Email = "lecturer@example.com", EmailConfirmed = true };
            await userManager.CreateAsync(user, "Password123!");
            await userManager.AddToRoleAsync(user, "Lecturer");
        }

        if (await userManager.FindByEmailAsync("coordinator@example.com") == null)
        {
            var user = new IdentityUser { UserName = "coordinator@example.com", Email = "coordinator@example.com", EmailConfirmed = true };
            await userManager.CreateAsync(user, "Password123!");
            await userManager.AddToRoleAsync(user, "Coordinator");
        }

        if (await userManager.FindByEmailAsync("manager@example.com") == null)
        {
            var user = new IdentityUser { UserName = "manager@example.com", Email = "manager@example.com", EmailConfirmed = true };
            await userManager.CreateAsync(user, "Password123!");
            await userManager.AddToRoleAsync(user, "Academic Manager");
        }

        if (await userManager.FindByEmailAsync("hr@example.com") == null)
        {
            var user = new IdentityUser { UserName = "hr@example.com", Email = "hr@example.com", EmailConfirmed = true };
            await userManager.CreateAsync(user, "Password123!");
            await userManager.AddToRoleAsync(user, "HR");
        }
    }
}

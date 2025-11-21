using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using CMCS__Claims_Management_System_.Data;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddIdentity<IdentityUser, IdentityRole>()
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddDefaultTokenProviders();

builder.Services.AddControllersWithViews();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();
    string[] roles = { "Lecturer", "Coordinator", "Academic Manager", "HR" };

    foreach (var role in roles)
    {
        if (!await roleManager.RoleExistsAsync(role))
            await roleManager.CreateAsync(new IdentityRole(role));
    }

    var userManager = services.GetRequiredService<UserManager<IdentityUser>>();

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

app.UseAuthentication();
app.UseAuthorization();
app.MapDefaultControllerRoute();
app.Run();

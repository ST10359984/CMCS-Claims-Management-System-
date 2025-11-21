namespace CMCS.ClaimsManagementSystem.Controllers;

using Microsoft.AspNetCore.Mvc;
using CMCS__Claims_Management_System_.Models;
using CMCS__Claims_Management_System_.Data;
using Microsoft.EntityFrameworkCore;
using System.Linq;

public class AccountController : Controller
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly RoleManager<IdentityRole> _roleManager;

    public AccountController(UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager)
    {
        _userManager = userManager;
        _roleManager = roleManager;
    }

    public async Task<IActionResult> AssignRoleToUser(string userId, string role)
    {
        var user = await _userManager.FindByIdAsync(userId);
        if (user != null)
        {
            await _userManager.AddToRoleAsync(user, role);
            TempData["SuccessMessage"] = $"{role} assigned to user successfully.";
        }
        return RedirectToAction("Index");
    }

    public async Task<IActionResult> CreateRoles()
    {
        string[] roleNames = { "Lecturer", "Coordinator", "Academic Manager", "HR" };
        foreach (var roleName in roleNames)
        {
            var roleExist = await _roleManager.RoleExistsAsync(roleName);
            if (!roleExist)
            {
                await _roleManager.CreateAsync(new IdentityRole(roleName));
            }
        }
        TempData["SuccessMessage"] = "Roles created successfully.";
        return RedirectToAction("Index");
    }
}

namespace CMCS.ClaimsManagementSystem.Controllers;

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using CMCS__Claims_Management_System_.Data;
using CMCS__Claims_Management_System_.Models;
using System.Threading.Tasks;
using System.Linq;
using System.IO;
using OfficeOpenXml;

[Authorize(Roles = "HR")]
public class HRController : Controller
{
    private readonly ApplicationDbContext _context;

    public HRController(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IActionResult> Lecturers()
    {
        var lecturers = await _context.Users
            .Where(u => u.Roles.Any(r => r.Role.Name == "Lecturer"))
            .ToListAsync();
        return View(lecturers);
    }

    public async Task<IActionResult> EditLecturer(string id)
    {
        var lecturer = await _context.Users.FindAsync(id);
        if (lecturer == null) return NotFound();
        return View(lecturer);
    }

    [HttpPost]
    public async Task<IActionResult> EditLecturer(User lecturer)
    {
        if (!ModelState.IsValid) return View(lecturer);
        _context.Update(lecturer);
        await _context.SaveChangesAsync();
        TempData["SuccessMessage"] = "Lecturer details updated.";
        return RedirectToAction(nameof(Lecturers));
    }

    public async Task<IActionResult> GenerateReport()
    {
        var approvedClaims = await _context.Claims
            .Where(c => c.Status == "Approved")
            .Include(c => c.ClaimDetails)
            .Include(c => c.Approvals)
            .Include(c => c.User)
            .ToListAsync();

        using var package = new ExcelPackage();
        var ws = package.Workbook.Worksheets.Add("Approved Claims");

        ws.Cells[1, 1].Value = "Claim ID";
        ws.Cells[1, 2].Value = "Lecturer";
        ws.Cells[1, 3].Value = "Month";
        ws.Cells[1, 4].Value = "Total Hours";
        ws.Cells[1, 5].Value = "Total Amount";
        ws.Cells[1, 6].Value = "Status";

        int row = 2;
        foreach (var claim in approvedClaims)
        {
            ws.Cells[row, 1].Value = claim.ClaimID;
            ws.Cells[row, 2].Value = claim.User.UserName;
            ws.Cells[row, 3].Value = claim.Month;
            ws.Cells[row, 4].Value = claim.TotalHours;
            ws.Cells[row, 5].Value = claim.TotalAmount;
            ws.Cells[row, 6].Value = claim.Status;
            row++;
        }

        var stream = new MemoryStream();
        package.SaveAs(stream);
        stream.Position = 0;
        string excelName = $"ApprovedClaims-{DateTime.Now:yyyyMMddHHmmss}.xlsx";
        return File(stream, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", excelName);
    }
}

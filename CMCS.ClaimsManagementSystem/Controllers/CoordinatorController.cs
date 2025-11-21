namespace CMCS.ClaimsManagementSystem.Controllers;

using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;

[Authorize(Roles = "Coordinator")]
public class CoordinatorController : Controller
{
    private readonly ApplicationDbContext _context;

    public CoordinatorController(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IActionResult> Dashboard()
    {
        var claims = await _context.Claims
            .Include(c => c.Approvals)
            .Include(c => c.ClaimDetails)
            .Include(c => c.Documents)
            .Where(c => c.Status == "Submitted")
            .ToListAsync();

        var model = claims.Select(c => new ClaimReviewViewModel
        {
            Claim = c,
            LecturerName = _context.Users.FirstOrDefault(u => u.UserID == c.UserID)?.Name ?? "Unknown"
        });

        return View(model);
    }

    public async Task<IActionResult> Review(int id)
    {
        var claim = await _context.Claims
            .Include(c => c.ClaimDetails)
            .Include(c => c.Documents)
            .Include(c => c.Approvals)
            .FirstOrDefaultAsync(c => c.ClaimID == id);

        if (claim == null) return NotFound();

        ViewData["LecturerName"] = _context.Users.FirstOrDefault(u => u.UserID == claim.UserID)?.Name ?? "Unknown";
        ViewData["CurrentRole"] = "Coordinator";

        return View(claim);
    }

    [HttpPost]
    public async Task<IActionResult> Decision(int claimId, string decision, string comments)
    {
        var claim = await _context.Claims.Include(c => c.Approvals).FirstOrDefaultAsync(c => c.ClaimID == claimId);
        if (claim == null) return NotFound();

        var approval = new Approval
        {
            ClaimID = claim.ClaimID,
            ApproverID = User.Identity.Name,
            Role = "Coordinator",
            Decision = decision,
            Date = DateTime.Now,
            Comments = comments
        };

        claim.Status = decision == "Approve" ? "Verified" : "Rejected";

        _context.Approvals.Add(approval);
        await _context.SaveChangesAsync();

        TempData["SuccessMessage"] = $"Claim {claim.ClaimID} {decision}d successfully.";
        return RedirectToAction("Dashboard");
    }
}

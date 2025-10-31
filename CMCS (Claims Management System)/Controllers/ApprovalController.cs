using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using System.Security.Claims;

[Authorize(Roles = "Programme Coordinator,Academic Manager")]
public class ApprovalController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly UserManager<IdentityUser> _userManager;

    public ApprovalController(ApplicationDbContext context, UserManager<IdentityUser> userManager)
    {
        _context = context;
        _userManager = userManager;
    }

    public async Task<IActionResult> Dashboard()
    {
        var currentUserRole = User.IsInRole("Programme Coordinator") ? "Programme Coordinator" : "Academic Manager";

        IQueryable<Claim> pendingClaims = _context.Claims
            .Include(c => c.ClaimDetails)
            .Include(c => c.Documents);

        if (currentUserRole == "Programme Coordinator")
        {
            pendingClaims = pendingClaims.Where(c => c.Status == "Pending");
        }
        else if (currentUserRole == "Academic Manager")
        {
            pendingClaims = pendingClaims.Where(c => c.Status == "Coordinator Approved");
        }
        else
        {
            return Forbid();
        }

        var claimsList = await pendingClaims.ToListAsync();

        var reviewList = new List<ClaimReviewViewModel>();
        foreach (var claim in claimsList)
        {
            var user = await _userManager.FindByIdAsync(claim.UserID);
            reviewList.Add(new ClaimReviewViewModel
            {
                Claim = claim,
                LecturerName = user?.UserName ?? "Unknown Lecturer"
            });
        }

        ViewData["CurrentRole"] = currentUserRole;
        return View(reviewList);
    }

    public async Task<IActionResult> Review(int id)
    {
        var claim = await _context.Claims
            .Include(c => c.ClaimDetails)
            .Include(c => c.Documents)
            .Include(c => c.Approvals)
            .FirstOrDefaultAsync(m => m.ClaimID == id);

        if (claim == null)
        {
            return NotFound();
        }

        var user = await _userManager.FindByIdAsync(claim.UserID);
        ViewData["LecturerName"] = user?.UserName ?? "Unknown Lecturer";
        ViewData["CurrentRole"] = User.IsInRole("Programme Coordinator") ? "Programme Coordinator" : "Academic Manager";

        return View(claim);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Decision(int claimId, string decision, string comments)
    {
        var claim = await _context.Claims.FirstOrDefaultAsync(c => c.ClaimID == claimId);

        if (claim == null || claim.Status.EndsWith("Approved") || claim.Status == "Rejected")
        {
            TempData["ErrorMessage"] = "Claim not found or decision has already been finalized.";
            return RedirectToAction(nameof(Dashboard));
        }

        var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var currentUserRole = User.IsInRole("Programme Coordinator") ? "Programme Coordinator" : "Academic Manager";

        if (currentUserRole == "Programme Coordinator" && claim.Status != "Pending")
        {
            TempData["ErrorMessage"] = "This claim cannot be reviewed by the Coordinator at this stage.";
            return RedirectToAction(nameof(Dashboard));
        }
        if (currentUserRole == "Academic Manager" && claim.Status != "Coordinator Approved")
        {
            TempData["ErrorMessage"] = "This claim is not ready for Academic Manager review.";
            return RedirectToAction(nameof(Dashboard));
        }

        try
        {
            if (decision == "Approve")
            {
                claim.Status = currentUserRole == "Programme Coordinator" ? "Coordinator Approved" : "Approved";
            }
            else if (decision == "Reject")
            {
                claim.Status = "Rejected";
            }

            var approval = new Approval
            {
                ClaimID = claimId,
                ApproverID = currentUserId,
                Role = currentUserRole,
                Decision = decision,
                Date = DateTime.Now,
                Comments = comments
            };
            _context.Approvals.Add(approval);

            await _context.SaveChangesAsync();
            TempData["SuccessMessage"] = $"Claim {claimId} was successfully {decision}d.";
        }
        catch (DbUpdateException)
        {
            TempData["ErrorMessage"] = $"A database error prevented the decision on Claim {claimId}. Please try again.";
        }
        catch (Exception)
        {
            TempData["ErrorMessage"] = $"A system error occurred while making the decision on Claim {claimId}.";
        }

        return RedirectToAction(nameof(Dashboard));
    }
}

public class ClaimReviewViewModel
{
    public Claim Claim { get; set; }
    public string LecturerName { get; set; } = string.Empty;
}
namespace CMCS.ClaimsManagementSystem.Controllers;

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using CMCS__Claims_Management_System_.Models;
using CMCS__Claims_Management_System_.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Text;

[Authorize]
public class ClaimsController : Controller
{
    private readonly ApplicationDbContext _context;

    public ClaimsController(ApplicationDbContext context)
    {
        _context = context;
    }

    private bool ValidateClaim(ClaimSubmissionViewModel model)
    {
        if (model.HourlyRate <= 0 || model.HourlyRate > 500) return false;

        decimal totalHours = model.Details.Sum(d => d.HoursWorked);
        if (totalHours > 160) return false;

        if (model.Details.Any(d => d.HoursWorked < 0.5m || d.HoursWorked > 24)) return false;

        decimal totalAmount = totalHours * model.HourlyRate;
        if (totalAmount > 10000) return false;

        return true;
    }

    [HttpPost]
    public async Task<IActionResult> SubmitClaim(ClaimSubmissionViewModel model)
    {
        if (!ValidateClaim(model)) return View(model);

        var claim = new Claim
        {
            UserID = User.Identity.Name,
            Month = model.Month,
            HourlyRate = model.HourlyRate,
            TotalHours = model.Details.Sum(d => d.HoursWorked),
            TotalAmount = model.Details.Sum(d => d.HoursWorked * model.HourlyRate),
            Status = "Submitted",
            ClaimDetails = model.Details.Select(d => new ClaimDetail
            {
                Date = d.Date,
                HoursWorked = d.HoursWorked,
                Description = d.Description
            }).ToList()
        };

        _context.Claims.Add(claim);
        await _context.SaveChangesAsync();

        if (ValidateClaim(model))
        {
            claim.Status = "Verified";
            await _context.SaveChangesAsync();
            await AutoApproveClaim(claim.ClaimID);
        }
        else
        {
            claim.Status = "Rejected";
            await _context.SaveChangesAsync();
        }

        TempData["SuccessMessage"] = $"Claim submitted and {claim.Status.ToLower()}!";
        return RedirectToAction("Index");
    }

    private async Task AutoApproveClaim(int claimId)
    {
        var claim = await _context.Claims.Include(c => c.ClaimDetails)
                                         .Include(c => c.Approvals)
                                         .FirstOrDefaultAsync(c => c.ClaimID == claimId);
        if (claim == null) return;

        if (claim.Status == "Verified")
        {
            claim.Status = "Approved";
            claim.Approvals ??= new List<Approval>();
            claim.Approvals.Add(new Approval
            {
                ClaimID = claimId,
                ApproverID = "System",
                Role = "Auto",
                Decision = "Approved",
                Comments = "Auto-approved based on valid claim",
                Date = System.DateTime.Now
            });
            await _context.SaveChangesAsync();
            await AutoPayClaim(claimId);
        }
    }

    private async Task AutoPayClaim(int claimId)
    {
        var claim = await _context.Claims.FindAsync(claimId);
        if (claim == null) return;

        if (claim.Status == "Approved")
        {
            claim.Status = "Paid";
            await _context.SaveChangesAsync();
        }
    }

    [Authorize(Roles = "Coordinator")]
    public async Task<IActionResult> VerifyClaim(int claimId)
    {
        var claim = await _context.Claims.FindAsync(claimId);
        if (claim == null) return NotFound();

        claim.Status = "Verified";
        await _context.SaveChangesAsync();
        await AutoApproveClaim(claimId);

        TempData["SuccessMessage"] = "Claim verified successfully.";
        return RedirectToAction("Index");
    }

    [Authorize(Roles = "Academic Manager")]
    [HttpPost]
    public async Task<IActionResult> ApproveRejectClaim(int claimId, bool approve, string comments)
    {
        var claim = await _context.Claims.Include(c => c.Approvals).FirstOrDefaultAsync(c => c.ClaimID == claimId);
        if (claim == null) return NotFound();

        string status = approve ? "Approved" : "Rejected";
        claim.Status = status;

        claim.Approvals ??= new List<Approval>();
        claim.Approvals.Add(new Approval
        {
            ClaimID = claimId,
            ApproverID = User.Identity.Name,
            Role = "Academic Manager",
            Decision = status,
            Comments = comments,
            Date = System.DateTime.Now
        });

        await _context.SaveChangesAsync();
        if (approve) await AutoPayClaim(claimId);

        TempData["SuccessMessage"] = $"Claim {status} successfully.";
        return RedirectToAction("Index");
    }

    [Authorize(Roles = "HR")]
    public async Task<IActionResult> PayClaim(int claimId)
    {
        await MoveClaimStatusAsync(claimId, "Paid");
        TempData["SuccessMessage"] = "Claim marked as paid.";
        return RedirectToAction("Index");
    }

    private async Task MoveClaimStatusAsync(int claimId, string newStatus)
    {
        var claim = await _context.Claims.FindAsync(claimId);
        if (claim == null) return;

        claim.Status = newStatus;
        _context.Claims.Update(claim);
        await _context.SaveChangesAsync();
    }

    [Authorize(Roles = "HR")]
    public async Task<IActionResult> GenerateReport()
    {
        var approvedClaims = await _context.Claims
            .Where(c => c.Status == "Approved")
            .Include(c => c.ClaimDetails)
            .Include(c => c.Approvals)
            .ToListAsync();

        return View(approvedClaims);
    }

    [Authorize(Roles = "HR")]
    [HttpPost]
    public async Task<IActionResult> DownloadReport()
    {
        var approvedClaims = await _context.Claims
            .Where(c => c.Status == "Approved")
            .Include(c => c.ClaimDetails)
            .Include(c => c.Approvals)
            .ToListAsync();

        var csv = new StringBuilder();
        csv.AppendLine("ClaimID,Lecturer,Month,TotalHours,TotalAmount,ApprovalHistory");

        foreach (var claim in approvedClaims)
        {
            var approvals = claim.Approvals != null
                ? string.Join(" | ", claim.Approvals.OrderBy(a => a.Date)
                                                    .Select(a => $"{a.Role}:{a.Decision}"))
                : "None";

            csv.AppendLine($"{claim.ClaimID},{claim.UserID},{claim.Month},{claim.TotalHours},{claim.TotalAmount},{approvals}");
        }

        byte[] buffer = Encoding.UTF8.GetBytes(csv.ToString());
        return File(buffer, "text/csv", "ApprovedClaimsReport.csv");
    }
}

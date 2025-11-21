using CMCS.ClaimsManagementSystem.Models;

using Microsoft.EntityFrameworkCore;

public class ClaimService
{
    private readonly ApplicationDbContext _context;

    public ClaimService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task VerifyAndProcessClaim(int claimId)
    {
        var claim = await _context.Claims
            .Include(c => c.ClaimDetails)
            .Include(c => c.Approvals)
            .FirstOrDefaultAsync(c => c.ClaimID == claimId);

        if (claim == null) return;

        bool isValid = true;

        foreach (var detail in claim.ClaimDetails!)
        {
            if (detail.HoursWorked <= 0 || detail.HoursWorked > 24)
                isValid = false;

            if (detail.Date > DateTime.Now)
                isValid = false;
        }

        if (claim.HourlyRate <= 0 || claim.TotalHours != claim.ClaimDetails.Sum(d => d.HoursWorked))
            isValid = false;

        decimal calculatedAmount = claim.ClaimDetails.Sum(d => d.HoursWorked) * claim.HourlyRate;
        if (calculatedAmount != claim.TotalAmount)
            isValid = false;

        if (!isValid)
        {
            claim.Status = "Rejected";
            claim.Approvals ??= new List<Approval>();
            claim.Approvals.Add(new Approval
            {
                ClaimID = claim.ClaimID,
                ApproverID = "System",
                Role = "System",
                Decision = "Reject",
                Date = DateTime.Now,
                Comments = "Claim failed automated verification."
            });
        }
        else
        {
            claim.Status = "Verified";
        }

        await _context.SaveChangesAsync();
    }

    public async Task AutoProcessAllSubmittedClaims()
    {
        var submittedClaims = await _context.Claims
            .Where(c => c.Status == "Submitted")
            .ToListAsync();

        foreach (var claim in submittedClaims)
        {
            await VerifyAndProcessClaim(claim.ClaimID);
        }
    }
}

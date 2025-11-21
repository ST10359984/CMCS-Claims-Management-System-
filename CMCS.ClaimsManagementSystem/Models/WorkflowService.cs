using CMCS.ClaimsManagementSystem.Models;

using Microsoft.EntityFrameworkCore;

public class WorkflowService
{
    private readonly ApplicationDbContext _context;

    public WorkflowService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task MoveClaimToNextStatus(int claimId)
    {
        var claim = await _context.Claims
            .Include(c => c.Approvals)
            .FirstOrDefaultAsync(c => c.ClaimID == claimId);

        if (claim == null) return;

        switch (claim.Status)
        {
            case "Submitted":
                claim.Status = "Verified";
                break;
            case "Verified":
                claim.Status = "Approved";
                break;
            case "Approved":
                claim.Status = "Paid";
                break;
            case "Rejected":
                break;
            default:
                claim.Status = "Submitted";
                break;
        }

        await _context.SaveChangesAsync();
    }

    public async Task<List<Claim>> GetClaimsForRole(string role)
    {
        switch (role)
        {
            case "Coordinator":
                return await _context.Claims
                    .Where(c => c.Status == "Verified")
                    .Include(c => c.Approvals)
                    .ToListAsync();
            case "Academic Manager":
                return await _context.Claims
                    .Where(c => c.Status == "Approved")
                    .Include(c => c.Approvals)
                    .ToListAsync();
            default:
                return new List<Claim>();
        }
    }

    public async Task AutoProcessAllClaims()
    {
        var claims = await _context.Claims.ToListAsync();
        foreach (var claim in claims)
        {
            if (claim.Status == "Verified")
                await MoveClaimToNextStatus(claim.ClaimID);
        }
    }
}

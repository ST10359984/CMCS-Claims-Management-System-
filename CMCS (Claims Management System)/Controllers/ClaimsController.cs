using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using System.IO;

[Authorize(Roles = "Lecturer")]
public class ClaimsController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly IWebHostEnvironment _hostingEnvironment;

    public ClaimsController(ApplicationDbContext context, IWebHostEnvironment hostingEnvironment)
    {
        _context = context;
        _hostingEnvironment = hostingEnvironment;
    }

    public IActionResult Submit()
    {
        var viewModel = new ClaimSubmissionViewModel();
        viewModel.Details.Add(new ClaimDetailViewModel());
        return View(viewModel);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Submit(ClaimSubmissionViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        try
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var totalHours = model.Details.Sum(d => d.HoursWorked);
            var totalAmount = totalHours * model.HourlyRate;

            var claim = new Claim
            {
                UserID = userId,
                Month = model.Month,
                TotalHours = totalHours,
                HourlyRate = model.HourlyRate,
                TotalAmount = totalAmount,
                Status = "Pending"
            };
            _context.Claims.Add(claim);
            await _context.SaveChangesAsync();

            foreach (var detail in model.Details)
            {
                var claimDetail = new ClaimDetail
                {
                    ClaimID = claim.ClaimID,
                    Date = detail.Date,
                    HoursWorked = detail.HoursWorked,
                    Description = detail.Description
                };
                _context.ClaimDetails.Add(claimDetail);
            }

            if (model.SupportingDocuments != null && model.SupportingDocuments.Count > 0)
            {
                var uploadsFolder = Path.Combine(_hostingEnvironment.WebRootPath, "documents", userId);
                Directory.CreateDirectory(uploadsFolder);

                foreach (var file in model.SupportingDocuments)
                {
                    var fileExtension = Path.GetExtension(file.FileName).ToLowerInvariant();
                    var allowedExtensions = new[] { ".pdf", ".jpg", ".jpeg", ".png", ".docx", ".xlsx" };

                    if (!allowedExtensions.Contains(fileExtension) || file.Length > 5242880)
                    {
                        ModelState.AddModelError("SupportingDocuments", $"File {file.FileName} is invalid or too large (Max 5MB).");
                        return View(model);
                    }

                    var uniqueFileName = Guid.NewGuid().ToString() + fileExtension;
                    var filePath = Path.Combine(uploadsFolder, uniqueFileName);

                    using (var fileStream = new FileStream(filePath, FileMode.Create))
                    {
                        await file.CopyToAsync(fileStream);
                    }

                    var document = new Document
                    {
                        ClaimID = claim.ClaimID,
                        FilePath = $"~/documents/{userId}/{uniqueFileName}",
                        UploadDate = DateTime.Now
                    };
                    _context.Documents.Add(document);
                }
            }

            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Claim submitted successfully and is now pending review.";
            return RedirectToAction("MyClaims");
        }
        catch (DbUpdateException)
        {
            ModelState.AddModelError(string.Empty, "A database error occurred while processing your claim. Please check your inputs and try again.");
        }
        catch (Exception)
        {
            ModelState.AddModelError(string.Empty, "A critical system error occurred. Please contact support.");
        }

        return View(model);
    }

    public async Task<IActionResult> MyClaims()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        var claims = await _context.Claims
            .Where(c => c.UserID == userId)
            .Include(c => c.Approvals)
            .OrderByDescending(c => c.ClaimID)
            .ToListAsync();

        return View(claims);
    }
}
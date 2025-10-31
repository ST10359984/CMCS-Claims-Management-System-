using System.ComponentModel.DataAnnotations;

public class ClaimSubmissionViewModel
{
    [Required]
    public string Month { get; set; }

    [Required]
    [Range(0.01, 9999.99)]
    public decimal HourlyRate { get; set; }

    [Required]
    public List<ClaimDetailViewModel> Details { get; set; } = new List<ClaimDetailViewModel>();

    public IFormFileCollection SupportingDocuments { get; set; }
}

public class ClaimDetailViewModel
{
    [Required]
    public DateTime Date { get; set; }

    [Required]
    [Range(0.01, 24.0)]
    public decimal HoursWorked { get; set; }

    [Required]
    public string Description { get; set; }
}
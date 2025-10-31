using System.ComponentModel.DataAnnotations;

public class Claim
{
    [Key]
    public int ClaimID { get; set; }

    public string UserID { get; set; } = string.Empty;
    public string Month { get; set; } = string.Empty;
    public decimal TotalHours { get; set; }
    public decimal HourlyRate { get; set; }
    public decimal TotalAmount { get; set; }
    public string Status { get; set; } = "Pending";

    public ICollection<ClaimDetail>? ClaimDetails { get; set; }
    public ICollection<Document>? Documents { get; set; }
    public ICollection<Approval>? Approvals { get; set; }
}
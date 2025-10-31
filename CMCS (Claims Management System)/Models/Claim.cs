using System.ComponentModel.DataAnnotations;
using System.Reflection.Metadata;

public class Claim
{
    [Key]
    public int ClaimID { get; set; }
    public int UserID { get; set; }
    public string Month { get; set; }
    public decimal TotalHours { get; set; }
    public decimal HourlyRate { get; set; }
    public decimal TotalAmount { get; set; }
    public string Status { get; set; }

    public User User { get; set; }
    public ICollection<ClaimDetail> ClaimDetails { get; set; }
    public ICollection<Document> Documents { get; set; }
    public ICollection<Approval> Approvals { get; set; }
}
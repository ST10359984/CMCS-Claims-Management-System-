using System.ComponentModel.DataAnnotations;

public class Approval
{
    [Key]
    public int ApprovalID { get; set; }
    public int ClaimID { get; set; }
    public string ApproverID { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
    public string Decision { get; set; } = string.Empty;
    public DateTime Date { get; set; }
    public string Comments { get; set; } = string.Empty;

    public Claim? Claim { get; set; }
}
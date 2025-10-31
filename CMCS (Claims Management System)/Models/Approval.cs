using System.ComponentModel.DataAnnotations;

public class Approval
{
    [Key]
    public int ApprovalID { get; set; }
    public int ClaimID { get; set; }
    public int ApproverID { get; set; }
    public string Role { get; set; }
    public string Decision { get; set; }
    public DateTime Date { get; set; }
    public string Comments { get; set; }

    public Claim Claim { get; set; }
    public User Approver { get; set; }
}
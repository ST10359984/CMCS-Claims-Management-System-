using CMCS.ClaimsManagementSystem.Models;

using System.ComponentModel.DataAnnotations;

public class User
{
    [Key]
    public int UserID { get; set; }

    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;

    public ICollection<Claim>? Claims { get; set; }
    public ICollection<Approval>? Approvals { get; set; }
}
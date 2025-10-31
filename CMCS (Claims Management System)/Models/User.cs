using System.ComponentModel.DataAnnotations;
using System.Security.Claims;

public class User
{
    [Key]
    public int UserID { get; set; }
    public string Name { get; set; }
    public string Email { get; set; }
    public string Role { get; set; }

    public ICollection<Claim> Claims { get; set; }
    public ICollection<Approval> Approvals { get; set; }
}
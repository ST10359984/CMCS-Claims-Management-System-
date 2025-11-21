using CMCS.ClaimsManagementSystem.Models;

using System;
using System.ComponentModel.DataAnnotations;

public class ClaimDetail
{
    [Key]
    public int DetailID { get; set; }
    public int ClaimID { get; set; }
    public DateTime Date { get; set; }
    public decimal HoursWorked { get; set; }
    public string Description { get; set; } = string.Empty;

    public Claim? Claim { get; set; }
}

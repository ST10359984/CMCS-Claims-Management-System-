using CMCS.ClaimsManagementSystem.Models;

using System.ComponentModel.DataAnnotations;

public class Document
{
    [Key]
    public int DocumentID { get; set; }
    public int ClaimID { get; set; }
    public string FilePath { get; set; } = string.Empty;
    public DateTime UploadDate { get; set; }

    public Claim? Claim { get; set; }
}

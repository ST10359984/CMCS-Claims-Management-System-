using System.ComponentModel.DataAnnotations;

public class Document
{
    [Key]
    public int DocumentID { get; set; }
    public int ClaimID { get; set; }
    public string FilePath { get; set; }
    public DateTime UploadDate { get; set; }

    public Claim Claim { get; set; }
}
namespace CMCS__Claims_Management_System_.Models
{
    public class Document
    {
    }
}

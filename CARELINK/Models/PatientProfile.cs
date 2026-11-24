namespace CARELINK.Models;

/// <summary>dbo.PatientProfile — one per patient account. PatientId looks like "PT-000001".</summary>
public class PatientProfile
{
    public string PatientId { get; set; } = string.Empty;
    public string UserId { get; set; } = string.Empty;
    public string? FullName { get; set; }
    public DateOnly? DateOfBirth { get; set; }
    public string? NationalIdEncrypted { get; set; }

    public ApplicationUser? User { get; set; }
}

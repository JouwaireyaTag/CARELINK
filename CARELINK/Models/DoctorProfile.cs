namespace CARELINK.Models;

/// <summary>
/// dbo.DoctorProfile — one per doctor account. DoctorId looks like "DR-000001".
/// A doctor can only request access to patients after an admin sets IsApproved.
/// </summary>
public class DoctorProfile
{
    public string DoctorId { get; set; } = string.Empty;
    public string UserId { get; set; } = string.Empty;
    public string? FullName { get; set; }
    public string? Specialty { get; set; }
    public string? LicenseNumber { get; set; }
    public bool IsApproved { get; set; }

    public ApplicationUser? User { get; set; }
}

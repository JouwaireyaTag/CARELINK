namespace CARELINK.Models;

/// <summary>dbo.Allergies. AllergyId looks like "ALG-000001".</summary>
public class Allergy
{
    public string AllergyId { get; set; } = string.Empty;
    public string PatientId { get; set; } = string.Empty;
    public string? AllergyName { get; set; }
    public string? Reaction { get; set; }
    public string? Severity { get; set; }
    public string? Notes { get; set; }
    public PatientProfile? Patient { get; set; }
}
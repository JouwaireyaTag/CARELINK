using CARELINK.Models;

/// <summary>dbo.Surgeries. SurgeryId looks like "SUR-000001".</summary>
public class Surgery
{
    public string SurgeryId { get; set; } = string.Empty;
    public string PatientId { get; set; } = string.Empty;
    public string? SurgeryName { get; set; }
    public DateOnly? SurgeryDate { get; set; }
    public string? Hospital { get; set; }
    public string? Notes { get; set; }              // column Notes_Surgeries

    public PatientProfile? Patient { get; set; }
}

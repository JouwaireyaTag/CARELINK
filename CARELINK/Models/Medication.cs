using CARELINK.Models;

/// <summary>dbo.Medications. MedicationId looks like "MED-000001".</summary>
public class Medication
{
    public string MedicationId { get; set; } = string.Empty;
    public string PatientId { get; set; } = string.Empty;
    public string? MedicationName { get; set; }
    public string? Dosage { get; set; }
    public string? Frequency { get; set; }
    public DateOnly? StartDate { get; set; }
    public DateOnly? EndDate { get; set; }
    public string? Notes { get; set; }              // column Notes_Medications

    public PatientProfile? Patient { get; set; }
}

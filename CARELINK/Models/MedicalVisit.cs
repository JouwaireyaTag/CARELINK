using CARELINK.Models;

/// <summary>dbo.MedicalVisits. VisitId looks like "VIS-000001".</summary>
public class MedicalVisit
{
    public string VisitId { get; set; } = string.Empty;
    public string PatientId { get; set; } = string.Empty;
    public string DoctorId { get; set; } = string.Empty;
    public DateTime? VisitDate { get; set; }        // UTC
    public string? Diagnosis { get; set; }
    public string? Treatment { get; set; }
    public string? Notes { get; set; }              // column Notes_Visits

    public PatientProfile? Patient { get; set; }
    public DoctorProfile? Doctor { get; set; }
}
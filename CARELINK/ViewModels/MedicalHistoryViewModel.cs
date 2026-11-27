using CARELINK.Models;

namespace CARELINK.ViewModels;

/// <summary>Everything the medical-history page shows for one patient.</summary>
public class MedicalHistoryViewModel
{
    public string PatientId { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
    public int? Age { get; set; }

    /// <summary>Set only in the doctor's view: when the doctor's access ends (UTC).</summary>
    public DateTime? AccessUntilUtc { get; set; }
    public bool IsDoctorView => AccessUntilUtc.HasValue;

    public List<MedicalCondition> Conditions { get; set; } = [];
    public List<Allergy> Allergies { get; set; } = [];
    public List<Surgery> Surgeries { get; set; } = [];
    public List<Medication> Medications { get; set; } = [];
    public List<MedicalVisit> Visits { get; set; } = [];
}
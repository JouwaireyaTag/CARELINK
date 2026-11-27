namespace CARELINK.Models;

/// <summary>dbo.MedicalConditions. ConditionId looks like "CON-000001".</summary>
public class MedicalCondition
{
    public string ConditionId { get; set; } = string.Empty;
    public string PatientId { get; set; } = string.Empty;
    public string? ConditionName { get; set; }
    public DateOnly? DiagnosisDate { get; set; }
    public string? Notes { get; set; }              // column Notes_Condition

    public PatientProfile? Patient { get; set; }
}
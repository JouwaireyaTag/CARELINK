using System.ComponentModel.DataAnnotations;

namespace CARELINK.ViewModels;

public class MedicalConditionFormViewModel
{
    public string? ConditionId { get; set; }

    [Required]
    [StringLength(150)]
    public string ConditionName { get; set; } = string.Empty;

    public DateOnly? DiagnosisDate { get; set; }

    [StringLength(500)]
    public string? Notes { get; set; }
}
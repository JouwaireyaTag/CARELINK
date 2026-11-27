using System.ComponentModel.DataAnnotations;

namespace CARELINK.ViewModels;

public class MedicalVisitFormViewModel
{
    public string? VisitId { get; set; }

    [Required(ErrorMessage = "Visit date is required.")]
    public DateTime? VisitDate { get; set; }

    [Required(ErrorMessage = "Diagnosis is required.")]
    [StringLength(500)]
    public string? Diagnosis { get; set; }

    [StringLength(500)]
    public string? Treatment { get; set; }

    [StringLength(500)]
    public string? Notes { get; set; }
}
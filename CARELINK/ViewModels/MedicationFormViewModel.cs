using System.ComponentModel.DataAnnotations;

namespace CARELINK.ViewModels;

public class MedicationFormViewModel : IValidatableObject
{
    public string? MedicationId { get; set; }

    [Required(ErrorMessage = "Medication name is required.")]
    [StringLength(150)]
    public string? MedicationName { get; set; }

    [StringLength(100)]
    public string? Dosage { get; set; }

    [StringLength(100)]
    public string? Frequency { get; set; }

    public DateOnly? StartDate { get; set; }

    public DateOnly? EndDate { get; set; }

    [StringLength(500)]
    public string? Notes { get; set; }

    public IEnumerable<ValidationResult> Validate(
        ValidationContext validationContext)
    {
        if (StartDate.HasValue &&
            EndDate.HasValue &&
            EndDate.Value < StartDate.Value)
        {
            yield return new ValidationResult(
                "End date cannot be before start date.",
                [nameof(EndDate)]);
        }
    }
}
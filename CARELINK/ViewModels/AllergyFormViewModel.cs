using System.ComponentModel.DataAnnotations;

namespace CARELINK.ViewModels;

public class AllergyFormViewModel
{
    public string? AllergyId { get; set; }

    [Required(ErrorMessage = "Allergy name is required.")]
    [StringLength(150)]
    public string? AllergyName { get; set; }

    [StringLength(200)]
    public string? Reaction { get; set; }

    [StringLength(50)]
    public string? Severity { get; set; }

    [StringLength(500)]
    public string? Notes { get; set; }
}
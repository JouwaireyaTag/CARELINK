using System.ComponentModel.DataAnnotations;

namespace CARELINK.ViewModels;

public class SurgeryFormViewModel
{
    public string? SurgeryId { get; set; }

    [Required(ErrorMessage = "Surgery name is required.")]
    [StringLength(150)]
    public string? SurgeryName { get; set; }

    public DateOnly? SurgeryDate { get; set; }

    [StringLength(150)]
    public string? Hospital { get; set; }

    [StringLength(500)]
    public string? Notes { get; set; }
}
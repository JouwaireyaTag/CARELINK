using System.ComponentModel.DataAnnotations;

namespace CARELINK.ViewModels;

public class PatientProfileViewModel
{
    public string PatientId { get; set; } = string.Empty;
    public string? Email { get; set; }

    [Required, StringLength(150)]
    [Display(Name = "Full name")]
    public string FullName { get; set; } = string.Empty;

    [DataType(DataType.Date)]
    [Display(Name = "Date of birth")]
    public DateOnly? DateOfBirth { get; set; }
}
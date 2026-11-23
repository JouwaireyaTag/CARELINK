using System.ComponentModel.DataAnnotations;

namespace CARELINK.ViewModels;

public class RegisterViewModel
{
    /// <summary>"Patient" or "Doctor". Admins cannot self-register.</summary>
    [Required]
    public string Role { get; set; } = "Patient";

    [Required, StringLength(150), Display(Name = "Full name")]
    public string FullName { get; set; } = string.Empty;

    [Required, EmailAddress, StringLength(256)]
    public string Email { get; set; } = string.Empty;

    [Required, DataType(DataType.Password), StringLength(100, MinimumLength = 8)]
    public string Password { get; set; } = string.Empty;

    [Required, DataType(DataType.Password), Display(Name = "Confirm password")]
    [Compare(nameof(Password), ErrorMessage = "The passwords do not match.")]
    public string ConfirmPassword { get; set; } = string.Empty;

    // Patient only
    [DataType(DataType.Date), Display(Name = "Date of birth")]
    public DateOnly? DateOfBirth { get; set; }

    // Doctor only (checked in the controller when Role is Doctor)
    [StringLength(256)]
    public string? Specialty { get; set; }

    [StringLength(256), Display(Name = "Medical license number")]
    public string? LicenseNumber { get; set; }
}

public class LoginViewModel
{
    [Required, EmailAddress]
    public string Email { get; set; } = string.Empty;

    [Required, DataType(DataType.Password)]
    public string Password { get; set; } = string.Empty;

    [Display(Name = "Remember me")]
    public bool RememberMe { get; set; }

    public string? ReturnUrl { get; set; }
}

public class PendingDoctorRow
{
    public string DoctorId { get; set; } = string.Empty;
    public string? FullName { get; set; }
    public string? Email { get; set; }
    public string? Specialty { get; set; }
    public string? LicenseNumber { get; set; }
    public bool IsApproved { get; set; }
}

public class AdminDoctorsViewModel
{
    public List<PendingDoctorRow> Pending { get; set; } = [];
    public List<PendingDoctorRow> Approved { get; set; } = [];
}

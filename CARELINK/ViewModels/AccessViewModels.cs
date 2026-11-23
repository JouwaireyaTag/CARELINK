using System.ComponentModel.DataAnnotations;
using CARELINK.Models;

namespace CARELINK.ViewModels;

public class DoctorAccessViewModel
{
    public DoctorProfile Doctor { get; set; } = null!;

    [Display(Name = "Patient ID")]
    public string? SearchPatientId { get; set; }

    /// <summary>Set after a search. Only safe fields are shown (no National ID).</summary>
    public PatientSearchResult? Found { get; set; }
    public bool SearchedButNotFound { get; set; }

    public List<AccessRequestRow> Requests { get; set; } = [];
}

public class PatientSearchResult
{
    public string PatientId { get; set; } = string.Empty;

    /// <summary>First name + initial only, e.g. "Sara A."</summary>
    public string DisplayName { get; set; } = string.Empty;
    public int? Age { get; set; }
}

public class EnterCodeViewModel
{
    public AccessRequestRow Request { get; set; } = null!;

    [Required, RegularExpression(@"^\s*\d{6}\s*$", ErrorMessage = "The code is 6 digits.")]
    [Display(Name = "Verification code")]
    public string Code { get; set; } = string.Empty;
}

public class PatientAccessViewModel
{
    public PatientProfile Patient { get; set; } = null!;
    public List<AccessRequestRow> Requests { get; set; } = [];

    /// <summary>Shown once right after approving; never stored in plain text.</summary>
    public string? NewCode { get; set; }
    public string? NewCodeRequestId { get; set; }
}

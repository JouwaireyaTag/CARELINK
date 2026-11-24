namespace CARELINK.ViewModels;

/// <summary>One access request as shown on the doctor's or the patient's page.</summary>
public class AccessRequestRow
{
    public string RequestId { get; set; } = string.Empty;
    public string PatientId { get; set; } = string.Empty;
    public string? PatientName { get; set; }
    public string DoctorId { get; set; } = string.Empty;
    public string? DoctorName { get; set; }
    public string? DoctorSpecialty { get; set; }

    /// <summary>Pending / Approved / Rejected / Expired (database value).</summary>
    public string Status { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; }

    /// <summary>Approved, code not used yet and still valid: the doctor should enter it.</summary>
    public bool WaitingForCode { get; set; }
    public DateTime? CodeExpiresAt { get; set; }
    public int AttemptsLeft { get; set; }

    /// <summary>Code was used and access has not ended.</summary>
    public bool AccessActive { get; set; }
    public DateTime? AccessExpiresAt { get; set; }
}

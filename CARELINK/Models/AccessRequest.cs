namespace CARELINK.Models;

/// <summary>
/// dbo.AccessRequests — a doctor asking to see a patient's medical history.
/// RequestId looks like "REQ-000001".
/// </summary>
public class AccessRequest
{
    public string RequestId { get; set; } = string.Empty;
    public string DoctorId { get; set; } = string.Empty;
    public string PatientId { get; set; } = string.Empty;

    /// <summary>One of <see cref="AccessRequestStatus"/>.</summary>
    public string Status { get; set; } = AccessRequestStatus.Pending;

    /// <summary>UTC.</summary>
    public DateTime CreatedAt { get; set; }

    public DoctorProfile? Doctor { get; set; }
    public PatientProfile? Patient { get; set; }
    public AccessGrant? Grant { get; set; }
}

/// <summary>The values allowed by CK_AccessRequests_Status.</summary>
public static class AccessRequestStatus
{
    /// <summary>Waiting for the patient to approve or reject.</summary>
    public const string Pending = "Pending";

    /// <summary>The patient approved; a code exists (see AccessGrant), or access is active.</summary>
    public const string Approved = "Approved";

    /// <summary>The patient said no.</summary>
    public const string Rejected = "Rejected";

    /// <summary>Timed out, code locked after too many tries, access ended, or revoked.</summary>
    public const string Expired = "Expired";
}

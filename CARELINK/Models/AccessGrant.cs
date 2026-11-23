namespace CARELINK.Models;

/// <summary>
/// dbo.AccessGrants — the one-time verification code created when a patient approves
/// an access request. AccessId looks like "ACC-000001". Only a hash of the code is stored.
/// </summary>
public class AccessGrant
{
    public string AccessId { get; set; } = string.Empty;
    public string RequestId { get; set; } = string.Empty;

    /// <summary>Hash of the 6-digit code (never the code itself).</summary>
    public string CodeHash { get; set; } = string.Empty;

    /// <summary>UTC time the CODE stops working if it has not been used.</summary>
    public DateTime ExpiredAt { get; set; }

    /// <summary>True once the doctor entered the right code. A code works once only.</summary>
    public bool IsUsed { get; set; }

    /// <summary>Wrong codes entered so far.</summary>
    public int Attempts { get; set; }

    /// <summary>
    /// UTC time the doctor's ACCESS ends (set when the code is used; moved to now on revoke).
    /// Column added by Database/Scripts/CARELINK_V2_Auth_Patch.sql.
    /// </summary>
    public DateTime? AccessExpiresAt { get; set; }

    public AccessRequest? Request { get; set; }
}

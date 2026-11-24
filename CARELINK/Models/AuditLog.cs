namespace CARELINK.Models;

/// <summary>dbo.AuditLogs — who did what, and when. AuditId looks like "LOG-000001".</summary>
public class AuditLog
{
    public string AuditId { get; set; } = string.Empty;
    public string? UserId { get; set; }
    public string Action { get; set; } = string.Empty;

    /// <summary>UTC.</summary>
    public DateTime LoggedAt { get; set; }

    public ApplicationUser? User { get; set; }
}

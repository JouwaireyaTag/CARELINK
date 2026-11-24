using CARELINK.Data;
using CARELINK.Models;

namespace CARELINK.Services;

/// <summary>
/// Writes rows to dbo.AuditLogs. Member 4 (Audit Logs) can extend this class
/// (e.g. add a list page); the LogAsync signature is what the auth code depends on.
/// </summary>
public class AuditService(ApplicationDbContext db, TimeProvider clock)
{
    public async Task LogAsync(string? userId, string action, CancellationToken cancellationToken = default)
    {
        db.AuditLogs.Add(new AuditLog
        {
            AuditId = await SequenceIds.NextAsync(db, SequenceIds.Audit, cancellationToken),
            UserId = userId,
            Action = action.Length > 256 ? action[..256] : action,
            LoggedAt = clock.GetUtcNow().UtcDateTime,
        });
        await db.SaveChangesAsync(cancellationToken);
    }
}

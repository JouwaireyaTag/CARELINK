
using CARELINK.Data;
using CARELINK.Models;
using Microsoft.EntityFrameworkCore;

namespace CARELINK.Services;

/// <summary>
/// Writes and reads audit logs from dbo.AuditLogs.
/// </summary>
public class AuditService(ApplicationDbContext db, TimeProvider clock)
{
    public async Task LogAsync(
        string? userId,
        string action,
        CancellationToken cancellationToken = default)
    {
        db.AuditLogs.Add(new AuditLog
        {
            AuditId = await SequenceIds.NextAsync(
                db, SequenceIds.Audit, cancellationToken),

            UserId = userId,

            Action = action.Length > 256
                ? action[..256]
                : action,

            LoggedAt = clock.GetUtcNow().UtcDateTime,
        });

        await db.SaveChangesAsync(cancellationToken);
    }

    /// <summary>
    /// Returns the latest audit logs with their associated users.
    /// </summary>
    public async Task<List<AuditLog>> GetRecentLogsAsync(
        int count = 200,
        CancellationToken cancellationToken = default)
    {
        count = Math.Clamp(count, 1, 500);

        return await db.AuditLogs
            .AsNoTracking()
            .Include(log => log.User)
            .OrderByDescending(log => log.LoggedAt)
            .Take(count)
            .ToListAsync(cancellationToken);
    }
}

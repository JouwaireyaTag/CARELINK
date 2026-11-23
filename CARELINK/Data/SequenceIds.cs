using Microsoft.EntityFrameworkCore;

namespace CARELINK.Data;

/// <summary>
/// Makes IDs like "PT-000001" from the database sequences in CARELINK_Version2.sql,
/// in the same format as the tables' DEFAULT constraints.
/// </summary>
public static class SequenceIds
{
    public static readonly (string Sequence, string Prefix) User = ("Seq_ApplicationUsers", "USR-");
    public static readonly (string Sequence, string Prefix) Patient = ("Seq_PatientProfile", "PT-");
    public static readonly (string Sequence, string Prefix) Doctor = ("Seq_DoctorProfile", "DR-");
    public static readonly (string Sequence, string Prefix) Request = ("Seq_AccessRequests", "REQ-");
    public static readonly (string Sequence, string Prefix) Grant = ("Seq_AccessGrants", "ACC-");
    public static readonly (string Sequence, string Prefix) Audit = ("Seq_AuditLogs", "LOG-");

    private static readonly HashSet<string> AllowedSequences =
    [
        User.Sequence, Patient.Sequence, Doctor.Sequence, Request.Sequence, Grant.Sequence, Audit.Sequence,
    ];

    public static async Task<string> NextAsync(
        DbContext db, (string Sequence, string Prefix) kind, CancellationToken cancellationToken = default)
    {
        // A sequence name cannot be a SQL parameter, so it is put in the SQL text. That is
        // safe only because it must be one of the fixed names above (never user input).
        if (!AllowedSequences.Contains(kind.Sequence))
            throw new ArgumentException($"Unknown sequence '{kind.Sequence}'.", nameof(kind));

        // ToListAsync (not SingleAsync) so EF runs the SQL as-is: SQL Server does not allow
        // NEXT VALUE FOR inside the sub-query EF would wrap it in.
#pragma warning disable EF1002 // checked against AllowedSequences above
        var values = await db.Database
            .SqlQueryRaw<int>($"SELECT NEXT VALUE FOR dbo.[{kind.Sequence}] AS [Value]")
            .ToListAsync(cancellationToken);
#pragma warning restore EF1002

        return $"{kind.Prefix}{values[0]:D6}";
    }
}

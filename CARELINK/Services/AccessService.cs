
using System.Security.Cryptography;
using CARELINK.Data;
using CARELINK.Models;
using CARELINK.ViewModels;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace CARELINK.Services;

/// <summary>
/// Result of an access-flow action. On failure, Error says why (safe to show).
/// </summary>
public record AccessResult(bool Succeeded, string? Error = null)
{
    public string? RequestId { get; init; }
    public string? Code { get; init; }
    public string? PatientId { get; init; }
    public DateTime? AccessExpiresAt { get; init; }

    public static AccessResult Fail(string error) => new(false, error);
}

/// <summary>
/// Handles the patient consent and doctor access workflow.
/// Every step is written to the audit log.
/// </summary>
public class AccessService(
    ApplicationDbContext db,
    AuditService audit,
    IOptions<AccessOptions> options,
    TimeProvider clock)
{
    private readonly AccessOptions _options = options.Value;

    private static readonly PasswordHasher<AccessGrant> CodeHasher = new();

    private DateTime Now => clock.GetUtcNow().UtcDateTime;

    // ---------- Lookups ----------

    public Task<DoctorProfile?> GetDoctorAsync(string doctorUserId) =>
        db.DoctorProfiles.AsNoTracking()
            .FirstOrDefaultAsync(d => d.UserId == doctorUserId);

    public Task<PatientProfile?> GetPatientByUserAsync(string patientUserId) =>
        db.PatientProfiles.AsNoTracking()
            .FirstOrDefaultAsync(p => p.UserId == patientUserId);

    /// <summary>
    /// Exact lookup by Patient ID. Case and spaces are ignored.
    /// </summary>
    public Task<PatientProfile?> FindPatientAsync(string? patientId)
    {
        var id = NormalizeId(patientId);

        return db.PatientProfiles.AsNoTracking()
            .FirstOrDefaultAsync(p => p.PatientId == id);
    }

    // ---------- 1. Doctor requests access ----------

    public async Task<AccessResult> RequestAccessAsync(
        string doctorUserId,
        string? patientId)
    {
        var doctor = await GetDoctorAsync(doctorUserId);

        if (doctor is null || !doctor.IsApproved)
            return AccessResult.Fail(
                "Only doctors approved by the admin can request access.");

        var patient = await FindPatientAsync(patientId);

        if (patient is null)
            return AccessResult.Fail(
                "No patient found with this Patient ID.");

        await ExpireOldRequestsAsync();

        var alreadyOpen = await db.AccessRequests.AnyAsync(r =>
            r.DoctorId == doctor.DoctorId &&
            r.PatientId == patient.PatientId &&
            (r.Status == AccessRequestStatus.Pending ||
             r.Status == AccessRequestStatus.Approved));

        if (alreadyOpen)
            return AccessResult.Fail(
                "You already have an open request or active access for this patient.");

        var request = new AccessRequest
        {
            RequestId = await SequenceIds.NextAsync(
                db, SequenceIds.Request),

            DoctorId = doctor.DoctorId,
            PatientId = patient.PatientId,
            Status = AccessRequestStatus.Pending,
            CreatedAt = Now,
        };

        db.AccessRequests.Add(request);
        await db.SaveChangesAsync();

        await audit.LogAsync(
            doctorUserId,
            $"Requested access to {patient.PatientId} ({request.RequestId})");

        return new AccessResult(true)
        {
            RequestId = request.RequestId
        };
    }

    // ---------- 2. Patient approves / rejects ----------

    public async Task<AccessResult> ApproveAsync(
        string patientUserId,
        string requestId)
    {
        await ExpireOldRequestsAsync();

        var (request, error) =
            await FindPendingForPatientAsync(patientUserId, requestId);

        if (request is null)
            return AccessResult.Fail(error!);

        var code = RandomNumberGenerator
            .GetInt32(0, 1_000_000)
            .ToString("D6");

        var grant = new AccessGrant
        {
            AccessId = await SequenceIds.NextAsync(
                db, SequenceIds.Grant),

            RequestId = request.RequestId,
            ExpiredAt = Now + _options.CodeLifetime,
            IsUsed = false,
            Attempts = 0,
        };

        grant.CodeHash = CodeHasher.HashPassword(grant, code);

        db.AccessGrants.Add(grant);
        request.Status = AccessRequestStatus.Approved;

        await db.SaveChangesAsync();

        await audit.LogAsync(
            patientUserId,
            $"Approved access request {request.RequestId} for {request.DoctorId}");

        return new AccessResult(true)
        {
            Code = code,
            RequestId = request.RequestId
        };
    }

    public async Task<AccessResult> RejectAsync(
        string patientUserId,
        string requestId)
    {
        await ExpireOldRequestsAsync();

        var (request, error) =
            await FindPendingForPatientAsync(patientUserId, requestId);

        if (request is null)
            return AccessResult.Fail(error!);

        request.Status = AccessRequestStatus.Rejected;

        await db.SaveChangesAsync();

        await audit.LogAsync(
            patientUserId,
            $"Rejected access request {request.RequestId} from {request.DoctorId}");

        return new AccessResult(true)
        {
            RequestId = request.RequestId
        };
    }

    // ---------- 3. Doctor verifies the code ----------

    public async Task<AccessResult> VerifyCodeAsync(
        string doctorUserId,
        string requestId,
        string? code)
    {
        await ExpireOldRequestsAsync();

        var doctor = await GetDoctorAsync(doctorUserId);

        if (doctor is null || !doctor.IsApproved)
            return AccessResult.Fail(
                "Only doctors approved by the admin can open patient files.");

        var request = await db.AccessRequests.AsNoTracking()
            .Include(r => r.Grant)
            .FirstOrDefaultAsync(r =>
                r.RequestId == requestId &&
                r.DoctorId == doctor.DoctorId);

        if (request is null)
            return AccessResult.Fail("Access request not found.");

        var grant = request.Grant;

        if (request.Status != AccessRequestStatus.Approved ||
            grant is null)
        {
            return AccessResult.Fail(
                "This request is not approved, or it has expired. Send a new request.");
        }

        if (grant.IsUsed)
            return AccessResult.Fail("This code was already used.");

        code = code?.Trim();

        if (code is null ||
            code.Length != 6 ||
            !code.All(char.IsAsciiDigit))
        {
            return AccessResult.Fail(
                "Enter the 6-digit code the patient gave you.");
        }

        var now = Now;
        var max = _options.MaxCodeAttempts;

        if (CodeHasher.VerifyHashedPassword(
            grant,
            grant.CodeHash,
            code) == PasswordVerificationResult.Failed)
        {
            await db.AccessGrants
                .Where(g =>
                    g.AccessId == grant.AccessId &&
                    !g.IsUsed &&
                    g.Attempts < max)
                .ExecuteUpdateAsync(s =>
                    s.SetProperty(
                        g => g.Attempts,
                        g => g.Attempts + 1));

            var attempts = await db.AccessGrants
                .Where(g => g.AccessId == grant.AccessId)
                .Select(g => g.Attempts)
                .SingleAsync();

            var left = Math.Max(0, max - attempts);

            if (left == 0)
            {
                await SetStatusAsync(
                    request.RequestId,
                    AccessRequestStatus.Expired);

                await audit.LogAsync(
                    doctorUserId,
                    $"Verification code locked after {max} wrong tries ({request.RequestId}, {request.PatientId})");

                return AccessResult.Fail(
                    "Too many wrong codes. This request is closed; send a new one.");
            }

            await audit.LogAsync(
                doctorUserId,
                $"Wrong verification code ({request.RequestId}, {request.PatientId})");

            return AccessResult.Fail(
                $"Wrong code. Attempts left: {left} of {max}.");
        }

        var accessUntil = now + _options.AccessDuration;

        var updated = await db.AccessGrants
            .Where(g =>
                g.AccessId == grant.AccessId &&
                !g.IsUsed &&
                g.ExpiredAt > now &&
                g.Attempts < max)
            .ExecuteUpdateAsync(s => s
                .SetProperty(g => g.IsUsed, true)
                .SetProperty(
                    g => g.AccessExpiresAt,
                    (DateTime?)accessUntil));

        if (updated != 1)
            return AccessResult.Fail(
                "This code is no longer valid. Send a new request.");

        await audit.LogAsync(
            doctorUserId,
            $"Verification code used; access to {request.PatientId} until {accessUntil:yyyy-MM-dd HH:mm} UTC ({request.RequestId})");

        return new AccessResult(true)
        {
            RequestId = request.RequestId,
            PatientId = request.PatientId,
            AccessExpiresAt = accessUntil,
        };
    }

    // ---------- 4. Medical history access checks ----------

    /// <summary>
    /// True if the doctor is approved and currently has access to this patient.
    /// </summary>
    public async Task<bool> HasActiveAccessAsync(
        string doctorUserId,
        string? patientId)
    {
        return await GetActiveAccessUntilAsync(
            doctorUserId, patientId) is not null;
    }

    /// <summary>
    /// Returns when the doctor's access ends (UTC), or null if no active access.
    /// </summary>
    public async Task<DateTime?> GetActiveAccessUntilAsync(
        string doctorUserId,
        string? patientId)
    {
        var id = NormalizeId(patientId);
        var now = Now;

        return await db.AccessRequests.AsNoTracking()
            .Where(r =>
                r.PatientId == id &&
                r.Doctor!.UserId == doctorUserId &&
                r.Doctor.IsApproved &&
                r.Status == AccessRequestStatus.Approved &&
                r.Grant!.IsUsed &&
                r.Grant.AccessExpiresAt > now)
            .Select(r => r.Grant!.AccessExpiresAt)
            .OrderByDescending(t => t)
            .FirstOrDefaultAsync();
    }

    // ---------- 5. Patient revokes access ----------

    public async Task<AccessResult> RevokeAsync(
        string patientUserId,
        string requestId)
    {
        var patient = await GetPatientByUserAsync(patientUserId);

        if (patient is null)
            return AccessResult.Fail("Patient profile not found.");

        var request = await db.AccessRequests
            .Include(r => r.Grant)
            .FirstOrDefaultAsync(r =>
                r.RequestId == requestId &&
                r.PatientId == patient.PatientId);

        if (request is null)
            return AccessResult.Fail("Access request not found.");

        if (request.Status != AccessRequestStatus.Approved)
            return AccessResult.Fail(
                "There is no active access to cancel for this request.");

        var now = Now;

        request.Status = AccessRequestStatus.Expired;

        if (request.Grant is not null)
        {
            if (request.Grant.ExpiredAt > now)
                request.Grant.ExpiredAt = now;

            if (request.Grant.AccessExpiresAt > now)
                request.Grant.AccessExpiresAt = now;
        }

        await db.SaveChangesAsync();

        await audit.LogAsync(
            patientUserId,
            $"Revoked access {request.RequestId} for {request.DoctorId}");

        return new AccessResult(true)
        {
            RequestId = request.RequestId
        };
    }

    // ---------- Lists for the pages ----------

    public async Task<List<AccessRequestRow>> GetDoctorRequestsAsync(
        string doctorUserId)
    {
        await ExpireOldRequestsAsync();

        return await ToRows(
            db.AccessRequests.Where(r => r.Doctor!.UserId == doctorUserId));
    }

    public async Task<List<AccessRequestRow>> GetPatientRequestsAsync(
        string patientUserId)
    {
        await ExpireOldRequestsAsync();

        return await ToRows(
            db.AccessRequests.Where(r => r.Patient!.UserId == patientUserId));
    }

    public async Task<AccessRequestRow?> GetDoctorRequestAsync(
        string doctorUserId,
        string requestId)
    {
        await ExpireOldRequestsAsync();

        return (await ToRows(
            db.AccessRequests.Where(r =>
                r.RequestId == requestId &&
                r.Doctor!.UserId == doctorUserId)))
            .FirstOrDefault();
    }

    // ---------- Helpers ----------

    /// <summary>
    /// Expires outdated requests and records each expiration in the audit log.
    /// </summary>
    private async Task ExpireOldRequestsAsync()
    {
        var now = Now;
        var pendingCutoff = now - _options.RequestLifetime;
        var max = _options.MaxCodeAttempts;

        // 1. Expire old pending requests.
        var expiredPending = await db.AccessRequests
            .Where(r =>
                r.Status == AccessRequestStatus.Pending &&
                r.CreatedAt <= pendingCutoff)
            .ToListAsync();

        foreach (var request in expiredPending)
        {
            request.Status = AccessRequestStatus.Expired;
        }

        if (expiredPending.Count > 0)
        {
            await db.SaveChangesAsync();

            foreach (var request in expiredPending)
            {
                await audit.LogAsync(
                    null,
                    $"System expired pending access request {request.RequestId} for patient {request.PatientId}");
            }
        }

        // 2. Expire approved requests whose code or access period ended.
        var expiredApproved = await db.AccessRequests
            .Include(r => r.Grant)
            .Where(r =>
                r.Status == AccessRequestStatus.Approved &&
                r.Grant != null &&
                (
                    (!r.Grant.IsUsed &&
                        (r.Grant.ExpiredAt <= now ||
                         r.Grant.Attempts >= max))
                    ||
                    (r.Grant.IsUsed &&
                        (r.Grant.AccessExpiresAt == null ||
                         r.Grant.AccessExpiresAt <= now))
                ))
            .ToListAsync();

        foreach (var request in expiredApproved)
        {
            request.Status = AccessRequestStatus.Expired;
        }

        if (expiredApproved.Count > 0)
        {
            await db.SaveChangesAsync();

            foreach (var request in expiredApproved)
            {
                var reason = !request.Grant!.IsUsed
                    ? request.Grant.Attempts >= max
                        ? "maximum wrong attempts reached"
                        : "verification code expired"
                    : "access period ended";

                await audit.LogAsync(
                    null,
                    $"System expired access request {request.RequestId} for patient {request.PatientId}: {reason}");
            }
        }
    }

    private async Task<(AccessRequest? Request, string? Error)>
        FindPendingForPatientAsync(
            string patientUserId,
            string requestId)
    {
        var patient = await GetPatientByUserAsync(patientUserId);

        if (patient is null)
            return (null, "Patient profile not found.");

        var request = await db.AccessRequests
            .FirstOrDefaultAsync(r =>
                r.RequestId == requestId &&
                r.PatientId == patient.PatientId);

        if (request is null)
            return (null, "Access request not found.");

        if (request.Status != AccessRequestStatus.Pending)
            return (null, "This request is no longer pending.");

        return (request, null);
    }

    private Task SetStatusAsync(
        string requestId,
        string status) =>
        db.AccessRequests
            .Where(r => r.RequestId == requestId)
            .ExecuteUpdateAsync(s =>
                s.SetProperty(r => r.Status, status));

    private Task<List<AccessRequestRow>> ToRows(
        IQueryable<AccessRequest> query)
    {
        var now = Now;
        var max = _options.MaxCodeAttempts;

        return query.AsNoTracking()
            .OrderByDescending(r => r.CreatedAt)
            .Select(r => new AccessRequestRow
            {
                RequestId = r.RequestId,
                PatientId = r.PatientId,
                PatientName = r.Patient!.FullName,
                DoctorId = r.DoctorId,
                DoctorName = r.Doctor!.FullName,
                DoctorSpecialty = r.Doctor.Specialty,
                Status = r.Status,
                CreatedAt = r.CreatedAt,

                WaitingForCode =
                    r.Status == AccessRequestStatus.Approved &&
                    r.Grant != null &&
                    !r.Grant.IsUsed &&
                    r.Grant.ExpiredAt > now,

                CodeExpiresAt =
                    r.Grant != null ? r.Grant.ExpiredAt : null,

                AttemptsLeft =
                    r.Grant != null
                        ? max - r.Grant.Attempts
                        : max,

                AccessActive =
                    r.Status == AccessRequestStatus.Approved &&
                    r.Grant != null &&
                    r.Grant.IsUsed &&
                    r.Grant.AccessExpiresAt > now,

                AccessExpiresAt =
                    r.Grant != null
                        ? r.Grant.AccessExpiresAt
                        : null,
            })
            .ToListAsync();
    }

    private static string NormalizeId(string? id) =>
        (id ?? string.Empty).Trim().ToUpperInvariant();
}

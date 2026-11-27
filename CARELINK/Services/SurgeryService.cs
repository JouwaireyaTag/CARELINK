
using CARELINK.Data;
using CARELINK.Models;
using CARELINK.ViewModels;
using Microsoft.EntityFrameworkCore;

namespace CARELINK.Services;

public class SurgeryService(
    ApplicationDbContext db,
    AuditService audit)
{
    public async Task<List<Surgery>> GetByPatientAsync(string patientId)
    {
        var id = patientId.Trim().ToUpperInvariant();

        return await db.Surgeries
            .AsNoTracking()
            .Where(s => s.PatientId == id)
            .OrderByDescending(s => s.SurgeryDate)
            .ToListAsync();
    }

    public async Task<Surgery?> GetByIdAsync(string surgeryId)
    {
        var id = surgeryId.Trim().ToUpperInvariant();

        return await db.Surgeries
            .AsNoTracking()
            .FirstOrDefaultAsync(s => s.SurgeryId == id);
    }

    public async Task CreateAsync(
        string patientId,
        SurgeryFormViewModel model,
        string actorUserId)
    {
        var id = patientId.Trim().ToUpperInvariant();

        var surgery = new Surgery
        {
            SurgeryId = await SequenceIds.NextAsync(db, SequenceIds.Surgery),
            PatientId = id,
            SurgeryName = model.SurgeryName?.Trim(),
            SurgeryDate = model.SurgeryDate,
            Hospital = model.Hospital?.Trim(),
            Notes = model.Notes?.Trim()
        };

        db.Surgeries.Add(surgery);
        await db.SaveChangesAsync();

        await audit.LogAsync(
            actorUserId,
            $"Added surgery {surgery.SurgeryId} for patient {id}");
    }

    public async Task<bool> UpdateAsync(
        string surgeryId,
        string patientId,
        SurgeryFormViewModel model,
        string actorUserId)
    {
        var surgery = await db.Surgeries.FirstOrDefaultAsync(s =>
            s.SurgeryId == surgeryId.Trim().ToUpperInvariant() &&
            s.PatientId == patientId.Trim().ToUpperInvariant());

        if (surgery is null)
            return false;

        surgery.SurgeryName = model.SurgeryName?.Trim();
        surgery.SurgeryDate = model.SurgeryDate;
        surgery.Hospital = model.Hospital?.Trim();
        surgery.Notes = model.Notes?.Trim();

        await db.SaveChangesAsync();

        await audit.LogAsync(
            actorUserId,
            $"Updated surgery {surgery.SurgeryId}");

        return true;
    }

    public async Task<bool> DeleteAsync(
        string surgeryId,
        string patientId,
        string actorUserId)
    {
        var surgery = await db.Surgeries.FirstOrDefaultAsync(s =>
            s.SurgeryId == surgeryId.Trim().ToUpperInvariant() &&
            s.PatientId == patientId.Trim().ToUpperInvariant());

        if (surgery is null)
            return false;

        db.Surgeries.Remove(surgery);
        await db.SaveChangesAsync();

        await audit.LogAsync(
            actorUserId,
            $"Deleted surgery {surgery.SurgeryId}");

        return true;
    }
}

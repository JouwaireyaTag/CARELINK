
using CARELINK.Data;
using CARELINK.Models;
using CARELINK.ViewModels;
using Microsoft.EntityFrameworkCore;

namespace CARELINK.Services;

public class AllergyService(
    ApplicationDbContext db,
    AuditService audit)
{
    public async Task<List<Allergy>> GetByPatientAsync(string patientId)
    {
        var id = patientId.Trim().ToUpperInvariant();

        return await db.Allergies
            .AsNoTracking()
            .Where(a => a.PatientId == id)
            .OrderBy(a => a.AllergyName)
            .ToListAsync();
    }

    public async Task<Allergy?> GetByIdAsync(string allergyId)
    {
        var id = allergyId.Trim().ToUpperInvariant();

        return await db.Allergies
            .AsNoTracking()
            .FirstOrDefaultAsync(a => a.AllergyId == id);
    }

    public async Task CreateAsync(
        string patientId,
        AllergyFormViewModel model,
        string actorUserId)
    {
        var id = patientId.Trim().ToUpperInvariant();

        var allergy = new Allergy
        {
            AllergyId = await SequenceIds.NextAsync(db, SequenceIds.Allergy),
            PatientId = id,
            AllergyName = model.AllergyName?.Trim(),
            Reaction = model.Reaction?.Trim(),
            Severity = model.Severity?.Trim(),
            Notes = model.Notes?.Trim()
        };

        db.Allergies.Add(allergy);
        await db.SaveChangesAsync();

        await audit.LogAsync(
            actorUserId,
            $"Added allergy {allergy.AllergyId} for patient {id}");
    }

    public async Task<bool> UpdateAsync(
        string allergyId,
        string patientId,
        AllergyFormViewModel model,
        string actorUserId)
    {
        var allergy = await db.Allergies.FirstOrDefaultAsync(a =>
            a.AllergyId == allergyId.Trim().ToUpperInvariant() &&
            a.PatientId == patientId.Trim().ToUpperInvariant());

        if (allergy is null)
            return false;

        allergy.AllergyName = model.AllergyName?.Trim();
        allergy.Reaction = model.Reaction?.Trim();
        allergy.Severity = model.Severity?.Trim();
        allergy.Notes = model.Notes?.Trim();

        await db.SaveChangesAsync();

        await audit.LogAsync(
            actorUserId,
            $"Updated allergy {allergy.AllergyId}");

        return true;
    }

    public async Task<bool> DeleteAsync(
        string allergyId,
        string patientId,
        string actorUserId)
    {
        var allergy = await db.Allergies.FirstOrDefaultAsync(a =>
            a.AllergyId == allergyId.Trim().ToUpperInvariant() &&
            a.PatientId == patientId.Trim().ToUpperInvariant());

        if (allergy is null)
            return false;

        db.Allergies.Remove(allergy);
        await db.SaveChangesAsync();

        await audit.LogAsync(
            actorUserId,
            $"Deleted allergy {allergy.AllergyId}");

        return true;
    }
}

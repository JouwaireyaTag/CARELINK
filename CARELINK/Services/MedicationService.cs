
using CARELINK.Data;
using CARELINK.Models;
using CARELINK.ViewModels;
using Microsoft.EntityFrameworkCore;

namespace CARELINK.Services;

public class MedicationService(
    ApplicationDbContext db,
    AuditService audit)
{
    public async Task<List<Medication>> GetByPatientAsync(string patientId)
    {
        var id = patientId.Trim().ToUpperInvariant();

        return await db.Medications
            .AsNoTracking()
            .Where(m => m.PatientId == id)
            .OrderByDescending(m => m.StartDate)
            .ToListAsync();
    }

    public async Task<Medication?> GetByIdAsync(string medicationId)
    {
        var id = medicationId.Trim().ToUpperInvariant();

        return await db.Medications
            .AsNoTracking()
            .FirstOrDefaultAsync(m => m.MedicationId == id);
    }

    public async Task CreateAsync(
        string patientId,
        MedicationFormViewModel model,
        string actorUserId)
    {
        var id = patientId.Trim().ToUpperInvariant();

        ValidateDateRange(model);

        var medication = new Medication
        {
            MedicationId = await SequenceIds.NextAsync(db, SequenceIds.Medication),
            PatientId = id,
            MedicationName = model.MedicationName?.Trim(),
            Dosage = model.Dosage?.Trim(),
            Frequency = model.Frequency?.Trim(),
            StartDate = model.StartDate,
            EndDate = model.EndDate,
            Notes = model.Notes?.Trim()
        };

        db.Medications.Add(medication);
        await db.SaveChangesAsync();

        await audit.LogAsync(
            actorUserId,
            $"Added medication {medication.MedicationId} for patient {id}");
    }

    public async Task<bool> UpdateAsync(
        string medicationId,
        string patientId,
        MedicationFormViewModel model,
        string actorUserId)

    {
        ValidateDateRange(model);

        var medication = await db.Medications.FirstOrDefaultAsync(m =>
            m.MedicationId == medicationId.Trim().ToUpperInvariant() &&
            m.PatientId == patientId.Trim().ToUpperInvariant());

        if (medication is null)
            return false;

        medication.MedicationName = model.MedicationName?.Trim();
        medication.Dosage = model.Dosage?.Trim();
        medication.Frequency = model.Frequency?.Trim();
        medication.StartDate = model.StartDate;
        medication.EndDate = model.EndDate;
        medication.Notes = model.Notes?.Trim();

        await db.SaveChangesAsync();

        await audit.LogAsync(
            actorUserId,
            $"Updated medication {medication.MedicationId}");

        return true;
    }

    public async Task<bool> DeleteAsync(
        string medicationId,
        string patientId,
        string actorUserId)
    {
        var medication = await db.Medications.FirstOrDefaultAsync(m =>
            m.MedicationId == medicationId.Trim().ToUpperInvariant() &&
            m.PatientId == patientId.Trim().ToUpperInvariant());

        if (medication is null)
            return false;

        db.Medications.Remove(medication);
        await db.SaveChangesAsync();

        await audit.LogAsync(
            actorUserId,
            $"Deleted medication {medication.MedicationId}");

        return true;
    }

    private static void ValidateDateRange(MedicationFormViewModel model)
    {
        if (model.StartDate.HasValue &&
            model.EndDate.HasValue &&
            model.EndDate.Value < model.StartDate.Value)
        {
            throw new ArgumentException(
                "Medication end date cannot be before start date.");
        }
    }
}

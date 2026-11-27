
using CARELINK.Data;
using CARELINK.Models;
using CARELINK.ViewModels;
using Microsoft.EntityFrameworkCore;

namespace CARELINK.Services;

public class MedicalConditionService(
    ApplicationDbContext db,
    AuditService audit)
{
    public async Task<List<MedicalCondition>> GetByPatientAsync(string patientId)
    {
        var id = patientId.Trim().ToUpperInvariant();

        return await db.MedicalConditions
            .AsNoTracking()
            .Where(c => c.PatientId == id)
            .OrderByDescending(c => c.DiagnosisDate)
            .ToListAsync();
    }

    public Task<MedicalCondition?> GetByIdAsync(string conditionId)
    {
        var id = conditionId.Trim().ToUpperInvariant();

        return db.MedicalConditions
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.ConditionId == id);
    }

    public async Task CreateAsync(
        string patientId,
        MedicalConditionFormViewModel model,
        string actorUserId)
    {
        var id = patientId.Trim().ToUpperInvariant();

        var condition = new MedicalCondition
        {
            ConditionId = await SequenceIds.NextAsync(db, SequenceIds.Condition),
            PatientId = id,
            ConditionName = model.ConditionName.Trim(),
            DiagnosisDate = model.DiagnosisDate,
            Notes = model.Notes?.Trim()
        };

        db.MedicalConditions.Add(condition);
        await db.SaveChangesAsync();

        await audit.LogAsync(
            actorUserId,
            $"Added medical condition {condition.ConditionId} for patient {id}");
    }

    public async Task<bool> UpdateAsync(
        string conditionId,
        string patientId,
        MedicalConditionFormViewModel model,
        string actorUserId)
    {
        var condition = await db.MedicalConditions.FirstOrDefaultAsync(c =>
            c.ConditionId == conditionId.Trim().ToUpperInvariant() &&
            c.PatientId == patientId.Trim().ToUpperInvariant());

        if (condition is null)
            return false;

        condition.ConditionName = model.ConditionName.Trim();
        condition.DiagnosisDate = model.DiagnosisDate;
        condition.Notes = model.Notes?.Trim();

        await db.SaveChangesAsync();

        await audit.LogAsync(
            actorUserId,
            $"Updated medical condition {condition.ConditionId}");

        return true;
    }

    public async Task<bool> DeleteAsync(
        string conditionId,
        string patientId,
        string actorUserId)
    {
        var condition = await db.MedicalConditions.FirstOrDefaultAsync(c =>
            c.ConditionId == conditionId.Trim().ToUpperInvariant() &&
            c.PatientId == patientId.Trim().ToUpperInvariant());

        if (condition is null)
            return false;

        db.MedicalConditions.Remove(condition);
        await db.SaveChangesAsync();

        await audit.LogAsync(
            actorUserId,
            $"Deleted medical condition {condition.ConditionId}");

        return true;
    }
}

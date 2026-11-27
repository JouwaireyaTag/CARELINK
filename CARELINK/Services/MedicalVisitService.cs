
using CARELINK.Data;
using CARELINK.Models;
using CARELINK.ViewModels;
using Microsoft.EntityFrameworkCore;

namespace CARELINK.Services;

public class MedicalVisitService(
    ApplicationDbContext db,
    AuditService audit)
{
    public async Task<List<MedicalVisit>> GetByPatientAsync(string patientId)
    {
        var id = patientId.Trim().ToUpperInvariant();

        return await db.MedicalVisits
            .AsNoTracking()
            .Include(v => v.Doctor)
            .Where(v => v.PatientId == id)
            .OrderByDescending(v => v.VisitDate)
            .ToListAsync();
    }

    public async Task<MedicalVisit?> GetByIdAsync(string visitId)
    {
        var id = visitId.Trim().ToUpperInvariant();

        return await db.MedicalVisits
            .AsNoTracking()
            .Include(v => v.Doctor)
            .FirstOrDefaultAsync(v => v.VisitId == id);
    }

    public async Task CreateAsync(
        string patientId,
        string doctorId,
        MedicalVisitFormViewModel model,
        string actorUserId)
    {
        var patient = patientId.Trim().ToUpperInvariant();
        var doctor = doctorId.Trim().ToUpperInvariant();

        var visit = new MedicalVisit
        {
            VisitId = await SequenceIds.NextAsync(db, SequenceIds.Visit),
            PatientId = patient,
            DoctorId = doctor,
            VisitDate = model.VisitDate,
            Diagnosis = model.Diagnosis?.Trim(),
            Treatment = model.Treatment?.Trim(),
            Notes = model.Notes?.Trim()
        };

        db.MedicalVisits.Add(visit);
        await db.SaveChangesAsync();

        await audit.LogAsync(
            actorUserId,
            $"Added medical visit {visit.VisitId} for patient {patient}");
    }

    public async Task<bool> UpdateAsync(
        string visitId,
        string patientId,
        MedicalVisitFormViewModel model,
        string actorUserId)
    {
        var visit = await db.MedicalVisits.FirstOrDefaultAsync(v =>
            v.VisitId == visitId.Trim().ToUpperInvariant() &&
            v.PatientId == patientId.Trim().ToUpperInvariant());

        if (visit is null)
            return false;

        visit.VisitDate = model.VisitDate;
        visit.Diagnosis = model.Diagnosis?.Trim();
        visit.Treatment = model.Treatment?.Trim();
        visit.Notes = model.Notes?.Trim();

        await db.SaveChangesAsync();

        await audit.LogAsync(
            actorUserId,
            $"Updated medical visit {visit.VisitId}");

        return true;
    }

    public async Task<bool> DeleteAsync(
        string visitId,
        string patientId,
        string actorUserId)
    {
        var visit = await db.MedicalVisits.FirstOrDefaultAsync(v =>
            v.VisitId == visitId.Trim().ToUpperInvariant() &&
            v.PatientId == patientId.Trim().ToUpperInvariant());

        if (visit is null)
            return false;

        db.MedicalVisits.Remove(visit);
        await db.SaveChangesAsync();

        await audit.LogAsync(
            actorUserId,
            $"Deleted medical visit {visit.VisitId}");

        return true;
    }
}

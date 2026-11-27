using CARELINK.Data;
using CARELINK.ViewModels;
using Microsoft.EntityFrameworkCore;

namespace CARELINK.Services;

/// <summary>Reads a patient's medical history. Access checks are done by the caller (AccessService).</summary>
public class MedicalHistoryService(ApplicationDbContext db)
{
    public async Task<MedicalHistoryViewModel?> GetHistoryAsync(string? patientId, bool shortName)
    {
        var id = (patientId ?? string.Empty).Trim().ToUpperInvariant();
        var patient = await db.PatientProfiles.AsNoTracking().FirstOrDefaultAsync(p => p.PatientId == id);
        if (patient is null) return null;

        return new MedicalHistoryViewModel
        {
            PatientId = patient.PatientId,
            DisplayName = shortName ? ShortName(patient.FullName) : patient.FullName ?? "(no name)",
            Age = AgeOf(patient.DateOfBirth),
            Conditions = await db.MedicalConditions.AsNoTracking()
                .Where(c => c.PatientId == id).OrderByDescending(c => c.DiagnosisDate).ToListAsync(),
            Allergies = await db.Allergies.AsNoTracking()
                .Where(a => a.PatientId == id).OrderBy(a => a.AllergyName).ToListAsync(),
            Surgeries = await db.Surgeries.AsNoTracking()
                .Where(s => s.PatientId == id).OrderByDescending(s => s.SurgeryDate).ToListAsync(),
            Medications = await db.Medications.AsNoTracking()
                .Where(m => m.PatientId == id).OrderByDescending(m => m.StartDate).ToListAsync(),
            Visits = await db.MedicalVisits.AsNoTracking().Include(v => v.Doctor)
                .Where(v => v.PatientId == id).OrderByDescending(v => v.VisitDate).ToListAsync(),
        };
    }

    private static string ShortName(string? fullName)
    {
        var parts = (fullName ?? string.Empty).Split(' ', StringSplitOptions.RemoveEmptyEntries);
        return parts.Length switch
        {
            0 => "(no name)",
            1 => parts[0],
            _ => $"{parts[0]} {parts[^1][0]}.",
        };
    }

    private static int? AgeOf(DateOnly? dob)
    {
        if (dob is not { } d) return null;
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var age = today.Year - d.Year;
        if (d > today.AddYears(-age)) age--;
        return age;
    }
}
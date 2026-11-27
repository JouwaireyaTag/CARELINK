using CARELINK.Data;
using CARELINK.ViewModels;
using Microsoft.EntityFrameworkCore;

namespace CARELINK.Services;

/// <summary>Patient reads and edits their own profile (found by login, never by an id from the form).</summary>
public class PatientProfileService(ApplicationDbContext db, AuditService audit)
{
    public Task<PatientProfileViewModel?> GetAsync(string userId) =>
        db.PatientProfiles.AsNoTracking()
            .Where(p => p.UserId == userId)
            .Select(p => new PatientProfileViewModel
            {
                PatientId = p.PatientId,
                Email = p.User!.Email,
                FullName = p.FullName ?? string.Empty,
                DateOfBirth = p.DateOfBirth,
            })
            .FirstOrDefaultAsync();

    public async Task<bool> UpdateAsync(string userId, PatientProfileViewModel model)
    {
        var profile = await db.PatientProfiles.FirstOrDefaultAsync(p => p.UserId == userId);
        if (profile is null) return false;

        profile.FullName = model.FullName.Trim();
        profile.DateOfBirth = model.DateOfBirth;
        await db.SaveChangesAsync();

        await audit.LogAsync(userId, $"Updated patient profile {profile.PatientId}");
        return true;
    }
}
using CARELINK.Data;
using CARELINK.Models;
using CARELINK.Services;
using CARELINK.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CARELINK.Controllers;

/// <summary>
/// Doctor verification. (Users / Roles / Audit Logs pages of the admin dashboard
/// belong to other team members and can be added to this controller.)
/// </summary>
[Authorize(Roles = AppRoles.Admin)]
public class AdminController(
    ApplicationDbContext db,
    UserManager<ApplicationUser> userManager,
    AuditService audit) : Controller
{
    [HttpGet]
    public async Task<IActionResult> Doctors()
    {
        var now = DateTimeOffset.UtcNow;
        var rows = await db.DoctorProfiles.AsNoTracking()
            // Rejected doctors are locked out permanently, so they drop off the list.
            .Where(d => d.User!.LockoutEnd == null || d.User.LockoutEnd <= now)
            .OrderBy(d => d.DoctorId)
            .Select(d => new PendingDoctorRow
            {
                DoctorId = d.DoctorId,
                FullName = d.FullName,
                Email = d.User!.Email,
                Specialty = d.Specialty,
                LicenseNumber = d.LicenseNumber,
                IsApproved = d.IsApproved,
            })
            .ToListAsync();

        return View(new AdminDoctorsViewModel
        {
            Pending = rows.Where(r => !r.IsApproved).ToList(),
            Approved = rows.Where(r => r.IsApproved).ToList(),
        });
    }

    [HttpPost]
    public async Task<IActionResult> ApproveDoctor(string doctorId)
    {
        var doctor = await db.DoctorProfiles.FirstOrDefaultAsync(d => d.DoctorId == doctorId);
        if (doctor is null) return NotFound();

        if (!doctor.IsApproved)
        {
            doctor.IsApproved = true;
            await db.SaveChangesAsync();
            await audit.LogAsync(userManager.GetUserId(User), $"Approved doctor {doctor.DoctorId}");
        }

        TempData["Message"] = $"{doctor.FullName} ({doctor.DoctorId}) is approved.";
        return RedirectToAction(nameof(Doctors));
    }

    [HttpPost]
    public async Task<IActionResult> RejectDoctor(string doctorId)
    {
        var doctor = await db.DoctorProfiles.Include(d => d.User).FirstOrDefaultAsync(d => d.DoctorId == doctorId);
        if (doctor?.User is null) return NotFound();

        // Keep the row (audit history points to it) but block the account for good
        // and end any session it already has.
        doctor.IsApproved = false;
        await db.SaveChangesAsync();
        await userManager.SetLockoutEnabledAsync(doctor.User, true);
        await userManager.SetLockoutEndDateAsync(doctor.User, DateTimeOffset.MaxValue);
        await userManager.UpdateSecurityStampAsync(doctor.User);

        await audit.LogAsync(userManager.GetUserId(User), $"Rejected doctor {doctor.DoctorId}");
        TempData["Message"] = $"{doctor.FullName} ({doctor.DoctorId}) was rejected and the account is blocked.";
        return RedirectToAction(nameof(Doctors));
    }
}

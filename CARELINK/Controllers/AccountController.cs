using CARELINK.Data;
using CARELINK.Models;
using CARELINK.Services;
using CARELINK.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CARELINK.Controllers;

/// <summary>Register / Login / Logout. Patients and doctors register here; admins are seeded.</summary>
public class AccountController(
    ApplicationDbContext db,
    UserManager<ApplicationUser> userManager,
    SignInManager<ApplicationUser> signInManager,
    AuditService audit) : Controller
{
    [HttpGet, AllowAnonymous]
    public IActionResult Register(string? role)
    {
        if (User.Identity?.IsAuthenticated == true) return RedirectToHome(User.IsInRole);
        return View(new RegisterViewModel { Role = role == AppRoles.Doctor ? AppRoles.Doctor : AppRoles.Patient });
    }

    [HttpPost, AllowAnonymous]
    public async Task<IActionResult> Register(RegisterViewModel model)
    {
        if (model.Role != AppRoles.Patient && model.Role != AppRoles.Doctor)
            ModelState.AddModelError(nameof(model.Role), "Choose Patient or Doctor.");

        if (model.Role == AppRoles.Doctor)
        {
            if (string.IsNullOrWhiteSpace(model.Specialty))
                ModelState.AddModelError(nameof(model.Specialty), "Specialty is required for doctors.");
            if (string.IsNullOrWhiteSpace(model.LicenseNumber))
                ModelState.AddModelError(nameof(model.LicenseNumber), "License number is required for doctors.");
            else if (await db.DoctorProfiles.AnyAsync(d => d.LicenseNumber == model.LicenseNumber.Trim()))
                ModelState.AddModelError(nameof(model.LicenseNumber), "This license number is already registered.");
        }

        if (model.Role == AppRoles.Patient && model.DateOfBirth is { } dob &&
            (dob > DateOnly.FromDateTime(DateTime.UtcNow) || dob.Year < 1900))
            ModelState.AddModelError(nameof(model.DateOfBirth), "Enter a valid date of birth.");

        if (!ModelState.IsValid) return View(model);

        var email = model.Email.Trim();
        var user = new ApplicationUser { UserName = email, Email = email, Role = model.Role };

        // User + profile are saved together: if the profile fails, the account is rolled back.
        await using (var tx = await db.Database.BeginTransactionAsync())
        {
            var result = await userManager.CreateAsync(user, model.Password);
            if (!result.Succeeded)
            {
                foreach (var e in result.Errors) ModelState.AddModelError(string.Empty, e.Description);
                return View(model);
            }

            if (model.Role == AppRoles.Patient)
            {
                db.PatientProfiles.Add(new PatientProfile
                {
                    PatientId = await SequenceIds.NextAsync(db, SequenceIds.Patient),
                    UserId = user.Id,
                    FullName = model.FullName.Trim(),
                    DateOfBirth = model.DateOfBirth,
                });
            }
            else
            {
                db.DoctorProfiles.Add(new DoctorProfile
                {
                    DoctorId = await SequenceIds.NextAsync(db, SequenceIds.Doctor),
                    UserId = user.Id,
                    FullName = model.FullName.Trim(),
                    Specialty = model.Specialty!.Trim(),
                    LicenseNumber = model.LicenseNumber!.Trim(),
                    IsApproved = false,
                });
            }

            await db.SaveChangesAsync();
            await tx.CommitAsync();
        }

        await audit.LogAsync(user.Id, $"Registered as {model.Role}");
        await signInManager.SignInAsync(user, isPersistent: false);
        return RedirectToHome(role => role == user.Role);
    }

    [HttpGet, AllowAnonymous]
    public IActionResult Login(string? returnUrl)
    {
        if (User.Identity?.IsAuthenticated == true) return RedirectToHome(User.IsInRole);
        return View(new LoginViewModel { ReturnUrl = returnUrl });
    }

    [HttpPost, AllowAnonymous]
    public async Task<IActionResult> Login(LoginViewModel model)
    {
        if (!ModelState.IsValid) return View(model);

        var result = await signInManager.PasswordSignInAsync(
            model.Email.Trim(), model.Password, model.RememberMe, lockoutOnFailure: true);

        if (result.IsLockedOut)
        {
            ModelState.AddModelError(string.Empty, "This account is locked. Try again later or contact the admin.");
            return View(model);
        }
        if (!result.Succeeded)
        {
            // Same message for unknown email and wrong password, so emails cannot be guessed.
            ModelState.AddModelError(string.Empty, "Invalid email or password.");
            return View(model);
        }

        var user = await userManager.FindByEmailAsync(model.Email.Trim());
        await audit.LogAsync(user?.Id, "Logged in");

        // Only redirect to our own site (prevents open-redirect attacks).
        if (!string.IsNullOrEmpty(model.ReturnUrl) && Url.IsLocalUrl(model.ReturnUrl))
            return LocalRedirect(model.ReturnUrl);

        return RedirectToHome(role => role == user?.Role);
    }

    [HttpPost, Authorize]
    public async Task<IActionResult> Logout()
    {
        var userId = userManager.GetUserId(User);
        await signInManager.SignOutAsync();
        await audit.LogAsync(userId, "Logged out");
        return RedirectToAction("Index", "Home");
    }

    [HttpGet, AllowAnonymous]
    public IActionResult AccessDenied() => View();

    /// <summary>Each role's start page.</summary>
    private RedirectToActionResult RedirectToHome(Func<string, bool> isInRole)
    {
        if (isInRole(AppRoles.Admin)) return RedirectToAction("Doctors", "Admin");
        if (isInRole(AppRoles.Doctor)) return RedirectToAction("Doctor", "AccessRequest");
        if (isInRole(AppRoles.Patient)) return RedirectToAction("Patient", "AccessRequest");
        return RedirectToAction("Index", "Home");
    }
}

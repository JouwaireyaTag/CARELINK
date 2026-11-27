using CARELINK.Models;
using CARELINK.Services;
using CARELINK.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace CARELINK.Controllers;

/// <summary>Patient's own profile: /PatientProfile and /PatientProfile/Edit.</summary>
[Authorize(Roles = AppRoles.Patient)]
public class PatientProfileController(
    PatientProfileService profiles,
    UserManager<ApplicationUser> userManager) : Controller
{
    private string CurrentUserId => userManager.GetUserId(User)!;

    [HttpGet]
    public async Task<IActionResult> Index()
    {
        var model = await profiles.GetAsync(CurrentUserId);
        return model is null ? NotFound() : View(model);
    }

    [HttpGet]
    public async Task<IActionResult> Edit()
    {
        var model = await profiles.GetAsync(CurrentUserId);
        return model is null ? NotFound() : View(model);
    }

    [HttpPost]
    public async Task<IActionResult> Edit(PatientProfileViewModel model)
    {
        if (model.DateOfBirth is { } dob &&
            (dob > DateOnly.FromDateTime(DateTime.UtcNow) || dob.Year < 1900))
            ModelState.AddModelError(nameof(model.DateOfBirth), "Enter a valid date of birth.");

        if (!ModelState.IsValid) return View(model);

        if (!await profiles.UpdateAsync(CurrentUserId, model)) return NotFound();

        TempData["Message"] = "Profile updated.";
        return RedirectToAction(nameof(Index));
    }
}
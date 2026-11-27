
using CARELINK.Models;
using CARELINK.Services;
using CARELINK.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace CARELINK.Controllers;

[Authorize(Roles = AppRoles.Patient)]
public class AllergyController(
    AllergyService service,
    AccessService access,
    UserManager<ApplicationUser> userManager) : Controller
{
    private string CurrentUserId => userManager.GetUserId(User)!;

    private async Task<string?> GetPatientIdAsync()
    {
        var patient = await access.GetPatientByUserAsync(CurrentUserId);
        return patient?.PatientId;
    }

    public async Task<IActionResult> Index()
    {
        var patientId = await GetPatientIdAsync();
        if (patientId is null) return Forbid();

        return View(await service.GetByPatientAsync(patientId));
    }

    [HttpGet]
    public IActionResult Create() => View(new AllergyFormViewModel());

    [HttpPost]
    public async Task<IActionResult> Create(AllergyFormViewModel model)
    {
        if (!ModelState.IsValid) return View(model);

        var patientId = await GetPatientIdAsync();
        if (patientId is null) return Forbid();

        await service.CreateAsync(patientId, model, CurrentUserId);

        TempData["Success"] = "Allergy added successfully.";
        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    public async Task<IActionResult> Edit(string id)
    {
        var patientId = await GetPatientIdAsync();
        if (patientId is null) return Forbid();

        var allergy = await service.GetByIdAsync(id);

        if (allergy is null || allergy.PatientId != patientId)
            return NotFound();

        return View(new AllergyFormViewModel
        {
            AllergyId = allergy.AllergyId,
            AllergyName = allergy.AllergyName,
            Reaction = allergy.Reaction,
            Severity = allergy.Severity,
            Notes = allergy.Notes
        });
    }

    [HttpPost]
    public async Task<IActionResult> Edit(string id, AllergyFormViewModel model)
    {
        if (!ModelState.IsValid) return View(model);

        var patientId = await GetPatientIdAsync();
        if (patientId is null) return Forbid();

        var updated = await service.UpdateAsync(
            id, patientId, model, CurrentUserId);

        if (!updated) return NotFound();

        TempData["Success"] = "Allergy updated successfully.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    public async Task<IActionResult> Delete(string id)
    {
        var patientId = await GetPatientIdAsync();
        if (patientId is null) return Forbid();

        var deleted = await service.DeleteAsync(
            id, patientId, CurrentUserId);

        if (!deleted) return NotFound();

        TempData["Success"] = "Allergy deleted successfully.";
        return RedirectToAction(nameof(Index));
    }
}

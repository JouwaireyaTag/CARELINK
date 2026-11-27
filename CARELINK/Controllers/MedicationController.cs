
using CARELINK.Models;
using CARELINK.Services;
using CARELINK.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace CARELINK.Controllers;

[Authorize(Roles = AppRoles.Patient)]
public class MedicationController(
    MedicationService service,
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
    public IActionResult Create() => View(new MedicationFormViewModel());

    [HttpPost]
    public async Task<IActionResult> Create(MedicationFormViewModel model)
    {
        if (!ModelState.IsValid) return View(model);

        var patientId = await GetPatientIdAsync();
        if (patientId is null) return Forbid();

        if (model.StartDate.HasValue &&
            model.EndDate.HasValue &&
            model.EndDate < model.StartDate)
        {
            ModelState.AddModelError(
                nameof(model.EndDate),
                "End date cannot be before start date.");

            return View(model);
        }

        await service.CreateAsync(patientId, model, CurrentUserId);

        TempData["Success"] = "Medication added successfully.";
        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    public async Task<IActionResult> Edit(string id)
    {
        var patientId = await GetPatientIdAsync();
        if (patientId is null) return Forbid();

        var medication = await service.GetByIdAsync(id);

        if (medication is null || medication.PatientId != patientId)
            return NotFound();

        return View(new MedicationFormViewModel
        {
            MedicationId = medication.MedicationId,
            MedicationName = medication.MedicationName,
            Dosage = medication.Dosage,
            Frequency = medication.Frequency,
            StartDate = medication.StartDate,
            EndDate = medication.EndDate,
            Notes = medication.Notes
        });
    }

    [HttpPost]
    public async Task<IActionResult> Edit(string id, MedicationFormViewModel model)
    {
        if (!ModelState.IsValid) return View(model);

        var patientId = await GetPatientIdAsync();
        if (patientId is null) return Forbid();

        if (model.StartDate.HasValue &&
            model.EndDate.HasValue &&
            model.EndDate < model.StartDate)
        {
            ModelState.AddModelError(
                nameof(model.EndDate),
                "End date cannot be before start date.");

            return View(model);
        }

        var updated = await service.UpdateAsync(
            id, patientId, model, CurrentUserId);

        if (!updated) return NotFound();

        TempData["Success"] = "Medication updated successfully.";
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

        TempData["Success"] = "Medication deleted successfully.";
        return RedirectToAction(nameof(Index));
    }
}

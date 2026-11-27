
using CARELINK.Models;
using CARELINK.Services;
using CARELINK.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace CARELINK.Controllers;

[Authorize(Roles = AppRoles.Patient)]
public class MedicalConditionController(
    MedicalConditionService service,
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
    public IActionResult Create()
    {
        return View(new MedicalConditionFormViewModel());
    }

    [HttpPost]
    public async Task<IActionResult> Create(MedicalConditionFormViewModel model)
    {
        if (!ModelState.IsValid) return View(model);

        var patientId = await GetPatientIdAsync();
        if (patientId is null) return Forbid();

        await service.CreateAsync(patientId, model, CurrentUserId);

        TempData["Success"] = "Condition added successfully.";
        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    public async Task<IActionResult> Edit(string id)
    {
        var patientId = await GetPatientIdAsync();
        if (patientId is null) return Forbid();

        var condition = await service.GetByIdAsync(id);

        if (condition is null || condition.PatientId != patientId)
            return NotFound();

        return View(new MedicalConditionFormViewModel
        {
            ConditionId = condition.ConditionId,
            ConditionName = condition.ConditionName ?? "",
            DiagnosisDate = condition.DiagnosisDate,
            Notes = condition.Notes
        });
    }

    [HttpPost]
    public async Task<IActionResult> Edit(string id, MedicalConditionFormViewModel model)
    {
        if (!ModelState.IsValid) return View(model);

        var patientId = await GetPatientIdAsync();
        if (patientId is null) return Forbid();

        var updated = await service.UpdateAsync(
            id, patientId, model, CurrentUserId);

        if (!updated) return NotFound();

        TempData["Success"] = "Condition updated successfully.";
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

        TempData["Success"] = "Condition deleted successfully.";
        return RedirectToAction(nameof(Index));
    }
}

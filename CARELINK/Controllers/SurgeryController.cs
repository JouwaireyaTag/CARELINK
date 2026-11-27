
using CARELINK.Models;
using CARELINK.Services;
using CARELINK.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace CARELINK.Controllers;

[Authorize(Roles = AppRoles.Patient)]
public class SurgeryController(
    SurgeryService service,
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
    public IActionResult Create() => View(new SurgeryFormViewModel());

    [HttpPost]
    public async Task<IActionResult> Create(SurgeryFormViewModel model)
    {
        if (!ModelState.IsValid) return View(model);

        var patientId = await GetPatientIdAsync();
        if (patientId is null) return Forbid();

        await service.CreateAsync(patientId, model, CurrentUserId);

        TempData["Success"] = "Surgery added successfully.";
        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    public async Task<IActionResult> Edit(string id)
    {
        var patientId = await GetPatientIdAsync();
        if (patientId is null) return Forbid();

        var surgery = await service.GetByIdAsync(id);

        if (surgery is null || surgery.PatientId != patientId)
            return NotFound();

        return View(new SurgeryFormViewModel
        {
            SurgeryId = surgery.SurgeryId,
            SurgeryName = surgery.SurgeryName,
            SurgeryDate = surgery.SurgeryDate,
            Hospital = surgery.Hospital,
            Notes = surgery.Notes
        });
    }

    [HttpPost]
    public async Task<IActionResult> Edit(string id, SurgeryFormViewModel model)
    {
        if (!ModelState.IsValid) return View(model);

        var patientId = await GetPatientIdAsync();
        if (patientId is null) return Forbid();

        var updated = await service.UpdateAsync(
            id, patientId, model, CurrentUserId);

        if (!updated) return NotFound();

        TempData["Success"] = "Surgery updated successfully.";
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

        TempData["Success"] = "Surgery deleted successfully.";
        return RedirectToAction(nameof(Index));
    }
}

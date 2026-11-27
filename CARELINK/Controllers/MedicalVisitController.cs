
using CARELINK.Models;
using CARELINK.Services;
using CARELINK.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace CARELINK.Controllers;

[Authorize]
public class MedicalVisitController(
    MedicalVisitService service,
    AccessService access,
    AuditService audit,
    UserManager<ApplicationUser> userManager) : Controller
{
    private string CurrentUserId => userManager.GetUserId(User)!;

    // Patient: View own medical visits.
    [HttpGet]
    [Authorize(Roles = AppRoles.Patient)]
    public async Task<IActionResult> Mine()
    {
        var patient = await access.GetPatientByUserAsync(CurrentUserId);

        if (patient is null)
            return Forbid();

        return View("Index",
            await service.GetByPatientAsync(patient.PatientId));
    }

    // Doctor: View a patient's medical visits with active access.
    [HttpGet]
    [Authorize(Policy = AppPolicies.ApprovedDoctor)]
    public async Task<IActionResult> Index(string patientId)
    {
        if (string.IsNullOrWhiteSpace(patientId))
            return BadRequest();

        var until = await access.GetActiveAccessUntilAsync(
            CurrentUserId, patientId);

        if (until is null)
        {
            await audit.LogAsync(
                CurrentUserId,
                $"Denied: tried to view medical visits of {patientId}");

            return Forbid();
        }

        return View(await service.GetByPatientAsync(patientId));
    }

    // Doctor: Display the form to add a medical visit.
    [HttpGet]
    [Authorize(Policy = AppPolicies.ApprovedDoctor)]
    public async Task<IActionResult> Create(string patientId)
    {
        if (string.IsNullOrWhiteSpace(patientId))
            return BadRequest();

        var until = await access.GetActiveAccessUntilAsync(
            CurrentUserId, patientId);

        if (until is null)
        {
            await audit.LogAsync(
                CurrentUserId,
                $"Denied: tried to add a medical visit for {patientId}");

            return Forbid();
        }

        return View(new MedicalVisitFormViewModel
        {
            VisitDate = DateTime.Now
        });
    }

    // Doctor: Save a medical visit.
    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Policy = AppPolicies.ApprovedDoctor)]
    public async Task<IActionResult> Create(
        string patientId,
        MedicalVisitFormViewModel model)
    {
        if (string.IsNullOrWhiteSpace(patientId))
            return BadRequest();

        var until = await access.GetActiveAccessUntilAsync(
            CurrentUserId, patientId);

        if (until is null)
        {
            await audit.LogAsync(
                CurrentUserId,
                $"Denied: tried to add a medical visit for {patientId}");

            return Forbid();
        }

        if (!ModelState.IsValid)
            return View(model);

        var doctor = await access.GetDoctorAsync(CurrentUserId);

        if (doctor is null)
            return Forbid();

        await service.CreateAsync(
            patientId,
            doctor.DoctorId,
            model,
            CurrentUserId);

        TempData["Success"] = "Medical visit added successfully.";

        return RedirectToAction(
            nameof(Index),
            new { patientId });
    }
}

using CARELINK.Models;
using CARELINK.Services;
using CARELINK.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace CARELINK.Controllers;

/// <summary>
/// Doctor:  /MedicalHistory/View/PT-000001  (only with active, patient-approved access)
/// Patient: /MedicalHistory/Mine             (own history)
/// </summary>
[Authorize]
public class MedicalHistoryController(
    MedicalHistoryService history,
    AccessService access,
    AuditService audit,
    UserManager<ApplicationUser> userManager) : Controller
{
    private string CurrentUserId => userManager.GetUserId(User)!;

    // The URL stays /MedicalHistory/View/{id}; the method is not named "View"
    // because that would clash with Controller.View().
    [HttpGet, ActionName("View"), Authorize(Policy = AppPolicies.ApprovedDoctor)]
    public async Task<IActionResult> ViewHistory(string id)
    {
        // Security is checked here in the backend, not by hiding buttons.
        // This check runs before we even look the patient up, so IDs cannot be guessed.
        var until = await access.GetActiveAccessUntilAsync(CurrentUserId, id);
        if (until is null)
        {
            await audit.LogAsync(CurrentUserId, $"Denied: tried to open medical history of {id}");
            return Forbid();
        }

        var model = await history.GetHistoryAsync(id, shortName: true);
        if (model is null) return NotFound();
        model.AccessUntilUtc = until;

        await audit.LogAsync(CurrentUserId, $"Viewed medical history of {model.PatientId}");
        return View("History", model);
    }

    [HttpGet, Authorize(Roles = AppRoles.Patient)]
    public async Task<IActionResult> Mine()
    {
        var patient = await access.GetPatientByUserAsync(CurrentUserId);
        if (patient is null) return Forbid();

        var model = await history.GetHistoryAsync(patient.PatientId, shortName: false);
        return View("History", model);
    }
}
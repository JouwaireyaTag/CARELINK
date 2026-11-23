using CARELINK.Models;
using CARELINK.Services;
using CARELINK.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace CARELINK.Controllers;

/// <summary>
/// Doctor side:  /AccessRequest/Doctor, Create, EnterCode
/// Patient side: /AccessRequest/Patient, Approve, Reject, Revoke
/// All rules live in AccessService; this controller only checks who is calling.
/// </summary>
[Authorize]
public class AccessRequestController(
    AccessService access,
    UserManager<ApplicationUser> userManager) : Controller
{
    private string CurrentUserId => userManager.GetUserId(User)!;

    // ================= Doctor =================

    /// <summary>Doctor start page: search a Patient ID, request access, see my requests.</summary>
    [HttpGet, Authorize(Roles = AppRoles.Doctor)]
    public async Task<IActionResult> Doctor(string? patientId)
    {
        var doctor = await access.GetDoctorAsync(CurrentUserId);
        if (doctor is null) return Forbid();
        if (!doctor.IsApproved) return View("PendingApproval", doctor);

        var model = new DoctorAccessViewModel
        {
            Doctor = doctor,
            SearchPatientId = patientId,
            Requests = await access.GetDoctorRequestsAsync(CurrentUserId),
        };

        if (!string.IsNullOrWhiteSpace(patientId))
        {
            var patient = await access.FindPatientAsync(patientId);
            if (patient is null)
                model.SearchedButNotFound = true;
            else
                model.Found = new PatientSearchResult
                {
                    PatientId = patient.PatientId,
                    DisplayName = ShortName(patient.FullName),
                    Age = AgeOf(patient.DateOfBirth),
                };
        }

        return View(model);
    }

    [HttpPost, Authorize(Policy = AppPolicies.ApprovedDoctor)]
    public async Task<IActionResult> Create(string patientId)
    {
        var result = await access.RequestAccessAsync(CurrentUserId, patientId);
        TempData[result.Succeeded ? "Message" : "Error"] = result.Succeeded
            ? $"Request {result.RequestId} sent. Waiting for the patient to approve."
            : result.Error;
        return RedirectToAction(nameof(Doctor));
    }

    [HttpGet, Authorize(Policy = AppPolicies.ApprovedDoctor)]
    public async Task<IActionResult> EnterCode(string id)
    {
        var row = await access.GetDoctorRequestAsync(CurrentUserId, id);
        if (row is null) return NotFound();
        return View(new EnterCodeViewModel { Request = row });
    }

    [HttpPost, Authorize(Policy = AppPolicies.ApprovedDoctor)]
    public async Task<IActionResult> EnterCode(string id, EnterCodeViewModel model)
    {
        var row = await access.GetDoctorRequestAsync(CurrentUserId, id);
        if (row is null) return NotFound();
        model.Request = row;
        ModelState.Remove(nameof(model.Request));
        if (!ModelState.IsValid) return View(model);

        var result = await access.VerifyCodeAsync(CurrentUserId, id, model.Code);
        if (!result.Succeeded)
        {
            ModelState.AddModelError(nameof(model.Code), result.Error!);
            model.Request = (await access.GetDoctorRequestAsync(CurrentUserId, id))!;
            return View(model);
        }

        TempData["Message"] = $"Access to {result.PatientId} granted until " +
                              $"{result.AccessExpiresAt!.Value.ToLocalTime():HH:mm}.";
        // Member 4's medical-history page goes here when it exists, e.g.
        // return RedirectToAction("View", "MedicalHistory", new { id = result.PatientId });
        return RedirectToAction(nameof(Doctor));
    }

    // ================= Patient =================

    /// <summary>Patient page: pending requests to approve/reject, active access to revoke.</summary>
    [HttpGet, Authorize(Roles = AppRoles.Patient)]
    public async Task<IActionResult> Patient()
    {
        var patient = await access.GetPatientByUserAsync(CurrentUserId);
        if (patient is null) return Forbid();

        return View(new PatientAccessViewModel
        {
            Patient = patient,
            Requests = await access.GetPatientRequestsAsync(CurrentUserId),
            NewCode = TempData["NewCode"] as string,
            NewCodeRequestId = TempData["NewCodeRequestId"] as string,
        });
    }

    [HttpPost, Authorize(Roles = AppRoles.Patient)]
    public async Task<IActionResult> Approve(string id)
    {
        var result = await access.ApproveAsync(CurrentUserId, id);
        if (result.Succeeded)
        {
            // Shown once on the next page, then gone (TempData is read once).
            TempData["NewCode"] = result.Code;
            TempData["NewCodeRequestId"] = result.RequestId;
        }
        else TempData["Error"] = result.Error;
        return RedirectToAction(nameof(Patient));
    }

    [HttpPost, Authorize(Roles = AppRoles.Patient)]
    public async Task<IActionResult> Reject(string id)
    {
        var result = await access.RejectAsync(CurrentUserId, id);
        TempData[result.Succeeded ? "Message" : "Error"] = result.Succeeded ? "Request rejected." : result.Error;
        return RedirectToAction(nameof(Patient));
    }

    [HttpPost, Authorize(Roles = AppRoles.Patient)]
    public async Task<IActionResult> Revoke(string id)
    {
        var result = await access.RevokeAsync(CurrentUserId, id);
        TempData[result.Succeeded ? "Message" : "Error"] = result.Succeeded ? "Access cancelled." : result.Error;
        return RedirectToAction(nameof(Patient));
    }

    // ================= helpers =================

    private static string ShortName(string? fullName)
    {
        var parts = (fullName ?? string.Empty).Split(' ', StringSplitOptions.RemoveEmptyEntries);
        return parts.Length switch
        {
            0 => "(no name)",
            1 => parts[0],
            _ => $"{parts[0]} {parts[^1][0]}.",
        };
    }

    private static int? AgeOf(DateOnly? dob)
    {
        if (dob is not { } d) return null;
        var today = DateOnly.FromDateTime(DateTime.Today);
        var age = today.Year - d.Year;
        if (d > today.AddYears(-age)) age--;
        return age;
    }
}

using System.Security.Claims;
using CARELINK.Data;
using CARELINK.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;

namespace CARELINK.Services;

public static class AppPolicies
{
    /// <summary>
    /// Doctor role AND DoctorProfile.Is_Approved = 1.
    /// Use [Authorize(Policy = AppPolicies.ApprovedDoctor)] on anything that touches patient data.
    /// </summary>
    public const string ApprovedDoctor = "ApprovedDoctor";
}

public class ApprovedDoctorRequirement : IAuthorizationRequirement;

/// <summary>
/// Checks approval in the database on every request (not from the cookie), so an
/// admin's approval takes effect immediately.
/// </summary>
public class ApprovedDoctorHandler(ApplicationDbContext db) : AuthorizationHandler<ApprovedDoctorRequirement>
{
    protected override async Task HandleRequirementAsync(
        AuthorizationHandlerContext context, ApprovedDoctorRequirement requirement)
    {
        if (!context.User.IsInRole(AppRoles.Doctor)) return;

        var userId = context.User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (userId is not null && await db.DoctorProfiles.AnyAsync(d => d.UserId == userId && d.IsApproved))
            context.Succeed(requirement);
    }
}

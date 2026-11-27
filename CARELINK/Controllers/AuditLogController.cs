
using CARELINK.Models;
using CARELINK.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CARELINK.Controllers;

[Authorize(Roles = AppRoles.Admin)]
public class AuditLogController(AuditService audit) : Controller
{
    [HttpGet]
    public async Task<IActionResult> Index(
        CancellationToken cancellationToken)
    {
        var logs = await audit.GetRecentLogsAsync(
            cancellationToken: cancellationToken);

        return View(logs);
    }
}

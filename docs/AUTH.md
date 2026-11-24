# Authentication & Access (Member 3)

What this branch (`feature/authentication`) adds, and how the rest of the team uses it.

## Run it locally

1. Create the database with Member 2's scripts (`feature/database` branch):
   `CARELINK_Version2.sql`, then (optional) `CARELINK_V2_Seed_Data.sql`.
2. Run **`Database/Scripts/CARELINK_V2_Auth_Patch.sql`** (this branch). It adds the
   Identity columns to `ApplicationUsers` and `AccessExpiresAt` to `AccessGrants`.
   In SSMS just run it; with `sqlcmd` add `-I`.
3. The connection string in `appsettings.json` points to
   `(localdb)\MSSQLLocalDB`, database `CARELINK_V2`. Change it locally if your
   SQL Server is different (do not commit passwords).
4. Create the first admin (stored on your machine only, not in git):
   ```
   cd CARELINK
   dotnet user-secrets set "SeedAdmin:Email" "admin@carelink.local"
   dotnet user-secrets set "SeedAdmin:Password" "<a strong password>"
   ```
   The admin is created the next time the site starts.
5. `dotnet run --project CARELINK` (or F5 in Visual Studio).

Seed-data users (`seed.patient@...`) cannot log in: their `PasswordHash` is a placeholder.
Register new demo users from the site.

## How it works

- **Identity on our own table.** ASP.NET Core Identity is mapped to `dbo.ApplicationUsers`
  (not `AspNetUsers`). `Id` is stored in `User_Id` (`USR-000001`). There are no
  `AspNetRoles` tables: the role is the `Role` column (`Patient`, `Doctor`, `Admin`) and is
  put in the login cookie as a role claim.
- **IDs** (`PT-`, `DR-`, `REQ-`, `ACC-`, `LOG-`, `USR-`) come from Member 2's sequences via
  `Data/SequenceIds.cs`, so they look the same as the database defaults.
- **Passwords**: min 8 chars, upper + lower + digit. **Lockout**: 5 wrong passwords → 15 min.
- **Doctor verification**: a doctor registers with specialty + license number and gets
  `Is_Approved = 0`. Admin approves at `/Admin/Doctors` (reject = account blocked).
- **Every POST** needs the anti-forgery token (added automatically by `<form asp-...>`).

### Access flow (consent + one-time code)

| Step | Who | Page | Result |
|---|---|---|---|
| 1 | Doctor | `/AccessRequest/Doctor` → search Patient ID → *Request access* | `AccessRequests.Status = Pending` |
| 2 | Patient | `/AccessRequest/Patient` → *Approve* | 6-digit code shown **once**; only its hash is stored in `AccessGrants` |
| 3 | Doctor | `/AccessRequest/EnterCode/{REQ-id}` | right code → access for 30 min |
| 4 | Patient | *Revoke* | access ends now |

Code rules: one-time use, valid 10 min, tied to one doctor + patient + request,
3 wrong tries → closed. Pending requests expire after 24 h.
All values are in `Services/AccessOptions.cs` (override in `appsettings.json` → `"Access"`).
Every step writes a row to `AuditLogs`.

## For Member 4 (Medical System)

**Before returning any medical-history data to a doctor, check access in the backend:**

```csharp
[Authorize(Policy = AppPolicies.ApprovedDoctor)]
public async Task<IActionResult> View(string id)   // id = Patient ID
{
    var doctorUserId = _userManager.GetUserId(User)!;
    if (!await _access.HasActiveAccessAsync(doctorUserId, id))
        return Forbid();
    await _audit.LogAsync(doctorUserId, $"Viewed medical history {id}");
    ...
}
```

- `AccessService` and `AuditService` are registered in DI; inject them.
- Patients: use `[Authorize(Roles = AppRoles.Patient)]` and get their profile with
  `_access.GetPatientByUserAsync(userId)`; never trust a Patient ID from the URL for "my" data.
- `AuditService.LogAsync(userId, action)` is the shared way to write audit rows; extend the
  class (list page etc.) but keep that method.
- Add your entities (`MedicalConditions`, `Allergies`, ...) to `ApplicationDbContext` with
  the same mapping style (`HasColumnName("Patient_Id")` etc.).
- After a successful code, `AccessRequestController.EnterCode` redirects to the doctor page.
  When your `MedicalHistory/View` exists, change the commented redirect there.

## For Member 5 (Frontend)

The views under `Views/Account`, `Views/AccessRequest`, `Views/Admin` are plain Bootstrap and
work as-is. Restyle them freely; keep the form field names, `asp-for` bindings and the
`asp-action` / `asp-route-id` values. View models are in `ViewModels/`.

| Route | View model |
|---|---|
| `GET/POST /Account/Register` | `RegisterViewModel` |
| `GET/POST /Account/Login` | `LoginViewModel` |
| `POST /Account/Logout` | – |
| `GET /Admin/Doctors`, `POST ApproveDoctor/RejectDoctor` | `AdminDoctorsViewModel` |
| `GET /AccessRequest/Doctor?patientId=` , `POST Create` | `DoctorAccessViewModel` |
| `GET/POST /AccessRequest/EnterCode/{id}` | `EnterCodeViewModel` |
| `GET /AccessRequest/Patient`, `POST Approve/Reject/Revoke/{id}` | `PatientAccessViewModel` |

## Files

```
Database/Scripts/CARELINK_V2_Auth_Patch.sql
CARELINK/Models/        ApplicationUser, AppRoles, PatientProfile, DoctorProfile,
                        AccessRequest (+AccessRequestStatus), AccessGrant, AuditLog
CARELINK/Data/          ApplicationDbContext, ApplicationUserStore, SequenceIds, DbInitializer
CARELINK/Services/      IdentityServiceExtensions, ApplicationClaimsPrincipalFactory,
                        ApprovedDoctorRequirement (+AppPolicies), AccessService,
                        AccessOptions, AuditService
CARELINK/Controllers/   AccountController, AdminController, AccessRequestController
CARELINK/ViewModels/    AccountViewModels, AccessViewModels, AccessRequestRow
CARELINK/Views/         Account/*, Admin/Doctors, AccessRequest/*, Shared/_LoginPartial
```

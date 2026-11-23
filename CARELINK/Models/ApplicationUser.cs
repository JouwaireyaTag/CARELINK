using Microsoft.AspNetCore.Identity;

namespace CARELINK.Models;

/// <summary>
/// A CARELINK account (patient, doctor or admin), stored in dbo.ApplicationUsers.
/// Id is mapped to the User_Id column and has the form "USR-000001".
/// </summary>
public class ApplicationUser : IdentityUser
{
    /// <summary>One of <see cref="AppRoles"/>: Patient, Doctor or Admin.</summary>
    public string Role { get; set; } = string.Empty;
}

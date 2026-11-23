namespace CARELINK.Models;

/// <summary>
/// The three CARELINK roles. Use these constants instead of typing the strings,
/// e.g. [Authorize(Roles = AppRoles.Doctor)].
/// </summary>
public static class AppRoles
{
    public const string Patient = "Patient";
    public const string Doctor = "Doctor";
    public const string Admin = "Admin";

    public static readonly string[] All = [Patient, Doctor, Admin];

    public static bool IsValid(string? role) => role is not null && All.Contains(role);
}

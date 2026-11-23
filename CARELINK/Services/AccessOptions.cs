namespace CARELINK.Services;

/// <summary>
/// Timing and limits for the access-request flow. Defaults below; can be changed in
/// appsettings.json under "Access".
/// </summary>
public class AccessOptions
{
    /// <summary>How long a pending request waits for the patient before it expires.</summary>
    public TimeSpan RequestLifetime { get; set; } = TimeSpan.FromHours(24);

    /// <summary>How long the 6-digit code works after the patient approves.</summary>
    public TimeSpan CodeLifetime { get; set; } = TimeSpan.FromMinutes(10);

    /// <summary>Wrong codes allowed before the code is locked.</summary>
    public int MaxCodeAttempts { get; set; } = 3;

    /// <summary>How long the doctor can view the file after entering the right code.</summary>
    public TimeSpan AccessDuration { get; set; } = TimeSpan.FromMinutes(30);
}

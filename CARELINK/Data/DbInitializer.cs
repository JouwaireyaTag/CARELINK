using CARELINK.Models;
using Microsoft.AspNetCore.Identity;

namespace CARELINK.Data;

/// <summary>
/// Creates the first Admin account at startup, because nobody can register as Admin
/// from the website. Email and password come from configuration (user secrets), never
/// from the repository:
///
///   dotnet user-secrets set "SeedAdmin:Email" "admin@carelink.local"
///   dotnet user-secrets set "SeedAdmin:Password" "YourStrongPassword1"
///
/// If they are not set, nothing happens.
/// </summary>
public static class DbInitializer
{
    public static async Task SeedAdminAsync(IServiceProvider services)
    {
        using var scope = services.CreateScope();
        var config = scope.ServiceProvider.GetRequiredService<IConfiguration>();
        var logger = scope.ServiceProvider.GetRequiredService<ILoggerFactory>().CreateLogger(nameof(DbInitializer));

        var email = config["SeedAdmin:Email"];
        var password = config["SeedAdmin:Password"];
        if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password)) return;

        try
        {
            var users = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
            if (await users.FindByEmailAsync(email) is not null) return;

            var admin = new ApplicationUser { UserName = email, Email = email, Role = AppRoles.Admin, EmailConfirmed = true };
            var result = await users.CreateAsync(admin, password);
            if (result.Succeeded)
                logger.LogInformation("Seed admin {Email} created as {UserId}.", email, admin.Id);
            else
                logger.LogWarning("Seed admin not created: {Errors}",
                    string.Join("; ", result.Errors.Select(e => e.Description)));
        }
        catch (Exception ex)
        {
            // Do not stop the site from starting if the database is not ready yet.
            logger.LogWarning(ex, "Seed admin skipped: could not reach the database.");
        }
    }
}

using CARELINK.Data;
using CARELINK.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace CARELINK.Services;

public static class IdentityServiceExtensions
{
    /// <summary>
    /// Registers the database, ASP.NET Core Identity (on dbo.ApplicationUsers),
    /// the login cookie, the ApprovedDoctor policy and the access/audit services.
    /// </summary>
    public static IServiceCollection AddCarelinkIdentity(
        this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

        services.AddDbContext<ApplicationDbContext>(options => options
            .UseSqlServer(connectionString)
            // We ignore Identity's claims/logins/tokens tables on purpose (see ApplicationDbContext).
            .ConfigureWarnings(w => w.Ignore(CoreEventId.MappedEntityTypeIgnoredWarning)));

        services.AddAuthentication(options =>
            {
                options.DefaultScheme = IdentityConstants.ApplicationScheme;
                options.DefaultSignInScheme = IdentityConstants.ExternalScheme;
            })
            .AddIdentityCookies();

        services.AddIdentityCore<ApplicationUser>(options =>
            {
                options.User.RequireUniqueEmail = true;

                options.Password.RequiredLength = 8;
                options.Password.RequireDigit = true;
                options.Password.RequireLowercase = true;
                options.Password.RequireUppercase = true;
                options.Password.RequireNonAlphanumeric = false;

                // Lock the account for 15 minutes after 5 wrong passwords.
                options.Lockout.AllowedForNewUsers = true;
                options.Lockout.MaxFailedAccessAttempts = 5;
                options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(15);

                // No email sending in CARELINK (yet).
                options.SignIn.RequireConfirmedAccount = false;
            })
            .AddUserStore<ApplicationUserStore>()
            .AddClaimsPrincipalFactory<ApplicationClaimsPrincipalFactory>()
            .AddSignInManager()
            .AddDefaultTokenProviders();

        services.AddAuthorization(options =>
            options.AddPolicy(AppPolicies.ApprovedDoctor, policy => policy
                .RequireRole(AppRoles.Doctor)
                .AddRequirements(new ApprovedDoctorRequirement())));
        services.AddScoped<IAuthorizationHandler, ApprovedDoctorHandler>();

        services.Configure<AccessOptions>(configuration.GetSection("Access"));
        services.AddSingleton(TimeProvider.System);
        services.AddScoped<AuditService>();
        services.AddScoped<AccessService>();

        services.ConfigureApplicationCookie(options =>
        {
            options.LoginPath = "/Account/Login";
            options.LogoutPath = "/Account/Logout";
            options.AccessDeniedPath = "/Account/AccessDenied";
            options.Cookie.Name = "CARELINK.Auth";
            options.Cookie.HttpOnly = true;
            options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
            options.ExpireTimeSpan = TimeSpan.FromHours(1);
            options.SlidingExpiration = true;
        });

        return services;
    }
}

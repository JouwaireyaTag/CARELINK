using System.Security.Claims;
using CARELINK.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;

namespace CARELINK.Services;

/// <summary>
/// Builds the login cookie's claims. The role comes from ApplicationUsers.Role, so
/// [Authorize(Roles = AppRoles.Doctor)] and User.IsInRole(...) work without Identity's
/// role tables.
/// </summary>
public class ApplicationClaimsPrincipalFactory(
    UserManager<ApplicationUser> userManager,
    IOptions<IdentityOptions> optionsAccessor)
    : UserClaimsPrincipalFactory<ApplicationUser>(userManager, optionsAccessor)
{
    // Written in full instead of calling base: the base version also reads the
    // AspNetUserClaims table, which CARELINK does not have.
    protected override async Task<ClaimsIdentity> GenerateClaimsAsync(ApplicationUser user)
    {
        var types = Options.ClaimsIdentity;
        var identity = new ClaimsIdentity(
            IdentityConstants.ApplicationScheme, types.UserNameClaimType, types.RoleClaimType);

        identity.AddClaim(new Claim(types.UserIdClaimType, user.Id));
        identity.AddClaim(new Claim(types.UserNameClaimType, user.UserName ?? user.Email ?? user.Id));

        if (!string.IsNullOrEmpty(user.Email))
            identity.AddClaim(new Claim(types.EmailClaimType, user.Email));

        if (UserManager.SupportsUserSecurityStamp)
            identity.AddClaim(new Claim(types.SecurityStampClaimType,
                await UserManager.GetSecurityStampAsync(user)));

        if (AppRoles.IsValid(user.Role))
            identity.AddClaim(new Claim(types.RoleClaimType, user.Role));

        return identity;
    }
}

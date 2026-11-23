using CARELINK.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;

namespace CARELINK.Data;

/// <summary>
/// Identity's user store, with one change: new users get an Id like "USR-000001"
/// from the dbo.Seq_ApplicationUsers sequence (same format as the database default),
/// instead of Identity's GUID, which would not fit in User_Id VARCHAR(20).
/// </summary>
public class ApplicationUserStore(ApplicationDbContext context, IdentityErrorDescriber? describer = null)
    : UserOnlyStore<ApplicationUser, ApplicationDbContext>(context, describer)
{
    public override async Task<IdentityResult> CreateAsync(
        ApplicationUser user, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(user);
        user.Id = await SequenceIds.NextAsync(Context, SequenceIds.User, cancellationToken);
        return await base.CreateAsync(user, cancellationToken);
    }
}

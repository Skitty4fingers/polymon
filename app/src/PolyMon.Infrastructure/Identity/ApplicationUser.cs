using Microsoft.AspNetCore.Identity;

namespace PolyMon.Infrastructure.Identity;

/// <summary>
/// PolyMon application user. Extends IdentityUser with an optional display name.
/// Users are managed via the Admin → Users page (not wired to the Operator table).
/// </summary>
public class ApplicationUser : IdentityUser
{
    public string? DisplayName { get; set; }
}

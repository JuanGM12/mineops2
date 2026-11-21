using Microsoft.AspNetCore.Identity;

namespace MineOps.Identity.Infrastructure.Identity;

public sealed class ApplicationUser : IdentityUser<Guid>
{
    public string Name { get; set; } = string.Empty;
}

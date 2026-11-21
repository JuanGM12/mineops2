using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace MineOps.Identity.Infrastructure.Identity;

public sealed class MineOpsIdentityDbContext(
    DbContextOptions<MineOpsIdentityDbContext> options)
    : IdentityDbContext<ApplicationUser, IdentityRole<Guid>, Guid>(options)
{
    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);
        builder.Entity<ApplicationUser>()
            .Property(user => user.Name)
            .HasMaxLength(150)
            .IsRequired();
    }
}

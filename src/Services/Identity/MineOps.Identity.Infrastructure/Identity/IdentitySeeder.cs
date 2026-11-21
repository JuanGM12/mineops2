using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;

namespace MineOps.Identity.Infrastructure.Identity;

public sealed class IdentitySeeder(
    RoleManager<IdentityRole<Guid>> roleManager,
    UserManager<ApplicationUser> userManager,
    IOptions<SeedOptions> seedOptions)
{
    public async Task SeedAsync(CancellationToken cancellationToken = default)
    {
        var password = seedOptions.Value.DefaultPassword;

        foreach (var roleName in IdentitySeedData.Roles)
        {
            cancellationToken.ThrowIfCancellationRequested();
            if (await roleManager.RoleExistsAsync(roleName))
            {
                continue;
            }

            EnsureSucceeded(
                await roleManager.CreateAsync(new IdentityRole<Guid>(roleName)),
                $"create role '{roleName}'");
        }

        foreach (var seedUser in IdentitySeedData.Users)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var user = await userManager.FindByEmailAsync(seedUser.Email);

            if (user is null)
            {
                user = new ApplicationUser
                {
                    Id = Guid.NewGuid(),
                    UserName = seedUser.Email,
                    Email = seedUser.Email,
                    EmailConfirmed = true,
                    Name = seedUser.Name
                };

                EnsureSucceeded(
                    await userManager.CreateAsync(user, password),
                    $"create user '{seedUser.Email}'");
            }

            if (!await userManager.IsInRoleAsync(user, seedUser.Role))
            {
                EnsureSucceeded(
                    await userManager.AddToRoleAsync(user, seedUser.Role),
                    $"assign role '{seedUser.Role}' to '{seedUser.Email}'");
            }
        }
    }

    private static void EnsureSucceeded(IdentityResult result, string operation)
    {
        if (result.Succeeded)
        {
            return;
        }

        var errors = string.Join(
            "; ",
            result.Errors.Select(error => $"{error.Code}: {error.Description}"));
        throw new InvalidOperationException($"Identity seed failed while attempting to {operation}: {errors}");
    }
}

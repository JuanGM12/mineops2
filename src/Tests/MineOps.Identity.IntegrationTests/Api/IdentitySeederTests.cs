using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using MineOps.Identity.Infrastructure.Identity;

namespace MineOps.Identity.IntegrationTests.Api;

public sealed class IdentitySeederTests(IdentityApiFixture fixture)
    : IClassFixture<IdentityApiFixture>
{
    [Fact]
    public async Task SeederIsIdempotentAndCreatesUsersRolesAndAssignments()
    {
        await using var scope = fixture.Services.CreateAsyncScope();
        var seeder = scope.ServiceProvider.GetRequiredService<IdentitySeeder>();
        await seeder.SeedAsync();
        await seeder.SeedAsync();

        var dbContext = scope.ServiceProvider.GetRequiredService<MineOpsIdentityDbContext>();
        Assert.Equal(4, await dbContext.Roles.CountAsync());
        Assert.Equal(4, await dbContext.Users.CountAsync());
        Assert.Equal(4, await dbContext.UserRoles.CountAsync());

        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
        foreach (var seedUser in IdentitySeedData.Users)
        {
            var user = await userManager.FindByEmailAsync(seedUser.Email);
            Assert.NotNull(user);
            Assert.True(await userManager.IsInRoleAsync(user, seedUser.Role));
            Assert.False(string.IsNullOrWhiteSpace(user.PasswordHash));
            Assert.NotEqual(IdentityApiFixture.Password, user.PasswordHash);
            Assert.True(await userManager.CheckPasswordAsync(user, IdentityApiFixture.Password));
        }
    }
}

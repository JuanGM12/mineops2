using MineOps.Identity.Infrastructure.Auth;
using MineOps.Identity.Infrastructure.Identity;

namespace MineOps.Identity.IntegrationTests.Configuration;

public sealed class OptionsValidatorTests
{
    [Fact]
    public void JwtValidatorRejectsShortSigningKey()
    {
        var result = new JwtOptionsValidator().Validate(null, new JwtOptions
        {
            Issuer = "MineOps.Identity",
            Audience = "MineOps.Api",
            SigningKey = "short",
            ExpirationMinutes = 60
        });

        Assert.True(result.Failed);
    }

    [Fact]
    public void SeedValidatorRejectsMissingPassword()
    {
        var result = new SeedOptionsValidator().Validate(null, new SeedOptions());

        Assert.True(result.Failed);
    }
}

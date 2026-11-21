using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MineOps.Maintenance.Api.Security;

namespace MineOps.Maintenance.IntegrationTests.Security;

public sealed class JwtConfigurationTests
{
    [Fact]
    public void IncompleteJwtConfigurationFailsFast()
    {
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Jwt:Issuer"] = "MineOps.Identity",
                ["Jwt:Audience"] = "MineOps.Api",
                ["Jwt:SigningKey"] = "short",
                ["Jwt:ExpirationMinutes"] = "60"
            })
            .Build();

        var services = new ServiceCollection();
        Assert.Throws<InvalidOperationException>(() => services.AddJwtAuthentication(configuration));
    }
}

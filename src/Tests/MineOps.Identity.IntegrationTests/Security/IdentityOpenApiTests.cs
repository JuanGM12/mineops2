using System.Net;
using MineOps.Identity.IntegrationTests.Api;

namespace MineOps.Identity.IntegrationTests.Security;

public sealed class IdentityOpenApiTests(IdentityApiFixture fixture)
    : IClassFixture<IdentityApiFixture>
{
    [Fact]
    public async Task SwaggerDocumentsAnonymousLoginEndpoint()
    {
        var response = await fixture.Client.GetAsync("/swagger/v1/swagger.json");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var json = await response.Content.ReadAsStringAsync();
        Assert.Contains("/api/auth/login", json, StringComparison.Ordinal);
    }
}

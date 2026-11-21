using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using MineOps.Planning.IntegrationTests.Api;

namespace MineOps.Planning.IntegrationTests.Security;

public sealed class PlanningSecurityTests(PlanningApiFixture fixture)
    : IClassFixture<PlanningApiFixture>
{
    [Fact]
    public async Task HealthIsAnonymousButActivitiesRequireToken()
    {
        using var client = fixture.CreateClient();
        Assert.Equal(HttpStatusCode.OK, (await client.GetAsync("/health")).StatusCode);
        Assert.Equal(HttpStatusCode.Unauthorized, (await client.GetAsync("/api/activities")).StatusCode);
    }

    [Fact]
    public async Task ValidTokenIsAcceptedAndTamperedOrExpiredTokenReturns401()
    {
        var valid = TestJwtTokens.Create("Operator");
        using var validClient = fixture.CreateClient(valid);
        using var tamperedClient = fixture.CreateClient(TestJwtTokens.Tamper(valid));
        using var expiredClient = fixture.CreateClient(TestJwtTokens.Create("Operator", expired: true));

        Assert.Equal(HttpStatusCode.OK, (await validClient.GetAsync("/api/activities")).StatusCode);
        Assert.Equal(HttpStatusCode.Unauthorized, (await tamperedClient.GetAsync("/api/activities")).StatusCode);
        Assert.Equal(HttpStatusCode.Unauthorized, (await expiredClient.GetAsync("/api/activities")).StatusCode);
    }

    [Fact]
    public async Task TokensWithWrongIssuerOrAudienceReturn401()
    {
        using var wrongIssuerClient = fixture.CreateClient(TestJwtTokens.Create(
            "Operator",
            issuer: "Untrusted.Identity"));
        using var wrongAudienceClient = fixture.CreateClient(TestJwtTokens.Create(
            "Operator",
            audience: "Untrusted.Api"));

        Assert.Equal(
            HttpStatusCode.Unauthorized,
            (await wrongIssuerClient.GetAsync("/api/activities")).StatusCode);
        Assert.Equal(
            HttpStatusCode.Unauthorized,
            (await wrongAudienceClient.GetAsync("/api/activities")).StatusCode);
    }

    [Fact]
    public async Task PlannerCreatesActivityButMaintenanceSupervisorGets403()
    {
        using var planner = fixture.CreateClient(TestJwtTokens.Create("Planner"));
        using var maintenance = fixture.CreateClient(TestJwtTokens.Create("MaintenanceSupervisor"));
        var body = ActivityBody($"SEC-{Guid.NewGuid():N}");

        Assert.Equal(HttpStatusCode.Created,
            (await planner.PostAsJsonAsync("/api/activities", body)).StatusCode);
        Assert.Equal(HttpStatusCode.OK,
            (await maintenance.GetAsync("/api/activities")).StatusCode);
        Assert.Equal(HttpStatusCode.Forbidden,
            (await maintenance.PostAsJsonAsync("/api/activities", ActivityBody($"DENY-{Guid.NewGuid():N}"))).StatusCode);
    }

    [Fact]
    public async Task OperatorCanReadStartAndCompleteButCannotAdminister()
    {
        using var planner = fixture.CreateClient(TestJwtTokens.Create("Planner"));
        using var operatorClient = fixture.CreateClient(TestJwtTokens.Create("Operator"));
        var created = await planner.PostAsJsonAsync("/api/activities", ActivityBody($"OP-{Guid.NewGuid():N}"));
        using var json = JsonDocument.Parse(await created.Content.ReadAsStringAsync());
        var id = json.RootElement.GetProperty("id").GetGuid();

        Assert.Equal(HttpStatusCode.OK, (await operatorClient.GetAsync($"/api/activities/{id}")).StatusCode);
        Assert.Equal(HttpStatusCode.OK, (await operatorClient.PatchAsync($"/api/activities/{id}/start", null)).StatusCode);
        Assert.Equal(HttpStatusCode.OK, (await operatorClient.PatchAsync($"/api/activities/{id}/complete", null)).StatusCode);
        Assert.Equal(HttpStatusCode.Forbidden,
            (await operatorClient.PostAsJsonAsync("/api/activities", ActivityBody($"OP-DENY-{Guid.NewGuid():N}"))).StatusCode);
    }

    [Fact]
    public async Task OperatorCannotUpdateOrCancelActivity()
    {
        using var planner = fixture.CreateClient(TestJwtTokens.Create("Planner"));
        using var operatorClient = fixture.CreateClient(TestJwtTokens.Create("Operator"));
        var created = await planner.PostAsJsonAsync(
            "/api/activities",
            ActivityBody($"OP-ADMIN-DENY-{Guid.NewGuid():N}"));
        using var json = JsonDocument.Parse(await created.Content.ReadAsStringAsync());
        var id = json.RootElement.GetProperty("id").GetGuid();

        Assert.Equal(
            HttpStatusCode.Forbidden,
            (await operatorClient.PutAsJsonAsync($"/api/activities/{id}", UpdateBody())).StatusCode);
        Assert.Equal(
            HttpStatusCode.Forbidden,
            (await operatorClient.PatchAsync($"/api/activities/{id}/cancel", null)).StatusCode);
    }

    [Fact]
    public async Task SwaggerExposesBearerSchemeAndRoleDocumentation()
    {
        using var client = fixture.CreateClient();
        var json = await client.GetStringAsync("/swagger/v1/swagger.json");
        Assert.Contains("bearer", json, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("Administrator", json, StringComparison.Ordinal);
        Assert.Contains("Planner", json, StringComparison.Ordinal);
    }

    private static object ActivityBody(string code) => new
    {
        code,
        title = "Security test activity",
        description = "JWT integration test",
        area = "Operations",
        location = "Mine",
        responsibleId = (Guid?)null,
        priority = "High",
        scheduledDate = DateTimeOffset.UtcNow.AddDays(1),
        dueDate = DateTimeOffset.UtcNow.AddDays(2)
    };

    private static object UpdateBody() => new
    {
        title = "Unauthorized update",
        description = "Operator must not update activities",
        area = "Operations",
        location = "Mine",
        responsibleId = (Guid?)null,
        priority = "High",
        scheduledDate = DateTimeOffset.UtcNow.AddDays(3),
        dueDate = DateTimeOffset.UtcNow.AddDays(4)
    };
}

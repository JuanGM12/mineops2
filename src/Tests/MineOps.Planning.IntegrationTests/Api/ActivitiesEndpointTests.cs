using System.Net;
using System.Net.Http.Json;
using System.Text.Json;

namespace MineOps.Planning.IntegrationTests.Api;

public sealed class ActivitiesEndpointTests(PlanningApiFixture fixture)
    : IClassFixture<PlanningApiFixture>
{
    [Fact]
    public async Task HealthReturnsOkWhenDatabaseIsAvailable()
    {
        var response = await fixture.Client.GetAsync("/health");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task FullActivityApiFlowCoversAllEndpoints()
    {
        var responsibleId = Guid.NewGuid();
        var createResponse = await CreateAsync(
            $"API-{Guid.NewGuid():N}",
            responsibleId,
            DateTimeOffset.UtcNow.AddDays(-2),
            DateTimeOffset.UtcNow.AddDays(-1));
        Assert.Equal(HttpStatusCode.Created, createResponse.StatusCode);
        var activityId = await ReadIdAsync(createResponse);

        Assert.Equal(HttpStatusCode.OK, (await fixture.Client.GetAsync($"/api/activities/{activityId}")).StatusCode);
        Assert.Equal(HttpStatusCode.OK, (await fixture.Client.GetAsync("/api/activities?page=1&pageSize=20")).StatusCode);
        Assert.Equal(HttpStatusCode.OK, (await fixture.Client.GetAsync("/api/activities/pending")).StatusCode);
        Assert.Equal(HttpStatusCode.OK, (await fixture.Client.GetAsync("/api/activities/overdue")).StatusCode);
        Assert.Equal(HttpStatusCode.OK, (await fixture.Client.GetAsync(
            $"/api/activities/responsible/{responsibleId}")).StatusCode);

        var updateResponse = await fixture.Client.PutAsJsonAsync($"/api/activities/{activityId}", new
        {
            title = "Updated activity",
            description = "Updated",
            area = "Processing",
            location = "Plant 1",
            responsibleId,
            priority = "Critical",
            scheduledDate = DateTimeOffset.UtcNow.AddDays(1),
            dueDate = DateTimeOffset.UtcNow.AddDays(2)
        });
        Assert.Equal(HttpStatusCode.OK, updateResponse.StatusCode);

        Assert.Equal(HttpStatusCode.OK, (await fixture.Client.PatchAsync(
            $"/api/activities/{activityId}/start", null)).StatusCode);
        Assert.Equal(HttpStatusCode.OK, (await fixture.Client.PatchAsync(
            $"/api/activities/{activityId}/complete", null)).StatusCode);

        var cancellableResponse = await CreateAsync($"CANCEL-{Guid.NewGuid():N}");
        var cancellableId = await ReadIdAsync(cancellableResponse);
        Assert.Equal(HttpStatusCode.OK, (await fixture.Client.PatchAsync(
            $"/api/activities/{cancellableId}/cancel", null)).StatusCode);
    }

    [Fact]
    public async Task InvalidRequestsUseConsistentErrorContract()
    {
        var badRequest = await fixture.Client.PostAsJsonAsync("/api/activities", new
        {
            code = " ",
            title = " ",
            description = (string?)null,
            area = "Operations",
            location = "Mine",
            responsibleId = (Guid?)null,
            priority = "Low",
            scheduledDate = DateTimeOffset.UtcNow,
            dueDate = (DateTimeOffset?)null
        });
        Assert.Equal(HttpStatusCode.BadRequest, badRequest.StatusCode);
        await AssertErrorContractAsync(badRequest);

        var unprocessable = await fixture.Client.PostAsJsonAsync("/api/activities", new
        {
            code = $"CRITICAL-{Guid.NewGuid():N}",
            title = "Critical without due date",
            description = (string?)null,
            area = "Operations",
            location = "Mine",
            responsibleId = (Guid?)null,
            priority = "Critical",
            scheduledDate = DateTimeOffset.UtcNow,
            dueDate = (DateTimeOffset?)null
        });
        Assert.Equal(HttpStatusCode.UnprocessableEntity, unprocessable.StatusCode);
        await AssertErrorContractAsync(unprocessable);

        var missing = await fixture.Client.GetAsync($"/api/activities/{Guid.NewGuid()}");
        Assert.Equal(HttpStatusCode.NotFound, missing.StatusCode);
        await AssertErrorContractAsync(missing);

        var invalidPage = await fixture.Client.GetAsync("/api/activities?page=1&pageSize=101");
        Assert.Equal(HttpStatusCode.BadRequest, invalidPage.StatusCode);
    }

    [Fact]
    public async Task DuplicateCodeAndInvalidTransitionReturnConflict()
    {
        var code = $"DUP-API-{Guid.NewGuid():N}";
        Assert.Equal(HttpStatusCode.Created, (await CreateAsync(code)).StatusCode);
        var duplicate = await CreateAsync(code);
        Assert.Equal(HttpStatusCode.Conflict, duplicate.StatusCode);

        var created = await CreateAsync($"STATE-{Guid.NewGuid():N}");
        var id = await ReadIdAsync(created);
        Assert.Equal(HttpStatusCode.OK, (await fixture.Client.PatchAsync(
            $"/api/activities/{id}/cancel", null)).StatusCode);
        var invalidStart = await fixture.Client.PatchAsync($"/api/activities/{id}/start", null);
        Assert.Equal(HttpStatusCode.Conflict, invalidStart.StatusCode);
    }

    private Task<HttpResponseMessage> CreateAsync(
        string code,
        Guid? responsibleId = null,
        DateTimeOffset? scheduledDate = null,
        DateTimeOffset? dueDate = null) =>
        fixture.Client.PostAsJsonAsync("/api/activities", new
        {
            code,
            title = "API activity",
            description = "Integration test",
            area = "Operations",
            location = "Mine",
            responsibleId,
            priority = "High",
            scheduledDate = scheduledDate ?? DateTimeOffset.UtcNow.AddDays(1),
            dueDate = dueDate ?? DateTimeOffset.UtcNow.AddDays(2)
        });

    private static async Task<Guid> ReadIdAsync(HttpResponseMessage response)
    {
        using var document = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        return document.RootElement.GetProperty("id").GetGuid();
    }

    private static async Task AssertErrorContractAsync(HttpResponseMessage response)
    {
        using var document = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        Assert.False(document.RootElement.GetProperty("success").GetBoolean());
        Assert.Equal(JsonValueKind.String, document.RootElement.GetProperty("message").ValueKind);
        Assert.Equal(JsonValueKind.Array, document.RootElement.GetProperty("errors").ValueKind);
    }
}

using MineOps.Planning.Application.Activities.Queries;
using MineOps.Planning.Application.Exceptions;
using MineOps.Planning.UnitTests.TestDoubles;

namespace MineOps.Planning.UnitTests.Application;

public sealed class ActivityQueryHandlerTests
{
    [Fact]
    public async Task GetByIdReturnsActivity()
    {
        var repository = new FakeActivityRepository();
        var activity = ActivityFactory.Create();
        repository.Seed(activity);

        var result = await new GetActivityByIdQueryHandler(repository)
            .Handle(new GetActivityByIdQuery(activity.Id), CancellationToken.None);

        Assert.Equal(activity.Id, result.Id);
    }

    [Fact]
    public async Task GetAllReturnsPaginationMetadata()
    {
        var repository = new FakeActivityRepository();
        repository.Seed(ActivityFactory.Create("ACT-001"), ActivityFactory.Create("ACT-002"));

        var result = await new GetActivitiesQueryHandler(repository)
            .Handle(new GetActivitiesQuery(1, 1), CancellationToken.None);

        Assert.Single(result.Items);
        Assert.Equal(2, result.TotalCount);
        Assert.Equal(2, result.TotalPages);
    }

    [Fact]
    public async Task GetAllRejectsPageSizeOverOneHundred()
    {
        var handler = new GetActivitiesQueryHandler(new FakeActivityRepository());

        await Assert.ThrowsAsync<RequestValidationException>(() =>
            handler.Handle(new GetActivitiesQuery(1, 101), CancellationToken.None));
    }

    [Fact]
    public async Task PendingQueryReturnsOnlyPendingActivities()
    {
        var repository = new FakeActivityRepository();
        var pending = ActivityFactory.Create("ACT-PENDING");
        var started = ActivityFactory.Create("ACT-STARTED");
        started.Start();
        repository.Seed(pending, started);

        var result = await new GetPendingActivitiesQueryHandler(repository)
            .Handle(new GetPendingActivitiesQuery(), CancellationToken.None);

        Assert.Single(result.Items);
        Assert.Equal(pending.Id, result.Items[0].Id);
    }

    [Fact]
    public async Task OverdueQueryReturnsOnlyOpenOverdueActivities()
    {
        var repository = new FakeActivityRepository();
        repository.Seed(
            ActivityFactory.Create(
                "ACT-OVERDUE",
                scheduledDate: DateTimeOffset.UtcNow.AddDays(-2),
                dueDate: DateTimeOffset.UtcNow.AddDays(-1)),
            ActivityFactory.Create("ACT-FUTURE"));

        var result = await new GetOverdueActivitiesQueryHandler(repository)
            .Handle(new GetOverdueActivitiesQuery(), CancellationToken.None);

        Assert.Single(result.Items);
        Assert.Equal("ACT-OVERDUE", result.Items[0].Code);
    }

    [Fact]
    public async Task ResponsibleQueryFiltersByResponsibleId()
    {
        var responsibleId = Guid.NewGuid();
        var repository = new FakeActivityRepository();
        repository.Seed(
            ActivityFactory.Create("ACT-MINE", responsibleId),
            ActivityFactory.Create("ACT-OTHER", Guid.NewGuid()));

        var result = await new GetActivitiesByResponsibleQueryHandler(repository)
            .Handle(
                new GetActivitiesByResponsibleQuery(responsibleId),
                CancellationToken.None);

        Assert.Single(result.Items);
        Assert.Equal(responsibleId, result.Items[0].ResponsibleId);
    }
}

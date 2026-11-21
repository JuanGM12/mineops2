using MineOps.Planning.Application.Activities.Commands;
using MineOps.Planning.Application.Exceptions;
using MineOps.Planning.Domain.Activities;
using MineOps.Planning.Domain.Exceptions;
using MineOps.Planning.UnitTests.TestDoubles;

namespace MineOps.Planning.UnitTests.Application;

public sealed class ActivityCommandHandlerTests
{
    [Fact]
    public async Task CreateAddsActivityInsideTransaction()
    {
        var repository = new FakeActivityRepository();
        var unitOfWork = new FakeUnitOfWork();
        var handler = new CreateActivityCommandHandler(repository, unitOfWork);

        var result = await handler.Handle(CreateCommand(), CancellationToken.None);

        Assert.Single(repository.Activities);
        Assert.Equal(result.Id, repository.Activities[0].Id);
        Assert.Equal(1, unitOfWork.TransactionCalls);
        Assert.Equal(1, unitOfWork.SaveCalls);
    }

    [Fact]
    public async Task CreateRejectsDuplicateCode()
    {
        var repository = new FakeActivityRepository();
        repository.Seed(ActivityFactory.Create());
        var handler = new CreateActivityCommandHandler(repository, new FakeUnitOfWork());

        await Assert.ThrowsAsync<ApplicationConflictException>(() =>
            handler.Handle(CreateCommand(), CancellationToken.None));
    }

    [Fact]
    public async Task CreateRejectsMissingTitleAsRequestValidation()
    {
        var handler = new CreateActivityCommandHandler(
            new FakeActivityRepository(),
            new FakeUnitOfWork());

        await Assert.ThrowsAsync<RequestValidationException>(() =>
            handler.Handle(CreateCommand() with { Title = " " }, CancellationToken.None));
    }

    [Fact]
    public async Task UpdateChangesDetailsScheduleAndResponsible()
    {
        var repository = new FakeActivityRepository();
        var activity = ActivityFactory.Create();
        repository.Seed(activity);
        var handler = new UpdateActivityCommandHandler(repository, new FakeUnitOfWork());
        var responsibleId = Guid.NewGuid();
        var scheduled = DateTimeOffset.UtcNow.AddDays(3);

        var result = await handler.Handle(new UpdateActivityCommand(
            activity.Id,
            "Updated title",
            "Updated description",
            "Processing",
            "Plant 1",
            responsibleId,
            ActivityPriority.Critical,
            scheduled,
            scheduled.AddDays(1)), CancellationToken.None);

        Assert.Equal("Updated title", result.Title);
        Assert.Equal(ActivityPriority.Critical, result.Priority);
        Assert.Equal(responsibleId, result.ResponsibleId);
    }

    [Fact]
    public async Task UpdateMissingActivityReturnsNotFound()
    {
        var handler = new UpdateActivityCommandHandler(
            new FakeActivityRepository(),
            new FakeUnitOfWork());

        await Assert.ThrowsAsync<EntityNotFoundException>(() => handler.Handle(
            new UpdateActivityCommand(
                Guid.NewGuid(),
                "Title",
                null,
                "Area",
                "Location",
                null,
                ActivityPriority.Low,
                DateTimeOffset.UtcNow,
                null),
            CancellationToken.None));
    }

    [Fact]
    public async Task TransitionHandlersMoveActivityThroughLifecycle()
    {
        var repository = new FakeActivityRepository();
        var activity = ActivityFactory.Create();
        repository.Seed(activity);
        var unitOfWork = new FakeUnitOfWork();

        await new StartActivityCommandHandler(repository, unitOfWork)
            .Handle(new StartActivityCommand(activity.Id), CancellationToken.None);
        var completed = await new CompleteActivityCommandHandler(repository, unitOfWork)
            .Handle(new CompleteActivityCommand(activity.Id), CancellationToken.None);

        Assert.Equal(ActivityStatus.Completed, completed.Status);
        Assert.Equal(2, unitOfWork.TransactionCalls);
    }

    [Fact]
    public async Task CancelHandlerCancelsPendingActivity()
    {
        var repository = new FakeActivityRepository();
        var activity = ActivityFactory.Create();
        repository.Seed(activity);

        var result = await new CancelActivityCommandHandler(repository, new FakeUnitOfWork())
            .Handle(new CancelActivityCommand(activity.Id), CancellationToken.None);

        Assert.Equal(ActivityStatus.Cancelled, result.Status);
        Assert.Throws<DomainConflictException>(activity.Start);
    }

    private static CreateActivityCommand CreateCommand() => new(
        "ACT-001",
        "Inspect pit",
        "Description",
        "Operations",
        "North pit",
        null,
        ActivityPriority.High,
        DateTimeOffset.UtcNow.AddDays(1),
        DateTimeOffset.UtcNow.AddDays(2));
}

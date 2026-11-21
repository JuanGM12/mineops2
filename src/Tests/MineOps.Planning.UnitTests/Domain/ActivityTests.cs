using MineOps.Planning.Domain.Activities;
using MineOps.Planning.Domain.Exceptions;

namespace MineOps.Planning.UnitTests.Domain;

public sealed class ActivityTests
{
    private static readonly DateTimeOffset Scheduled = new(2026, 11, 20, 13, 0, 0, TimeSpan.FromHours(-5));

    [Fact]
    public void ConstructorCreatesPendingActivityAndNormalizesDatesToUtc()
    {
        var activity = Create();

        Assert.NotEqual(Guid.Empty, activity.Id);
        Assert.Equal(ActivityStatus.Pending, activity.Status);
        Assert.Equal(TimeSpan.Zero, activity.ScheduledDate.Offset);
        Assert.Equal(TimeSpan.Zero, activity.DueDate?.Offset);
        Assert.Equal(TimeSpan.Zero, activity.CreatedAt.Offset);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void ConstructorRejectsEmptyTitle(string title)
    {
        Assert.Throws<DomainRuleException>(() => Create(title: title));
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void ConstructorRejectsEmptyCode(string code)
    {
        Assert.Throws<DomainRuleException>(() => Create(code: code));
    }

    [Fact]
    public void ConstructorRejectsDueDateBeforeScheduledDate()
    {
        Assert.Throws<DomainRuleException>(() => Create(dueDate: Scheduled.AddMinutes(-1)));
    }

    [Fact]
    public void ConstructorRejectsCriticalActivityWithoutDueDate()
    {
        Assert.Throws<DomainRuleException>(() => Create(priority: ActivityPriority.Critical, includeDueDate: false));
    }

    [Fact]
    public void StartMovesPendingActivityToInProgress()
    {
        var activity = Create();

        activity.Start();

        Assert.Equal(ActivityStatus.InProgress, activity.Status);
    }

    [Fact]
    public void CompletedActivityCannotStartAgain()
    {
        var activity = Create();
        activity.Start();
        activity.Complete();

        Assert.Throws<DomainConflictException>(activity.Start);
    }

    [Fact]
    public void CancelledActivityCannotBeCompleted()
    {
        var activity = Create();
        activity.Cancel();

        Assert.Throws<DomainConflictException>(activity.Complete);
    }

    [Fact]
    public void PendingActivityCannotBeCompleted()
    {
        var activity = Create();

        Assert.Throws<DomainConflictException>(activity.Complete);
    }

    [Fact]
    public void CompletedActivityCannotBeCancelled()
    {
        var activity = Create();
        activity.Start();
        activity.Complete();

        Assert.Throws<DomainConflictException>(activity.Cancel);
    }

    [Fact]
    public void RescheduleUpdatesUtcDates()
    {
        var activity = Create();
        var newDate = Scheduled.AddDays(2);

        activity.Reschedule(newDate, newDate.AddDays(1));

        Assert.Equal(newDate.ToUniversalTime(), activity.ScheduledDate);
        Assert.Equal(newDate.AddDays(1).ToUniversalTime(), activity.DueDate);
    }

    [Fact]
    public void AssignResponsibleRejectsEmptyGuid()
    {
        var activity = Create();

        Assert.Throws<DomainRuleException>(() => activity.AssignResponsible(Guid.Empty));
    }

    [Fact]
    public void UpdateDetailsRejectsCriticalPriorityWithoutDueDate()
    {
        var activity = Create(includeDueDate: false);

        Assert.Throws<DomainRuleException>(() => activity.UpdateDetails(
            "Updated", null, "Mine", "North", ActivityPriority.Critical));
    }

    private static Activity Create(
        string code = "ACT-001",
        string title = "Inspect pit",
        ActivityPriority priority = ActivityPriority.High,
        DateTimeOffset? dueDate = null,
        bool includeDueDate = true)
    {
        DateTimeOffset? effectiveDueDate = includeDueDate ? dueDate ?? Scheduled.AddDays(1) : null;

        return new Activity(
            code,
            title,
            "Description",
            "Operations",
            "North pit",
            null,
            priority,
            Scheduled,
            effectiveDueDate);
    }
}

using MineOps.Planning.Domain.Exceptions;

namespace MineOps.Planning.Domain.Activities;

public sealed class Activity
{
    private Activity()
    {
    }

    public Activity(
        string code,
        string title,
        string? description,
        string area,
        string location,
        Guid? responsibleId,
        ActivityPriority priority,
        DateTimeOffset scheduledDate,
        DateTimeOffset? dueDate)
    {
        Code = Required(code, nameof(code));
        Title = Required(title, nameof(title));
        Description = NormalizeOptional(description);
        Area = Normalize(area);
        Location = Normalize(location);
        Priority = priority;
        ValidateResponsible(responsibleId);
        ResponsibleId = responsibleId;
        SetSchedule(scheduledDate, dueDate, priority);

        Id = Guid.NewGuid();
        Status = ActivityStatus.Pending;
        CreatedAt = DateTimeOffset.UtcNow;
        UpdatedAt = CreatedAt;
    }

    public Guid Id { get; private set; }

    public string Code { get; private set; } = string.Empty;

    public string Title { get; private set; } = string.Empty;

    public string? Description { get; private set; }

    public string Area { get; private set; } = string.Empty;

    public string Location { get; private set; } = string.Empty;

    public Guid? ResponsibleId { get; private set; }

    public ActivityPriority Priority { get; private set; }

    public ActivityStatus Status { get; private set; }

    public DateTimeOffset ScheduledDate { get; private set; }

    public DateTimeOffset? DueDate { get; private set; }

    public DateTimeOffset CreatedAt { get; private set; }

    public DateTimeOffset UpdatedAt { get; private set; }

    public void Start()
    {
        if (Status is not ActivityStatus.Pending)
        {
            throw new DomainConflictException($"Only pending activities can be started. Current status: {Status}.");
        }

        Status = ActivityStatus.InProgress;
        Touch();
    }

    public void Complete()
    {
        if (Status is ActivityStatus.Cancelled)
        {
            throw new DomainConflictException("A cancelled activity cannot be completed.");
        }

        if (Status is not ActivityStatus.InProgress)
        {
            throw new DomainConflictException($"Only in-progress activities can be completed. Current status: {Status}.");
        }

        Status = ActivityStatus.Completed;
        Touch();
    }

    public void Cancel()
    {
        if (Status is ActivityStatus.Completed or ActivityStatus.Cancelled)
        {
            throw new DomainConflictException($"An activity in status {Status} cannot be cancelled.");
        }

        Status = ActivityStatus.Cancelled;
        Touch();
    }

    public void Reschedule(DateTimeOffset scheduledDate, DateTimeOffset? dueDate)
    {
        EnsureMutable();
        SetSchedule(scheduledDate, dueDate, Priority);
        Touch();
    }

    public void AssignResponsible(Guid responsibleId)
    {
        EnsureMutable();
        ValidateResponsible(responsibleId);
        ResponsibleId = responsibleId;
        Touch();
    }

    public void UpdateDetails(
        string title,
        string? description,
        string area,
        string location,
        ActivityPriority priority)
    {
        EnsureMutable();

        if (priority is ActivityPriority.Critical && DueDate is null)
        {
            throw new DomainRuleException("A critical activity must have a due date.");
        }

        Title = Required(title, nameof(title));
        Description = NormalizeOptional(description);
        Area = Normalize(area);
        Location = Normalize(location);
        Priority = priority;
        Touch();
    }

    private void SetSchedule(
        DateTimeOffset scheduledDate,
        DateTimeOffset? dueDate,
        ActivityPriority priority)
    {
        var utcScheduledDate = scheduledDate.ToUniversalTime();
        var utcDueDate = dueDate?.ToUniversalTime();

        if (utcDueDate < utcScheduledDate)
        {
            throw new DomainRuleException("Due date cannot be earlier than scheduled date.");
        }

        if (priority is ActivityPriority.Critical && utcDueDate is null)
        {
            throw new DomainRuleException("A critical activity must have a due date.");
        }

        ScheduledDate = utcScheduledDate;
        DueDate = utcDueDate;
    }

    private void EnsureMutable()
    {
        if (Status is ActivityStatus.Completed or ActivityStatus.Cancelled)
        {
            throw new DomainConflictException($"An activity in status {Status} cannot be modified.");
        }
    }

    private static string Required(string value, string field)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new DomainRuleException($"{field} is required.");
        }

        return value.Trim();
    }

    private static string Normalize(string? value) => value?.Trim() ?? string.Empty;

    private static string? NormalizeOptional(string? value) =>
        string.IsNullOrWhiteSpace(value) ? null : value.Trim();

    private static void ValidateResponsible(Guid? responsibleId)
    {
        if (responsibleId == Guid.Empty)
        {
            throw new DomainRuleException("Responsible id cannot be an empty GUID.");
        }
    }

    private void Touch() => UpdatedAt = DateTimeOffset.UtcNow;
}

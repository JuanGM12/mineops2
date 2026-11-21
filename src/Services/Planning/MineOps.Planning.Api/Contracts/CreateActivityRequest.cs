using MineOps.Planning.Domain.Activities;

namespace MineOps.Planning.Api.Contracts;

public sealed record CreateActivityRequest(
    string Code,
    string Title,
    string? Description,
    string Area,
    string Location,
    Guid? ResponsibleId,
    ActivityPriority Priority,
    DateTimeOffset ScheduledDate,
    DateTimeOffset? DueDate);

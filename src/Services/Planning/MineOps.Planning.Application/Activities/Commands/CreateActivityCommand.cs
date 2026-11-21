using MediatR;
using MineOps.Planning.Domain.Activities;

namespace MineOps.Planning.Application.Activities.Commands;

public sealed record CreateActivityCommand(
    string Code,
    string Title,
    string? Description,
    string Area,
    string Location,
    Guid? ResponsibleId,
    ActivityPriority Priority,
    DateTimeOffset ScheduledDate,
    DateTimeOffset? DueDate) : IRequest<ActivityDto>;

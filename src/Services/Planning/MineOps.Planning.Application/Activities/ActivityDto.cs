using MineOps.Planning.Application.Common;
using MineOps.Planning.Domain.Activities;

namespace MineOps.Planning.Application.Activities;

public sealed record ActivityDto(
    Guid Id,
    string Code,
    string Title,
    string? Description,
    string Area,
    string Location,
    Guid? ResponsibleId,
    ActivityPriority Priority,
    ActivityStatus Status,
    DateTimeOffset ScheduledDate,
    DateTimeOffset? DueDate,
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt)
{
    public static ActivityDto FromDomain(Activity activity) => new(
        activity.Id,
        activity.Code,
        activity.Title,
        activity.Description,
        activity.Area,
        activity.Location,
        activity.ResponsibleId,
        activity.Priority,
        activity.Status,
        activity.ScheduledDate,
        activity.DueDate,
        activity.CreatedAt,
        activity.UpdatedAt);

    public static PagedResult<ActivityDto> FromDomainPage(PagedResult<Activity> page) => new(
        page.Items.Select(FromDomain).ToArray(),
        page.Page,
        page.PageSize,
        page.TotalCount);
}

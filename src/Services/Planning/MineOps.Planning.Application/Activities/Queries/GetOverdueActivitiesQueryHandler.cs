using MediatR;
using MineOps.Planning.Application.Abstractions;
using MineOps.Planning.Application.Common;

namespace MineOps.Planning.Application.Activities.Queries;

public sealed class GetOverdueActivitiesQueryHandler(IActivityRepository repository)
    : IRequestHandler<GetOverdueActivitiesQuery, PagedResult<ActivityDto>>
{
    public async Task<PagedResult<ActivityDto>> Handle(
        GetOverdueActivitiesQuery request,
        CancellationToken cancellationToken)
    {
        Pagination.Validate(request.Page, request.PageSize);
        var page = await repository.GetOverdueAsync(
            DateTimeOffset.UtcNow,
            request.Page,
            request.PageSize,
            cancellationToken);
        return ActivityDto.FromDomainPage(page);
    }
}

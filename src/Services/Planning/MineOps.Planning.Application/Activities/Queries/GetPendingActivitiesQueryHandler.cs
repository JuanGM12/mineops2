using MediatR;
using MineOps.Planning.Application.Abstractions;
using MineOps.Planning.Application.Common;

namespace MineOps.Planning.Application.Activities.Queries;

public sealed class GetPendingActivitiesQueryHandler(IActivityRepository repository)
    : IRequestHandler<GetPendingActivitiesQuery, PagedResult<ActivityDto>>
{
    public async Task<PagedResult<ActivityDto>> Handle(
        GetPendingActivitiesQuery request,
        CancellationToken cancellationToken)
    {
        Pagination.Validate(request.Page, request.PageSize);
        var page = await repository.GetPendingAsync(request.Page, request.PageSize, cancellationToken);
        return ActivityDto.FromDomainPage(page);
    }
}

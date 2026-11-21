using MediatR;
using MineOps.Planning.Application.Abstractions;
using MineOps.Planning.Application.Common;

namespace MineOps.Planning.Application.Activities.Queries;

public sealed class GetActivitiesQueryHandler(IActivityRepository repository)
    : IRequestHandler<GetActivitiesQuery, PagedResult<ActivityDto>>
{
    public async Task<PagedResult<ActivityDto>> Handle(
        GetActivitiesQuery request,
        CancellationToken cancellationToken)
    {
        Pagination.Validate(request.Page, request.PageSize);
        var page = await repository.GetPagedAsync(request.Page, request.PageSize, cancellationToken);
        return ActivityDto.FromDomainPage(page);
    }
}

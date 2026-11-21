using MediatR;
using MineOps.Planning.Application.Common;

namespace MineOps.Planning.Application.Activities.Queries;

public sealed record GetOverdueActivitiesQuery(int Page = 1, int PageSize = 20)
    : IRequest<PagedResult<ActivityDto>>;

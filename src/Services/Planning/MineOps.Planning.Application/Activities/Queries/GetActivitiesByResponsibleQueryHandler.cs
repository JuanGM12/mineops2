using MediatR;
using MineOps.Planning.Application.Abstractions;
using MineOps.Planning.Application.Common;
using MineOps.Planning.Application.Exceptions;

namespace MineOps.Planning.Application.Activities.Queries;

public sealed class GetActivitiesByResponsibleQueryHandler(IActivityRepository repository)
    : IRequestHandler<GetActivitiesByResponsibleQuery, PagedResult<ActivityDto>>
{
    public async Task<PagedResult<ActivityDto>> Handle(
        GetActivitiesByResponsibleQuery request,
        CancellationToken cancellationToken)
    {
        Pagination.Validate(request.Page, request.PageSize);

        if (request.ResponsibleId == Guid.Empty)
        {
            throw new RequestValidationException("Responsible id cannot be an empty GUID.");
        }

        var page = await repository.GetByResponsibleAsync(
            request.ResponsibleId,
            request.Page,
            request.PageSize,
            cancellationToken);
        return ActivityDto.FromDomainPage(page);
    }
}

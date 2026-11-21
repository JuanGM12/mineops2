using MediatR;
using MineOps.Maintenance.Application.Abstractions;
using MineOps.Maintenance.Application.Common;
using MineOps.Maintenance.Application.Exceptions;

namespace MineOps.Maintenance.Application.WorkOrders.Queries;

public sealed class GetWorkOrderByIdQueryHandler(IWorkOrderRepository repository)
    : IRequestHandler<GetWorkOrderByIdQuery, WorkOrderDto>
{
    public async Task<WorkOrderDto> Handle(GetWorkOrderByIdQuery request, CancellationToken cancellationToken) =>
        WorkOrderDto.FromDomain(await repository.GetByIdAsync(request.Id, cancellationToken)
            ?? throw new EntityNotFoundException($"Work order '{request.Id}' was not found."));
}

public sealed class GetWorkOrdersQueryHandler(IWorkOrderRepository repository)
    : IRequestHandler<GetWorkOrdersQuery, PagedResult<WorkOrderDto>>
{
    public async Task<PagedResult<WorkOrderDto>> Handle(GetWorkOrdersQuery request, CancellationToken cancellationToken)
    {
        if (request.Page < 1 || request.PageSize is < 1 or > 100)
            throw new RequestValidationException("Page must be at least 1 and page size must be between 1 and 100.");
        return WorkOrderDto.FromPage(await repository.GetPagedAsync(request.Page, request.PageSize, cancellationToken));
    }
}

using MediatR;
using MineOps.Maintenance.Application.Common;

namespace MineOps.Maintenance.Application.WorkOrders.Queries;

public sealed record GetWorkOrderByIdQuery(Guid Id) : IRequest<WorkOrderDto>;
public sealed record GetWorkOrdersQuery(int Page = 1, int PageSize = 20) : IRequest<PagedResult<WorkOrderDto>>;

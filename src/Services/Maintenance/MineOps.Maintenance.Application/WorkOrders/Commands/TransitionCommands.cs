using MediatR;

namespace MineOps.Maintenance.Application.WorkOrders.Commands;

public sealed record StartWorkOrderCommand(Guid Id) : IRequest<WorkOrderDto>;
public sealed record CompleteWorkOrderCommand(Guid Id) : IRequest<WorkOrderDto>;
public sealed record CancelWorkOrderCommand(Guid Id) : IRequest<WorkOrderDto>;

using MediatR;
using MineOps.Maintenance.Domain.WorkOrders;

namespace MineOps.Maintenance.Application.WorkOrders.Commands;

public sealed record CreateWorkOrderCommand(
    string Code, Guid EquipmentId, string Title, string? Description,
    MaintenanceType MaintenanceType, WorkOrderPriority Priority, DateTimeOffset? ScheduledDate)
    : IRequest<WorkOrderDto>;

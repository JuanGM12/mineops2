using MineOps.Maintenance.Domain.WorkOrders;

namespace MineOps.Maintenance.Api.Contracts;

public sealed record CreateWorkOrderRequest(string Code, Guid EquipmentId, string Title, string? Description,
    MaintenanceType MaintenanceType, WorkOrderPriority Priority, DateTimeOffset? ScheduledDate);

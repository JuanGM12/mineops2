using MineOps.Maintenance.Application.Common;
using MineOps.Maintenance.Domain.WorkOrders;

namespace MineOps.Maintenance.Application.WorkOrders;

public sealed record WorkOrderDto(
    Guid Id, string Code, Guid EquipmentId, string Title, string? Description,
    MaintenanceType MaintenanceType, WorkOrderPriority Priority, WorkOrderStatus Status,
    DateTimeOffset? ScheduledDate, DateTimeOffset? StartedAt, DateTimeOffset? CompletedAt,
    DateTimeOffset CreatedAt, DateTimeOffset UpdatedAt)
{
    public static WorkOrderDto FromDomain(WorkOrder order) => new(
        order.Id, order.Code, order.EquipmentId, order.Title, order.Description,
        order.MaintenanceType, order.Priority, order.Status, order.ScheduledDate,
        order.StartedAt, order.CompletedAt, order.CreatedAt, order.UpdatedAt);

    public static PagedResult<WorkOrderDto> FromPage(PagedResult<WorkOrder> page) =>
        new(page.Items.Select(FromDomain).ToArray(), page.Page, page.PageSize, page.TotalCount);
}

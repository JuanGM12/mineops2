using MineOps.Maintenance.Domain.Exceptions;

namespace MineOps.Maintenance.Domain.WorkOrders;

public sealed class WorkOrder
{
    private WorkOrder() { }

    public WorkOrder(
        string code,
        Guid equipmentId,
        string title,
        string? description,
        MaintenanceType maintenanceType,
        WorkOrderPriority priority,
        DateTimeOffset? scheduledDate)
    {
        Code = Required(code, nameof(code));
        if (equipmentId == Guid.Empty) throw new DomainRuleException("Equipment id is required.");
        EquipmentId = equipmentId;
        Title = Required(title, nameof(title));
        Description = NormalizeOptional(description);
        MaintenanceType = maintenanceType;
        Priority = priority;
        SetSchedule(scheduledDate, maintenanceType);
        Id = Guid.NewGuid();
        Status = WorkOrderStatus.Pending;
        CreatedAt = DateTimeOffset.UtcNow;
        UpdatedAt = CreatedAt;
    }

    public Guid Id { get; private set; }
    public string Code { get; private set; } = string.Empty;
    public Guid EquipmentId { get; private set; }
    public string Title { get; private set; } = string.Empty;
    public string? Description { get; private set; }
    public MaintenanceType MaintenanceType { get; private set; }
    public WorkOrderPriority Priority { get; private set; }
    public WorkOrderStatus Status { get; private set; }
    public DateTimeOffset? ScheduledDate { get; private set; }
    public DateTimeOffset? StartedAt { get; private set; }
    public DateTimeOffset? CompletedAt { get; private set; }
    public DateTimeOffset CreatedAt { get; private set; }
    public DateTimeOffset UpdatedAt { get; private set; }

    public void Start(DateTimeOffset startedAt)
    {
        if (Status is not WorkOrderStatus.Pending)
            throw new DomainConflictException($"Only pending work orders can be started. Current status: {Status}.");
        Status = WorkOrderStatus.InProgress;
        StartedAt = startedAt.ToUniversalTime();
        Touch();
    }

    public void Complete(DateTimeOffset completedAt)
    {
        if (Status is WorkOrderStatus.Cancelled)
            throw new DomainConflictException("A cancelled work order cannot be completed.");
        if (Status is not WorkOrderStatus.InProgress)
            throw new DomainConflictException($"Only in-progress work orders can be completed. Current status: {Status}.");
        var utc = completedAt.ToUniversalTime();
        if (StartedAt.HasValue && utc < StartedAt.Value)
            throw new DomainRuleException("Completed date cannot be earlier than started date.");
        Status = WorkOrderStatus.Completed;
        CompletedAt = utc;
        Touch();
    }

    public void Cancel()
    {
        if (Status is WorkOrderStatus.Completed or WorkOrderStatus.Cancelled)
            throw new DomainConflictException($"A work order in status {Status} cannot be cancelled.");
        Status = WorkOrderStatus.Cancelled;
        Touch();
    }

    public void Reschedule(DateTimeOffset? scheduledDate)
    {
        EnsureMutable();
        SetSchedule(scheduledDate, MaintenanceType);
        Touch();
    }

    public void UpdateDetails(string title, string? description, MaintenanceType type, WorkOrderPriority priority)
    {
        EnsureMutable();
        if (type is MaintenanceType.Preventive && ScheduledDate is null)
            throw new DomainRuleException("Preventive maintenance requires a scheduled date.");
        Title = Required(title, nameof(title));
        Description = NormalizeOptional(description);
        MaintenanceType = type;
        Priority = priority;
        Touch();
    }

    private void SetSchedule(DateTimeOffset? value, MaintenanceType type)
    {
        if (type is MaintenanceType.Preventive && value is null)
            throw new DomainRuleException("Preventive maintenance requires a scheduled date.");
        ScheduledDate = value?.ToUniversalTime();
    }

    private void EnsureMutable()
    {
        if (Status is WorkOrderStatus.Completed or WorkOrderStatus.Cancelled)
            throw new DomainConflictException($"A work order in status {Status} cannot be modified.");
    }

    private static string Required(string value, string field)
    {
        if (string.IsNullOrWhiteSpace(value)) throw new DomainRuleException($"{field} is required.");
        return value.Trim();
    }

    private static string? NormalizeOptional(string? value) =>
        string.IsNullOrWhiteSpace(value) ? null : value.Trim();

    private void Touch() => UpdatedAt = DateTimeOffset.UtcNow;
}

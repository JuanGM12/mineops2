using MineOps.Maintenance.Domain.Exceptions;
using MineOps.Maintenance.Domain.WorkOrders;

namespace MineOps.Maintenance.UnitTests.Domain;

public sealed class WorkOrderTests
{
    [Fact]
    public void PreventiveRequiresSchedule() =>
        Assert.Throws<DomainRuleException>(() => Create(MaintenanceType.Preventive, null));

    [Fact]
    public void LifecycleSetsUtcTimestamps()
    {
        var order = Create();
        order.Start(DateTimeOffset.Now);
        order.Complete(DateTimeOffset.Now.AddHours(1));
        Assert.Equal(WorkOrderStatus.Completed, order.Status);
        Assert.Equal(TimeSpan.Zero, order.StartedAt?.Offset);
        Assert.Equal(TimeSpan.Zero, order.CompletedAt?.Offset);
    }

    [Fact]
    public void CompletedCannotStartAgain()
    {
        var order = Create(); order.Start(DateTimeOffset.UtcNow); order.Complete(DateTimeOffset.UtcNow.AddHours(1));
        Assert.Throws<DomainConflictException>(() => order.Start(DateTimeOffset.UtcNow));
    }

    [Fact]
    public void CancelledCannotComplete()
    {
        var order = Create(); order.Cancel();
        Assert.Throws<DomainConflictException>(() => order.Complete(DateTimeOffset.UtcNow));
    }

    [Fact]
    public void EmptyEquipmentIsRejected() =>
        Assert.Throws<DomainRuleException>(() => new WorkOrder("WO", Guid.Empty, "Title", null, MaintenanceType.Corrective, WorkOrderPriority.Low, null));

    private static WorkOrder Create(MaintenanceType type = MaintenanceType.Corrective, DateTimeOffset? schedule = null) =>
        new("WO-001", Guid.NewGuid(), "Repair", null, type, WorkOrderPriority.High,
            type is MaintenanceType.Preventive ? schedule : null);
}

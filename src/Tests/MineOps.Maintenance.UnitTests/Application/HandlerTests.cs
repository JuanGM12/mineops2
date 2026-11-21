using MineOps.Maintenance.Application.Abstractions;
using MineOps.Maintenance.Application.Common;
using MineOps.Maintenance.Application.Exceptions;
using MineOps.Maintenance.Application.WorkOrders.Commands;
using MineOps.Maintenance.Application.WorkOrders.Queries;
using EquipmentEntity = MineOps.Maintenance.Domain.Equipment.Equipment;
using MineOps.Maintenance.Domain.WorkOrders;

namespace MineOps.Maintenance.UnitTests.Application;

public sealed class HandlerTests
{
    [Fact]
    public async Task CreateRequiresExistingActiveEquipment()
    {
        var equipment = new EquipmentEntity("EXC", "Excavator"); equipment.Deactivate();
        var handler = new CreateWorkOrderCommandHandler(new FakeOrders(), new FakeEquipment(equipment), new FakeUnit());
        await Assert.ThrowsAsync<ApplicationConflictException>(() => handler.Handle(Command(equipment.Id), default));
    }

    [Fact]
    public async Task CreatePersistsOrderAndTransitionsWork()
    {
        var equipment = new EquipmentEntity("EXC", "Excavator"); var orders = new FakeOrders(); var unit = new FakeUnit();
        var created = await new CreateWorkOrderCommandHandler(orders, new FakeEquipment(equipment), unit).Handle(Command(equipment.Id), default);
        await new StartWorkOrderCommandHandler(orders, unit).Handle(new(created.Id), default);
        var complete = await new CompleteWorkOrderCommandHandler(orders, unit).Handle(new(created.Id), default);
        Assert.Equal(WorkOrderStatus.Completed, complete.Status); Assert.Equal(3, unit.Transactions);
    }

    [Fact]
    public async Task QueryValidatesPagination()
    {
        await Assert.ThrowsAsync<RequestValidationException>(() =>
            new GetWorkOrdersQueryHandler(new FakeOrders()).Handle(new(1, 101), default));
    }

    private static CreateWorkOrderCommand Command(Guid equipmentId) =>
        new("WO-1", equipmentId, "Repair", null, MaintenanceType.Corrective, WorkOrderPriority.High, null);

    private sealed class FakeEquipment(EquipmentEntity? value) : IEquipmentRepository
    { public Task<EquipmentEntity?> GetByIdAsync(Guid id, CancellationToken token) => Task.FromResult(value); }
    private sealed class FakeUnit : IUnitOfWork
    {
        public int Transactions { get; private set; }
        public Task<int> SaveChangesAsync(CancellationToken token) => Task.FromResult(1);
        public async Task ExecuteInTransactionAsync(Func<CancellationToken, Task> op, CancellationToken token)
        { Transactions++; await op(token); }
    }
    private sealed class FakeOrders : IWorkOrderRepository
    {
        private readonly List<WorkOrder> _items = [];
        public Task AddAsync(WorkOrder o, CancellationToken t) { _items.Add(o); return Task.CompletedTask; }
        public Task<bool> CodeExistsAsync(string c, CancellationToken t) => Task.FromResult(_items.Any(x => x.Code == c));
        public Task<WorkOrder?> GetByIdAsync(Guid id, CancellationToken t) => Task.FromResult(_items.SingleOrDefault(x => x.Id == id));
        public Task<PagedResult<WorkOrder>> GetPagedAsync(int p, int s, CancellationToken t) => Task.FromResult(new PagedResult<WorkOrder>(_items, p, s, _items.Count));
    }
}

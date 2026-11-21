using Microsoft.EntityFrameworkCore;
using MineOps.Maintenance.Application.Abstractions;
using MineOps.Maintenance.Application.Common;
using EquipmentEntity = MineOps.Maintenance.Domain.Equipment.Equipment;
using MineOps.Maintenance.Domain.WorkOrders;

namespace MineOps.Maintenance.Infrastructure.Persistence.Repositories;

public sealed class EfEquipmentRepository(MaintenanceDbContext db) : IEquipmentRepository
{
    public Task<EquipmentEntity?> GetByIdAsync(Guid id, CancellationToken token) =>
        db.Equipment.SingleOrDefaultAsync(x => x.Id == id, token);
}

public sealed class EfWorkOrderRepository(MaintenanceDbContext db) : IWorkOrderRepository
{
    public Task<WorkOrder?> GetByIdAsync(Guid id, CancellationToken token) =>
        db.WorkOrders.SingleOrDefaultAsync(x => x.Id == id, token);
    public Task<bool> CodeExistsAsync(string code, CancellationToken token) =>
        db.WorkOrders.AnyAsync(x => x.Code == code, token);
    public async Task AddAsync(WorkOrder order, CancellationToken token) => await db.WorkOrders.AddAsync(order, token);
    public async Task<PagedResult<WorkOrder>> GetPagedAsync(int page, int size, CancellationToken token)
    {
        var query = db.WorkOrders.AsNoTracking();
        var count = await query.CountAsync(token);
        var items = await query.OrderByDescending(x => x.CreatedAt).ThenBy(x => x.Id)
            .Skip((page - 1) * size).Take(size).ToListAsync(token);
        return new(items, page, size, count);
    }
}

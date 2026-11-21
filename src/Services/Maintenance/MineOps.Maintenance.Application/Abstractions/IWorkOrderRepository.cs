using MineOps.Maintenance.Application.Common;
using MineOps.Maintenance.Domain.WorkOrders;

namespace MineOps.Maintenance.Application.Abstractions;

public interface IWorkOrderRepository
{
    Task<WorkOrder?> GetByIdAsync(Guid id, CancellationToken cancellationToken);
    Task<bool> CodeExistsAsync(string code, CancellationToken cancellationToken);
    Task AddAsync(WorkOrder order, CancellationToken cancellationToken);
    Task<PagedResult<WorkOrder>> GetPagedAsync(int page, int pageSize, CancellationToken cancellationToken);
}

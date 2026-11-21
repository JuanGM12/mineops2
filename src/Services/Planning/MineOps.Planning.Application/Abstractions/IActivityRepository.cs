using MineOps.Planning.Application.Common;
using MineOps.Planning.Domain.Activities;

namespace MineOps.Planning.Application.Abstractions;

public interface IActivityRepository
{
    Task<Activity?> GetByIdAsync(Guid id, CancellationToken cancellationToken);

    Task<bool> CodeExistsAsync(string code, Guid? excludingId, CancellationToken cancellationToken);

    Task AddAsync(Activity activity, CancellationToken cancellationToken);

    Task<PagedResult<Activity>> GetPagedAsync(int page, int pageSize, CancellationToken cancellationToken);

    Task<PagedResult<Activity>> GetPendingAsync(int page, int pageSize, CancellationToken cancellationToken);

    Task<PagedResult<Activity>> GetOverdueAsync(
        DateTimeOffset now,
        int page,
        int pageSize,
        CancellationToken cancellationToken);

    Task<PagedResult<Activity>> GetByResponsibleAsync(
        Guid responsibleId,
        int page,
        int pageSize,
        CancellationToken cancellationToken);
}

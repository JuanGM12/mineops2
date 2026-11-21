using Microsoft.EntityFrameworkCore;
using MineOps.Planning.Application.Abstractions;
using MineOps.Planning.Application.Common;
using MineOps.Planning.Domain.Activities;

namespace MineOps.Planning.Infrastructure.Persistence.Repositories;

public sealed class EfActivityRepository(PlanningDbContext dbContext) : IActivityRepository
{
    public Task<Activity?> GetByIdAsync(Guid id, CancellationToken cancellationToken) =>
        dbContext.Activities.SingleOrDefaultAsync(activity => activity.Id == id, cancellationToken);

    public Task<bool> CodeExistsAsync(
        string code,
        Guid? excludingId,
        CancellationToken cancellationToken) =>
        dbContext.Activities.AnyAsync(
            activity => activity.Code == code &&
                        (!excludingId.HasValue || activity.Id != excludingId.Value),
            cancellationToken);

    public async Task AddAsync(Activity activity, CancellationToken cancellationToken)
    {
        await dbContext.Activities.AddAsync(activity, cancellationToken);
    }

    public Task<PagedResult<Activity>> GetPagedAsync(
        int page,
        int pageSize,
        CancellationToken cancellationToken) =>
        ToPageAsync(dbContext.Activities.AsNoTracking(), page, pageSize, cancellationToken);

    public Task<PagedResult<Activity>> GetPendingAsync(
        int page,
        int pageSize,
        CancellationToken cancellationToken) =>
        ToPageAsync(
            dbContext.Activities.AsNoTracking()
                .Where(activity => activity.Status == ActivityStatus.Pending),
            page,
            pageSize,
            cancellationToken);

    public Task<PagedResult<Activity>> GetOverdueAsync(
        DateTimeOffset now,
        int page,
        int pageSize,
        CancellationToken cancellationToken) =>
        ToPageAsync(
            dbContext.Activities.AsNoTracking()
                .Where(activity =>
                    activity.DueDate.HasValue &&
                    activity.DueDate.Value < now &&
                    activity.Status != ActivityStatus.Completed &&
                    activity.Status != ActivityStatus.Cancelled),
            page,
            pageSize,
            cancellationToken);

    public Task<PagedResult<Activity>> GetByResponsibleAsync(
        Guid responsibleId,
        int page,
        int pageSize,
        CancellationToken cancellationToken) =>
        ToPageAsync(
            dbContext.Activities.AsNoTracking()
                .Where(activity => activity.ResponsibleId == responsibleId),
            page,
            pageSize,
            cancellationToken);

    private static async Task<PagedResult<Activity>> ToPageAsync(
        IQueryable<Activity> query,
        int page,
        int pageSize,
        CancellationToken cancellationToken)
    {
        var totalCount = await query.CountAsync(cancellationToken);
        var items = await query
            .OrderByDescending(activity => activity.CreatedAt)
            .ThenBy(activity => activity.Id)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return new PagedResult<Activity>(items, page, pageSize, totalCount);
    }
}

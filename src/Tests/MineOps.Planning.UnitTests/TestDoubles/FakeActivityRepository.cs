using MineOps.Planning.Application.Abstractions;
using MineOps.Planning.Application.Common;
using MineOps.Planning.Domain.Activities;

namespace MineOps.Planning.UnitTests.TestDoubles;

internal sealed class FakeActivityRepository : IActivityRepository
{
    private readonly List<Activity> _activities = [];

    public IReadOnlyList<Activity> Activities => _activities;

    public void Seed(params Activity[] activities) => _activities.AddRange(activities);

    public Task<Activity?> GetByIdAsync(Guid id, CancellationToken cancellationToken) =>
        Task.FromResult(_activities.SingleOrDefault(activity => activity.Id == id));

    public Task<bool> CodeExistsAsync(
        string code,
        Guid? excludingId,
        CancellationToken cancellationToken) =>
        Task.FromResult(_activities.Any(activity =>
            string.Equals(activity.Code, code, StringComparison.OrdinalIgnoreCase) &&
            (!excludingId.HasValue || activity.Id != excludingId.Value)));

    public Task AddAsync(Activity activity, CancellationToken cancellationToken)
    {
        _activities.Add(activity);
        return Task.CompletedTask;
    }

    public Task<PagedResult<Activity>> GetPagedAsync(
        int page,
        int pageSize,
        CancellationToken cancellationToken) =>
        Task.FromResult(ToPage(_activities, page, pageSize));

    public Task<PagedResult<Activity>> GetPendingAsync(
        int page,
        int pageSize,
        CancellationToken cancellationToken) =>
        Task.FromResult(ToPage(
            _activities.Where(activity => activity.Status == ActivityStatus.Pending),
            page,
            pageSize));

    public Task<PagedResult<Activity>> GetOverdueAsync(
        DateTimeOffset now,
        int page,
        int pageSize,
        CancellationToken cancellationToken) =>
        Task.FromResult(ToPage(
            _activities.Where(activity =>
                activity.DueDate < now &&
                activity.Status is not ActivityStatus.Completed and not ActivityStatus.Cancelled),
            page,
            pageSize));

    public Task<PagedResult<Activity>> GetByResponsibleAsync(
        Guid responsibleId,
        int page,
        int pageSize,
        CancellationToken cancellationToken) =>
        Task.FromResult(ToPage(
            _activities.Where(activity => activity.ResponsibleId == responsibleId),
            page,
            pageSize));

    private static PagedResult<Activity> ToPage(
        IEnumerable<Activity> source,
        int page,
        int pageSize)
    {
        var items = source.ToArray();
        return new PagedResult<Activity>(
            items.Skip((page - 1) * pageSize).Take(pageSize).ToArray(),
            page,
            pageSize,
            items.Length);
    }
}

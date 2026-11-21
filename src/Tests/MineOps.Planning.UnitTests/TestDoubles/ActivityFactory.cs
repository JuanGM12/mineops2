using MineOps.Planning.Domain.Activities;

namespace MineOps.Planning.UnitTests.TestDoubles;

internal static class ActivityFactory
{
    public static Activity Create(
        string code = "ACT-001",
        Guid? responsibleId = null,
        DateTimeOffset? scheduledDate = null,
        DateTimeOffset? dueDate = null) => new(
            code,
            "Inspect pit",
            "Description",
            "Operations",
            "North pit",
            responsibleId,
            ActivityPriority.High,
            scheduledDate ?? DateTimeOffset.UtcNow.AddDays(1),
            dueDate ?? DateTimeOffset.UtcNow.AddDays(2));
}

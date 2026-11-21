namespace MineOps.Identity.Infrastructure.Identity;

public static class IdentitySeedData
{
    public const string AdministratorRole = "Administrator";
    public const string PlannerRole = "Planner";
    public const string MaintenanceSupervisorRole = "MaintenanceSupervisor";
    public const string OperatorRole = "Operator";

    public static readonly IReadOnlyList<string> Roles =
    [
        AdministratorRole,
        PlannerRole,
        MaintenanceSupervisorRole,
        OperatorRole
    ];

    public static readonly IReadOnlyList<SeedUser> Users =
    [
        new("admin@mineops.local", "MineOps Administrator", AdministratorRole),
        new("planner@mineops.local", "MineOps Planner", PlannerRole),
        new("maintenance@mineops.local", "Maintenance Supervisor", MaintenanceSupervisorRole),
        new("operator@mineops.local", "MineOps Operator", OperatorRole)
    ];
}

public sealed record SeedUser(string Email, string Name, string Role);

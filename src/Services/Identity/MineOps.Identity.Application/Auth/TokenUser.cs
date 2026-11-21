namespace MineOps.Identity.Application.Auth;

public sealed record TokenUser(
    Guid Id,
    string Name,
    string Email,
    IReadOnlyCollection<string> Roles);

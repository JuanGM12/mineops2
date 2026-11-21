namespace MineOps.Identity.Application.Auth;

public sealed record LoginResponse(
    string AccessToken,
    DateTimeOffset ExpiresAt,
    string TokenType = "Bearer");

namespace MineOps.Identity.Api.Errors;

public sealed record ErrorResponse(bool Success, string Message, IReadOnlyList<string> Errors);

namespace MineOps.Planning.Api.Errors;

public sealed record ErrorResponse(bool Success, string Message, IReadOnlyList<string> Errors);

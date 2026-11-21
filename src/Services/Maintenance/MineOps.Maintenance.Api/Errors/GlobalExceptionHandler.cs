using Microsoft.AspNetCore.Diagnostics;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using MineOps.Maintenance.Application.Exceptions;
using MineOps.Maintenance.Domain.Exceptions;

namespace MineOps.Maintenance.Api.Errors;

public sealed record ErrorResponse(bool Success, string Message, IReadOnlyList<string> Errors);

public sealed class GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger) : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(HttpContext context, Exception exception, CancellationToken token)
    {
        var result = exception switch
        {
            RequestValidationException e => (400, e.Message, e.Errors),
            EntityNotFoundException e => (404, e.Message, (IReadOnlyList<string>)[]),
            ApplicationConflictException e => (409, e.Message, (IReadOnlyList<string>)[]),
            DomainConflictException e => (409, e.Message, (IReadOnlyList<string>)[]),
            DbUpdateException { InnerException: SqlException { Number: 2601 or 2627 } } =>
                (409, "A record with the same unique value already exists.", (IReadOnlyList<string>)[]),
            DomainRuleException e => (422, e.Message, (IReadOnlyList<string>)[]),
            _ => (500, "An unexpected error occurred.", (IReadOnlyList<string>)[])
        };
        if (result.Item1 == 500) logger.LogError(exception, "Unhandled Maintenance error.");
        else logger.LogWarning(exception, "Maintenance request failed with {StatusCode}.", result.Item1);
        context.Response.StatusCode = result.Item1;
        await context.Response.WriteAsJsonAsync(new ErrorResponse(false, result.Item2, result.Item3), token);
        return true;
    }
}

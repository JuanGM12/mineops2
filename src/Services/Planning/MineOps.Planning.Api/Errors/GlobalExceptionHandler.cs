using Microsoft.AspNetCore.Diagnostics;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using MineOps.Planning.Application.Exceptions;
using MineOps.Planning.Domain.Exceptions;

namespace MineOps.Planning.Api.Errors;

public sealed class GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger) : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        var (statusCode, message, errors) = MapException(exception);

        if (statusCode >= StatusCodes.Status500InternalServerError)
        {
            logger.LogError(exception, "An unhandled error occurred while processing the request.");
        }
        else
        {
            logger.LogWarning(exception, "Request failed with status code {StatusCode}.", statusCode);
        }

        httpContext.Response.StatusCode = statusCode;
        await httpContext.Response.WriteAsJsonAsync(
            new ErrorResponse(false, message, errors),
            cancellationToken);
        return true;
    }

    private static (int StatusCode, string Message, IReadOnlyList<string> Errors) MapException(
        Exception exception) => exception switch
    {
        RequestValidationException validation => (
            StatusCodes.Status400BadRequest,
            validation.Message,
            validation.Errors),
        EntityNotFoundException notFound => (
            StatusCodes.Status404NotFound,
            notFound.Message,
            []),
        ApplicationConflictException conflict => (
            StatusCodes.Status409Conflict,
            conflict.Message,
            []),
        DomainConflictException conflict => (
            StatusCodes.Status409Conflict,
            conflict.Message,
            []),
        DbUpdateException { InnerException: SqlException { Number: 2601 or 2627 } } => (
            StatusCodes.Status409Conflict,
            "A record with the same unique value already exists.",
            []),
        DomainRuleException domainRule => (
            StatusCodes.Status422UnprocessableEntity,
            domainRule.Message,
            []),
        _ => (
            StatusCodes.Status500InternalServerError,
            "An unexpected error occurred.",
            [])
    };
}

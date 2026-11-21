namespace MineOps.Maintenance.Application.Exceptions;

public sealed class RequestValidationException(string message, IReadOnlyList<string>? errors = null) : Exception(message)
{
    public IReadOnlyList<string> Errors { get; } = errors ?? [message];
}

public sealed class EntityNotFoundException(string message) : Exception(message);

public sealed class ApplicationConflictException(string message) : Exception(message);

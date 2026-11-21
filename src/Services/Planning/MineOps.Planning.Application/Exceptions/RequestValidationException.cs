namespace MineOps.Planning.Application.Exceptions;

public sealed class RequestValidationException : Exception
{
    public RequestValidationException(string message)
        : this(message, [message])
    {
    }

    public RequestValidationException(string message, IReadOnlyList<string> errors)
        : base(message)
    {
        Errors = errors;
    }

    public IReadOnlyList<string> Errors { get; }
}

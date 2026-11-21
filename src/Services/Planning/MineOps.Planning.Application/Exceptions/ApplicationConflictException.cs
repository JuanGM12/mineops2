namespace MineOps.Planning.Application.Exceptions;

public sealed class ApplicationConflictException(string message) : Exception(message);

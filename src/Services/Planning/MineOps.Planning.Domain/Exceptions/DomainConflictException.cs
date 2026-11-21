namespace MineOps.Planning.Domain.Exceptions;

public sealed class DomainConflictException(string message) : Exception(message);

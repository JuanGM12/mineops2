namespace MineOps.Maintenance.Domain.Exceptions;

public sealed class DomainConflictException(string message) : Exception(message);

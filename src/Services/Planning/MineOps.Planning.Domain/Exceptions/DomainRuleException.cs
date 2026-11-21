namespace MineOps.Planning.Domain.Exceptions;

public sealed class DomainRuleException(string message) : Exception(message);

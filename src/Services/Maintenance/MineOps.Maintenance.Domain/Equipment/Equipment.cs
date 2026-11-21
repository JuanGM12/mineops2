using MineOps.Maintenance.Domain.Exceptions;

namespace MineOps.Maintenance.Domain.Equipment;

public sealed class Equipment
{
    private Equipment() { }

    public Equipment(string code, string name)
    {
        Code = Required(code, nameof(code));
        Name = Required(name, nameof(name));
        Id = Guid.NewGuid();
        IsActive = true;
        CreatedAt = DateTimeOffset.UtcNow;
    }

    public Guid Id { get; private set; }
    public string Code { get; private set; } = string.Empty;
    public string Name { get; private set; } = string.Empty;
    public bool IsActive { get; private set; }
    public DateTimeOffset CreatedAt { get; private set; }

    public void Deactivate() => IsActive = false;

    private static string Required(string value, string field)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new DomainRuleException($"{field} is required.");
        }

        return value.Trim();
    }
}

using MineOps.Maintenance.Domain.Equipment;
using MineOps.Maintenance.Domain.Exceptions;

namespace MineOps.Maintenance.UnitTests.Domain;

public sealed class EquipmentTests
{
    [Fact]
    public void ConstructorCreatesActiveEquipmentWithUtcDate()
    {
        var equipment = new Equipment("EXC-001", "Excavadora 001");
        Assert.True(equipment.IsActive);
        Assert.Equal(TimeSpan.Zero, equipment.CreatedAt.Offset);
    }

    [Theory]
    [InlineData("", "Name")]
    [InlineData("CODE", " ")]
    public void ConstructorRejectsRequiredValues(string code, string name) =>
        Assert.Throws<DomainRuleException>(() => new Equipment(code, name));
}

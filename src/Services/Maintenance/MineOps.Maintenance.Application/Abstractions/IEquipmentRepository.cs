using EquipmentEntity = MineOps.Maintenance.Domain.Equipment.Equipment;

namespace MineOps.Maintenance.Application.Abstractions;

public interface IEquipmentRepository
{
    Task<EquipmentEntity?> GetByIdAsync(Guid id, CancellationToken cancellationToken);
}

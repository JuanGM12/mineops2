using Microsoft.EntityFrameworkCore;
using EquipmentEntity = MineOps.Maintenance.Domain.Equipment.Equipment;

namespace MineOps.Maintenance.Infrastructure.Persistence;

public sealed class MaintenanceSeeder(MaintenanceDbContext db)
{
    private static readonly (string Code, string Name)[] EquipmentSeeds =
        [("EXC-001", "Excavadora 001"), ("TRK-001", "Camión 001")];

    public async Task SeedAsync(CancellationToken token = default)
    {
        foreach (var seed in EquipmentSeeds)
        {
            if (!await db.Equipment.AnyAsync(x => x.Code == seed.Code, token))
                await db.Equipment.AddAsync(new EquipmentEntity(seed.Code, seed.Name), token);
        }
        await db.SaveChangesAsync(token);
    }
}

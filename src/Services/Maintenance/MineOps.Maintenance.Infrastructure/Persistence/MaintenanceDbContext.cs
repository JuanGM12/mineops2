using Microsoft.EntityFrameworkCore;
using EquipmentEntity = MineOps.Maintenance.Domain.Equipment.Equipment;
using MineOps.Maintenance.Domain.WorkOrders;

namespace MineOps.Maintenance.Infrastructure.Persistence;

public sealed class MaintenanceDbContext(DbContextOptions<MaintenanceDbContext> options) : DbContext(options)
{
    public DbSet<EquipmentEntity> Equipment => Set<EquipmentEntity>();
    public DbSet<WorkOrder> WorkOrders => Set<WorkOrder>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(MaintenanceDbContext).Assembly);
        base.OnModelCreating(modelBuilder);
    }
}

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using EquipmentEntity = MineOps.Maintenance.Domain.Equipment.Equipment;
using MineOps.Maintenance.Domain.WorkOrders;

namespace MineOps.Maintenance.Infrastructure.Persistence.Configurations;

public sealed class EquipmentConfiguration : IEntityTypeConfiguration<EquipmentEntity>
{
    public void Configure(EntityTypeBuilder<EquipmentEntity> builder)
    {
        builder.ToTable("Equipment"); builder.HasKey(x => x.Id);
        builder.Property(x => x.Code).HasMaxLength(50).IsRequired();
        builder.HasIndex(x => x.Code).IsUnique().HasDatabaseName("UX_Equipment_Code");
        builder.Property(x => x.Name).HasMaxLength(150).IsRequired();
        builder.Property(x => x.CreatedAt).HasColumnType("datetimeoffset");
    }
}

public sealed class WorkOrderConfiguration : IEntityTypeConfiguration<WorkOrder>
{
    public void Configure(EntityTypeBuilder<WorkOrder> builder)
    {
        builder.ToTable("WorkOrders"); builder.HasKey(x => x.Id);
        builder.Property(x => x.Code).HasMaxLength(50).IsRequired();
        builder.HasIndex(x => x.Code).IsUnique().HasDatabaseName("UX_WorkOrders_Code");
        builder.Property(x => x.Title).HasMaxLength(200).IsRequired();
        builder.Property(x => x.Description).HasMaxLength(2000);
        builder.Property(x => x.MaintenanceType).HasConversion<string>().HasMaxLength(20);
        builder.Property(x => x.Priority).HasConversion<string>().HasMaxLength(20);
        builder.Property(x => x.Status).HasConversion<string>().HasMaxLength(20);
        builder.Property(x => x.ScheduledDate).HasColumnType("datetimeoffset");
        builder.Property(x => x.StartedAt).HasColumnType("datetimeoffset");
        builder.Property(x => x.CompletedAt).HasColumnType("datetimeoffset");
        builder.Property(x => x.CreatedAt).HasColumnType("datetimeoffset");
        builder.Property(x => x.UpdatedAt).HasColumnType("datetimeoffset");
        builder.HasIndex(x => x.Status); builder.HasIndex(x => x.EquipmentId);
        builder.HasOne<EquipmentEntity>().WithMany().HasForeignKey(x => x.EquipmentId).OnDelete(DeleteBehavior.Restrict);
    }
}

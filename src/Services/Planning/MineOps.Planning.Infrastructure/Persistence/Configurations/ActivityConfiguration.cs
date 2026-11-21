using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MineOps.Planning.Domain.Activities;

namespace MineOps.Planning.Infrastructure.Persistence.Configurations;

public sealed class ActivityConfiguration : IEntityTypeConfiguration<Activity>
{
    public void Configure(EntityTypeBuilder<Activity> builder)
    {
        builder.ToTable("Activities");
        builder.HasKey(activity => activity.Id);

        builder.Property(activity => activity.Code)
            .HasMaxLength(50)
            .IsRequired();
        builder.HasIndex(activity => activity.Code)
            .IsUnique()
            .HasDatabaseName("UX_Activities_Code");

        builder.Property(activity => activity.Title)
            .HasMaxLength(200)
            .IsRequired();
        builder.Property(activity => activity.Description)
            .HasMaxLength(2000);
        builder.Property(activity => activity.Area)
            .HasMaxLength(100)
            .IsRequired();
        builder.Property(activity => activity.Location)
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(activity => activity.Priority)
            .HasConversion<string>()
            .HasMaxLength(20)
            .IsRequired();
        builder.Property(activity => activity.Status)
            .HasConversion<string>()
            .HasMaxLength(20)
            .IsRequired();

        builder.Property(activity => activity.ScheduledDate)
            .HasColumnType("datetimeoffset")
            .IsRequired();
        builder.Property(activity => activity.DueDate)
            .HasColumnType("datetimeoffset");
        builder.Property(activity => activity.CreatedAt)
            .HasColumnType("datetimeoffset")
            .IsRequired();
        builder.Property(activity => activity.UpdatedAt)
            .HasColumnType("datetimeoffset")
            .IsRequired();

        builder.HasIndex(activity => activity.Status);
        builder.HasIndex(activity => activity.DueDate);
        builder.HasIndex(activity => activity.ResponsibleId);
    }
}

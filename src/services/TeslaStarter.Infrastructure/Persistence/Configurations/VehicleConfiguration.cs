using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TeslaStarter.Domain.Vehicles;

namespace TeslaStarter.Infrastructure.Persistence.Configurations;

public class VehicleConfiguration : IEntityTypeConfiguration<Vehicle>
{
    public void Configure(EntityTypeBuilder<Vehicle> builder)
    {
        builder.ToTable("vehicles");

        builder.HasKey(v => v.Id);

        builder.Property(v => v.Id)
            .IsRequired()
            .HasConversion(
                id => id.Value,
                value => new VehicleId(value));

        builder.Property(v => v.TeslaAccountId)
            .IsRequired()
            .HasMaxLength(100)
            .HasConversion(
                teslaAccountId => teslaAccountId.Value,
                value => TeslaAccountId.Create(value));

        builder.Property(v => v.VehicleIdentifier)
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(v => v.DisplayName)
            .HasMaxLength(100);

        builder.Property(v => v.LinkedAt)
            .IsRequired();

        builder.Property(v => v.LastSyncedAt);

        builder.Property(v => v.IsActive)
            .IsRequired();

        // Indexes
        builder.HasIndex(v => v.TeslaAccountId)
            .HasDatabaseName("ix_vehicles_tesla_account_id");

        builder.HasIndex(v => new { v.TeslaAccountId, v.VehicleIdentifier })
            .HasDatabaseName("ix_vehicles_tesla_account_id_vehicle_identifier");

        builder.HasIndex(v => v.VehicleIdentifier)
            .HasDatabaseName("ix_vehicles_vehicle_identifier");

        // Ignore domain events
        builder.Ignore(v => v.DomainEvents);
    }
}

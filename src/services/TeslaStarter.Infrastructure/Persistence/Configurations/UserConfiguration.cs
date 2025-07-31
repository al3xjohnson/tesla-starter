using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TeslaStarter.Domain.Users;

namespace TeslaStarter.Infrastructure.Persistence.Configurations;

public class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.ToTable("users");

        builder.HasKey(u => u.Id);

        builder.Property(u => u.Id)
            .IsRequired()
            .HasConversion(
                id => id.Value,
                value => new UserId(value));

        builder.Property(u => u.ExternalId)
            .IsRequired()
            .HasMaxLength(255)
            .HasConversion(
                externalId => externalId.Value,
                value => ExternalId.Create(value));

        builder.Property(u => u.Email)
            .IsRequired()
            .HasMaxLength(255)
            .HasConversion(
                email => email.Value,
                value => Email.Create(value));

        builder.Property(u => u.DisplayName)
            .HasMaxLength(100);

        builder.Property(u => u.CreatedAt)
            .IsRequired();

        builder.Property(u => u.LastLoginAt);

        // Configure TeslaAccount as owned entity
        builder.OwnsOne(u => u.TeslaAccount, ta =>
        {
            ta.Property(t => t.TeslaAccountId)
                .IsRequired(false)
                .HasMaxLength(100)
                .HasConversion(
                    teslaAccountId => teslaAccountId != null ? teslaAccountId.Value : null,
                    value => value != null ? TeslaAccountId.Create(value) : null!);

            ta.Property(t => t.LinkedAt);

            ta.Property(t => t.IsActive);

            ta.Property(t => t.RefreshToken)
                .HasMaxLength(2000); // Increased to accommodate encrypted data

            ta.Property(t => t.AccessToken)
                .HasMaxLength(2000); // Increased to accommodate encrypted data

            ta.Property(t => t.TokenExpiresAt);

            ta.Property(t => t.LastSyncedAt);
        });

        // Indexes
        builder.HasIndex(u => u.ExternalId)
            .HasDatabaseName("ix_users_external_id")
            .IsUnique();

        builder.HasIndex(u => u.Email)
            .HasDatabaseName("ix_users_email");

        // Ignore domain events
        builder.Ignore(u => u.DomainEvents);
    }
}

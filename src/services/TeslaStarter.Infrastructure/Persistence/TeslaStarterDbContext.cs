using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TeslaStarter.Application.Common.Interfaces;
using TeslaStarter.Domain.Users;
using TeslaStarter.Domain.Vehicles;

namespace TeslaStarter.Infrastructure.Persistence;

public class TeslaStarterDbContext : DbContext
{
    private readonly IEncryptionService? _encryptionService;

    public TeslaStarterDbContext(DbContextOptions<TeslaStarterDbContext> options) : base(options)
    {
    }

    public TeslaStarterDbContext(DbContextOptions<TeslaStarterDbContext> options, IEncryptionService encryptionService) : base(options)
    {
        _encryptionService = encryptionService;
    }

    public DbSet<User> Users => Set<User>();
    public DbSet<Vehicle> Vehicles => Set<Vehicle>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.ApplyConfigurationsFromAssembly(typeof(TeslaStarterDbContext).Assembly);

        // Apply encryption to Tesla tokens if encryption service is available
        if (_encryptionService != null)
        {
            EntityTypeBuilder<User> userEntity = modelBuilder.Entity<User>();
            OwnedNavigationBuilder<User, TeslaAccount> teslaAccountOwned = userEntity.OwnsOne(u => u.TeslaAccount);

            // Configure encryption for RefreshToken
            teslaAccountOwned.Property(t => t.RefreshToken)
                .HasConversion(
                    v => _encryptionService.Encrypt(v),
                    v => _encryptionService.Decrypt(v));

            // Configure encryption for AccessToken
            teslaAccountOwned.Property(t => t.AccessToken)
                .HasConversion(
                    v => _encryptionService.Encrypt(v),
                    v => _encryptionService.Decrypt(v));
        }
    }
}

using Microsoft.EntityFrameworkCore;
using TeslaStarter.Application.Common.Interfaces;
using TeslaStarter.Infrastructure.Persistence;

namespace TeslaStarter.Infrastructure.Tests.TestHelpers;

public static class TestDbContextFactory
{
    public static TeslaStarterDbContext CreateInMemoryContext()
    {
        DbContextOptions<TeslaStarterDbContext> options = new DbContextOptionsBuilder<TeslaStarterDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        return new TeslaStarterDbContext(options);
    }

    public static TeslaStarterDbContext CreateInMemoryContext(IEncryptionService encryptionService)
    {
        DbContextOptions<TeslaStarterDbContext> options = new DbContextOptionsBuilder<TeslaStarterDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        return new TeslaStarterDbContext(options, encryptionService);
    }
}

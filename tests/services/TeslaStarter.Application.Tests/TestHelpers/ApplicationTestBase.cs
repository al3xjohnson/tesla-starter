namespace TeslaStarter.Application.Tests.TestHelpers;

public abstract class ApplicationTestBase
{
    protected Mock<IUserRepository> UserRepositoryMock { get; }
    protected Mock<IVehicleRepository> VehicleRepositoryMock { get; }
    protected Mock<IUnitOfWork> UnitOfWorkMock { get; }
    protected IMapper Mapper { get; }
    protected Mock<ILogger<T>> CreateLoggerMock<T>() => new();

    protected ApplicationTestBase()
    {
        UserRepositoryMock = new Mock<IUserRepository>();
        VehicleRepositoryMock = new Mock<IVehicleRepository>();
        UnitOfWorkMock = new Mock<IUnitOfWork>();

        MapperConfiguration configuration = new MapperConfiguration(cfg =>
        {
            cfg.AddProfile<TeslaStarter.Application.Users.UserMappingProfile>();
            cfg.AddProfile<TeslaStarter.Application.Vehicles.VehicleMappingProfile>();
        });
        Mapper = configuration.CreateMapper();

        // Default setup
        UnitOfWorkMock.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);
    }

    protected static User CreateTestUser(
        string? externalId = null,
        string? email = null,
        string? displayName = "Test User")
    {
        return User.Create(
            externalId ?? "ext123",
            email ?? "test@example.com",
            displayName);
    }

    protected static Vehicle CreateTestVehicle(
        string? teslaAccountId = null,
        string? vehicleIdentifier = null,
        string? displayName = "Test Vehicle")
    {
        return Vehicle.Link(
            TeslaAccountId.Create(teslaAccountId ?? "tesla123"),
            vehicleIdentifier ?? "VIN123",
            displayName);
    }
}

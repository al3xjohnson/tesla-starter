using TeslaStarter.Application.Tests.TestHelpers;

namespace TeslaStarter.Application.Tests.Users;

public sealed class UserMappingProfileTests : ApplicationTestBase
{
    [Fact]
    public void UserMappingProfile_ConfigurationIsValid()
    {
        // Act & Assert
        Mapper.ConfigurationProvider.AssertConfigurationIsValid();
    }

    [Fact]
    public void Map_User_To_UserDto_MapsAllProperties()
    {
        // Arrange
        User user = CreateTestUser("ext123", "test@example.com", "Test User");
        user.RecordLogin();

        // Act
        UserDto dto = Mapper.Map<UserDto>(user);

        // Assert
        dto.Should().NotBeNull();
        dto.Id.Should().Be(user.Id.Value);
        dto.ExternalId.Should().Be("ext123");
        dto.Email.Should().Be("test@example.com");
        dto.DisplayName.Should().Be("Test User");
        dto.CreatedAt.Should().Be(user.CreatedAt);
        dto.LastLoginAt.Should().Be(user.LastLoginAt);
        dto.TeslaAccount.Should().BeNull();
    }

    [Fact]
    public void Map_UserWithTeslaAccount_To_UserDto_MapsNestedObject()
    {
        // Arrange
        User user = CreateTestUser();
        user.LinkTeslaAccount("tesla123");
        user.UpdateTeslaRefreshToken("refresh_token_123");

        // Force an update to LastSyncedAt by calling a method that would update it
        TeslaAccount teslaAccount = user.TeslaAccount!.UpdateRefreshToken("new_token");

        // Act
        UserDto dto = Mapper.Map<UserDto>(user);

        // Assert
        dto.Should().NotBeNull();
        dto.TeslaAccount.Should().NotBeNull();
        dto.TeslaAccount!.TeslaAccountId.Should().Be("tesla123");
        dto.TeslaAccount.LinkedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
        dto.TeslaAccount.IsActive.Should().BeTrue();
        dto.TeslaAccount.LastSyncedAt.Should().NotBeNull();
    }

    [Fact]
    public void Map_UserWithInactiveTeslaAccount_To_UserDto_MapsIsActiveFalse()
    {
        // Arrange
        User user = CreateTestUser();
        user.LinkTeslaAccount("tesla123");
        user.UnlinkTeslaAccount();

        // Act
        UserDto dto = Mapper.Map<UserDto>(user);

        // Assert
        dto.Should().NotBeNull();
        dto.TeslaAccount.Should().NotBeNull();
        dto.TeslaAccount!.IsActive.Should().BeFalse();
    }

    [Fact]
    public void Map_UserWithNullDisplayName_To_UserDto_MapsNullDisplayName()
    {
        // Arrange
        User user = CreateTestUser("ext123", "test@example.com", null);

        // Act
        UserDto dto = Mapper.Map<UserDto>(user);

        // Assert
        dto.Should().NotBeNull();
        dto.DisplayName.Should().BeNull();
    }

    [Fact]
    public void Map_UserWithoutLastLogin_To_UserDto_MapsNullLastLoginAt()
    {
        // Arrange
        User user = CreateTestUser();

        // Act
        UserDto dto = Mapper.Map<UserDto>(user);

        // Assert
        dto.Should().NotBeNull();
        dto.LastLoginAt.Should().BeNull();
    }

    [Fact]
    public void Map_TeslaAccount_To_TeslaAccountDto_MapsAllProperties()
    {
        // Arrange
        TeslaAccount teslaAccount = TeslaAccount.Create("tesla123");

        // Act
        TeslaAccountDto dto = Mapper.Map<TeslaAccountDto>(teslaAccount);

        // Assert
        dto.Should().NotBeNull();
        dto.TeslaAccountId.Should().Be("tesla123");
        dto.LinkedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
        dto.IsActive.Should().BeTrue();
        dto.LastSyncedAt.Should().BeNull();
    }

    [Fact]
    public void Map_DeactivatedTeslaAccount_To_TeslaAccountDto_MapsInactiveStatus()
    {
        // Arrange
        TeslaAccount teslaAccount = TeslaAccount.Create("tesla123")
            .Deactivate();

        // Act
        TeslaAccountDto dto = Mapper.Map<TeslaAccountDto>(teslaAccount);

        // Assert
        dto.Should().NotBeNull();
        dto.IsActive.Should().BeFalse();
    }

    [Fact]
    public void Map_TeslaAccountWithLastSyncedAt_To_TeslaAccountDto_MapsLastSyncedAt()
    {
        // Arrange
        TeslaAccount teslaAccount = TeslaAccount.Create("tesla123")
            .UpdateRefreshToken("token");

        // Act
        TeslaAccountDto dto = Mapper.Map<TeslaAccountDto>(teslaAccount);

        // Assert
        dto.Should().NotBeNull();
        dto.LastSyncedAt.Should().NotBeNull();
        dto.LastSyncedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
    }
}

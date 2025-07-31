namespace TeslaStarter.Domain.Tests.Users;

public class TeslaAccountTests
{
    [Fact]
    public void Create_WithValidData_CreatesTeslaAccount()
    {
        // Arrange
        string teslaAccountId = "tesla123";
        DateTime beforeCreation = DateTime.UtcNow;

        // Act
        TeslaAccount account = TeslaAccount.Create(teslaAccountId);

        // Assert
        account.Should().NotBeNull();
        account.TeslaAccountId.Value.Should().Be(teslaAccountId);
        account.LinkedAt.Should().BeOnOrAfter(beforeCreation);
        account.LinkedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
        account.IsActive.Should().BeTrue();
        account.RefreshToken.Should().BeNull();
        account.LastSyncedAt.Should().BeNull();
    }

    [Fact]
    public void UpdateRefreshToken_UpdatesTokenAndLastSyncedAt()
    {
        // Arrange
        TeslaAccount account = TeslaAccount.Create("tesla123");
        string newToken = "new_refresh_token";
        DateTime beforeUpdate = DateTime.UtcNow;

        // Act
        TeslaAccount updatedAccount = account.UpdateRefreshToken(newToken);

        // Assert
        updatedAccount.Should().NotBeSameAs(account); // Immutability check
        updatedAccount.TeslaAccountId.Should().Be(account.TeslaAccountId);
        updatedAccount.LinkedAt.Should().Be(account.LinkedAt);
        updatedAccount.IsActive.Should().BeTrue();
        updatedAccount.RefreshToken.Should().Be(newToken);
        updatedAccount.LastSyncedAt.Should().BeOnOrAfter(beforeUpdate);
        updatedAccount.LastSyncedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
    }

    [Fact]
    public void UpdateTokens_UpdatesAllTokensAndLastSyncedAt()
    {
        // Arrange
        TeslaAccount account = TeslaAccount.Create("tesla123");
        string newAccessToken = "new_access_token";
        string newRefreshToken = "new_refresh_token";
        DateTime expiresAt = DateTime.UtcNow.AddHours(8);
        DateTime beforeUpdate = DateTime.UtcNow;

        // Act
        TeslaAccount updatedAccount = account.UpdateTokens(newAccessToken, newRefreshToken, expiresAt);

        // Assert
        updatedAccount.Should().NotBeSameAs(account); // Immutability check
        updatedAccount.TeslaAccountId.Should().Be(account.TeslaAccountId);
        updatedAccount.LinkedAt.Should().Be(account.LinkedAt);
        updatedAccount.IsActive.Should().BeTrue();
        updatedAccount.AccessToken.Should().Be(newAccessToken);
        updatedAccount.RefreshToken.Should().Be(newRefreshToken);
        updatedAccount.TokenExpiresAt.Should().Be(expiresAt);
        updatedAccount.LastSyncedAt.Should().BeOnOrAfter(beforeUpdate);
        updatedAccount.LastSyncedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
    }

    [Fact]
    public void Deactivate_SetsIsActiveToFalseAndClearsRefreshToken()
    {
        // Arrange
        TeslaAccount account = TeslaAccount.Create("tesla123");
        TeslaAccount accountWithToken = account.UpdateRefreshToken("refresh_token");

        // Act
        TeslaAccount deactivatedAccount = accountWithToken.Deactivate();

        // Assert
        deactivatedAccount.Should().NotBeSameAs(accountWithToken); // Immutability check
        deactivatedAccount.TeslaAccountId.Should().Be(accountWithToken.TeslaAccountId);
        deactivatedAccount.LinkedAt.Should().Be(accountWithToken.LinkedAt);
        deactivatedAccount.IsActive.Should().BeFalse();
        deactivatedAccount.RefreshToken.Should().BeNull();
        deactivatedAccount.LastSyncedAt.Should().Be(accountWithToken.LastSyncedAt);
    }

    [Fact]
    public void Reactivate_SetsIsActiveToTrue()
    {
        // Arrange
        TeslaAccount account = TeslaAccount.Create("tesla123");
        TeslaAccount deactivatedAccount = account.Deactivate();

        // Act
        TeslaAccount reactivatedAccount = deactivatedAccount.Reactivate();

        // Assert
        reactivatedAccount.Should().NotBeSameAs(deactivatedAccount); // Immutability check
        reactivatedAccount.TeslaAccountId.Should().Be(deactivatedAccount.TeslaAccountId);
        reactivatedAccount.LinkedAt.Should().Be(deactivatedAccount.LinkedAt);
        reactivatedAccount.IsActive.Should().BeTrue();
        reactivatedAccount.RefreshToken.Should().Be(deactivatedAccount.RefreshToken);
        reactivatedAccount.LastSyncedAt.Should().Be(deactivatedAccount.LastSyncedAt);
    }

    [Fact]
    public void Reactivate_PreservesRefreshTokenIfPresent()
    {
        // Arrange
        TeslaAccount account = TeslaAccount.Create("tesla123");
        TeslaAccount accountWithToken = account.UpdateRefreshToken("refresh_token");
        TeslaAccount deactivatedAccount = accountWithToken.Deactivate();

        // Act
        TeslaAccount reactivatedAccount = deactivatedAccount.Reactivate();

        // Assert
        reactivatedAccount.IsActive.Should().BeTrue();
        // RefreshToken should be null after deactivation
        reactivatedAccount.RefreshToken.Should().BeNull();
    }

    [Fact]
    public void Equality_WithDifferentTeslaAccountId_ReturnsFalse()
    {
        // Arrange
        TeslaAccount account1 = TeslaAccount.Create("tesla123");
        TeslaAccount account2 = TeslaAccount.Create("tesla456");

        // Assert
        account1.Should().NotBe(account2);
        (account1 != account2).Should().BeTrue();
    }


    [Fact]
    public void Equality_WithDifferentIsActive_ReturnsFalse()
    {
        // Arrange
        TeslaAccount account1 = TeslaAccount.Create("tesla123");
        TeslaAccount account2 = account1.Deactivate();

        // Assert
        account1.Should().NotBe(account2);
    }

    [Fact]
    public void Equality_DifferentInstancesWithSameData_MayBeEqual()
    {
        // Arrange
        TeslaAccount account1 = TeslaAccount.Create("tesla123");
        Thread.Sleep(10); // Ensure different LinkedAt times
        TeslaAccount account2 = TeslaAccount.Create("tesla123");

        // Assert
        // Different instances are not equal because LinkedAt will be different
        account1.Should().NotBe(account2);
        // But they have the same data
        account1.TeslaAccountId.Should().Be(account2.TeslaAccountId);
        account1.IsActive.Should().Be(account2.IsActive);
    }
}

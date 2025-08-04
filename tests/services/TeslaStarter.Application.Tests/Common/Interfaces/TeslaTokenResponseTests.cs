using System.Diagnostics.CodeAnalysis;
using FluentAssertions;
using TeslaStarter.Application.Common.Interfaces;
using Xunit;

namespace TeslaStarter.Application.Tests.Common.Interfaces;

public sealed class TeslaTokenResponseTests
{
    [Fact]
    public void TeslaTokenResponse_ShouldInitializeWithDefaultValues()
    {
        // Arrange & Act
        TeslaTokenResponse response = new();

        // Assert
        response.AccessToken.Should().BeEmpty();
        response.RefreshToken.Should().BeEmpty();
        response.IdToken.Should().BeEmpty();
        response.TokenType.Should().Be("Bearer");
        response.ExpiresIn.Should().Be(0);
    }

    [Fact]
    public void TeslaTokenResponse_ExpiresAt_ShouldCalculateCorrectly()
    {
        // Arrange
        TeslaTokenResponse response = new()
        {
            ExpiresIn = 3600 // 1 hour
        };
        DateTime beforeAccess = DateTime.UtcNow;

        // Act
        DateTime expiresAt = response.ExpiresAt;

        // Assert
        expiresAt.Should().BeCloseTo(beforeAccess.AddSeconds(3600), TimeSpan.FromSeconds(1));
    }

    [Fact]
    public void TeslaTokenResponse_ShouldSetAllProperties()
    {
        // Arrange & Act
        TeslaTokenResponse response = new()
        {
            AccessToken = "access-token",
            RefreshToken = "refresh-token",
            IdToken = "id-token",
            TokenType = "Custom",
            ExpiresIn = 7200
        };

        // Assert
        response.AccessToken.Should().Be("access-token");
        response.RefreshToken.Should().Be("refresh-token");
        response.IdToken.Should().Be("id-token");
        response.TokenType.Should().Be("Custom");
        response.ExpiresIn.Should().Be(7200);
    }
}

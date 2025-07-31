using System.Diagnostics.CodeAnalysis;
using FluentAssertions;
using TeslaStarter.Application.Users.Commands.UpdateUserTeslaTokens;
using Xunit;

namespace TeslaStarter.Application.Tests.Users.Commands.UpdateUserTeslaTokens;

[ExcludeFromCodeCoverage(Justification = "Test class")]
public sealed class UpdateUserTeslaTokensCommandTests
{
    [Fact]
    public void UpdateUserTeslaTokensCommand_ShouldInitializeWithDefaultValues()
    {
        // Arrange & Act
        UpdateUserTeslaTokensCommand command = new();

        // Assert
        command.ExternalId.Should().BeEmpty();
        command.AccessToken.Should().BeEmpty();
        command.RefreshToken.Should().BeEmpty();
        command.ExpiresAt.Should().Be(default);
        command.TeslaAccountId.Should().BeEmpty();
    }

    [Fact]
    public void UpdateUserTeslaTokensCommand_ShouldInitializeWithGivenValues()
    {
        // Arrange
        string externalId = "descope123";
        string accessToken = "access-token";
        string refreshToken = "refresh-token";
        DateTime expiresAt = DateTime.UtcNow.AddHours(8);
        string teslaAccountId = "tesla123";

        // Act
        UpdateUserTeslaTokensCommand command = new()
        {
            ExternalId = externalId,
            AccessToken = accessToken,
            RefreshToken = refreshToken,
            ExpiresAt = expiresAt,
            TeslaAccountId = teslaAccountId
        };

        // Assert
        command.ExternalId.Should().Be(externalId);
        command.AccessToken.Should().Be(accessToken);
        command.RefreshToken.Should().Be(refreshToken);
        command.ExpiresAt.Should().Be(expiresAt);
        command.TeslaAccountId.Should().Be(teslaAccountId);
    }
}

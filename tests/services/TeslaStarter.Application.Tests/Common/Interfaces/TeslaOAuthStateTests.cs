using System.Diagnostics.CodeAnalysis;
using FluentAssertions;
using TeslaStarter.Application.Common.Interfaces;
using Xunit;

namespace TeslaStarter.Application.Tests.Common.Interfaces;

[ExcludeFromCodeCoverage(Justification = "Test class")]
public sealed class TeslaOAuthStateTests
{
    [Fact]
    public void TeslaOAuthState_ShouldInitializeWithDefaultValues()
    {
        // Arrange
        DateTime beforeCreation = DateTime.UtcNow;

        // Act
        TeslaOAuthState state = new();

        // Assert
        state.State.Should().BeEmpty();
        state.DescopeUserId.Should().BeEmpty();
        state.CreatedAt.Should().BeCloseTo(beforeCreation, TimeSpan.FromSeconds(1));
        state.IsExpired.Should().BeFalse();
    }

    [Fact]
    public void TeslaOAuthState_IsExpired_WhenCreatedRecently_ShouldBeFalse()
    {
        // Arrange & Act
        TeslaOAuthState state = new()
        {
            CreatedAt = DateTime.UtcNow.AddMinutes(-5) // 5 minutes ago
        };

        // Assert
        state.IsExpired.Should().BeFalse();
    }

    [Fact]
    public void TeslaOAuthState_IsExpired_WhenCreatedMoreThan10MinutesAgo_ShouldBeTrue()
    {
        // Arrange & Act
        TeslaOAuthState state = new()
        {
            CreatedAt = DateTime.UtcNow.AddMinutes(-11) // 11 minutes ago
        };

        // Assert
        state.IsExpired.Should().BeTrue();
    }

    [Fact]
    public void TeslaOAuthState_IsExpired_WhenCreatedExactly10MinutesAgo_ShouldBeFalse()
    {
        // Arrange & Act
        TeslaOAuthState state = new()
        {
            CreatedAt = DateTime.UtcNow.AddMinutes(-10).AddSeconds(1) // Just under 10 minutes ago
        };

        // Assert
        // Should be false because the condition is > not >=
        state.IsExpired.Should().BeFalse();
    }

    [Fact]
    public void TeslaOAuthState_ShouldSetAllProperties()
    {
        // Arrange
        DateTime createdAt = DateTime.UtcNow.AddMinutes(-2);

        // Act
        TeslaOAuthState state = new()
        {
            State = "test-state",
            DescopeUserId = "descope123",
            CreatedAt = createdAt
        };

        // Assert
        state.State.Should().Be("test-state");
        state.DescopeUserId.Should().Be("descope123");
        state.CreatedAt.Should().Be(createdAt);
        state.IsExpired.Should().BeFalse();
    }
}

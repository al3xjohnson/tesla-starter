using System.Diagnostics.CodeAnalysis;
using FluentAssertions;
using TeslaStarter.Application.Common.Interfaces;
using Xunit;

namespace TeslaStarter.Application.Tests.Common.Interfaces;

[ExcludeFromCodeCoverage(Justification = "Test class")]
public sealed class SelectTenantResultTests
{
    [Fact]
    public void SelectTenantResult_ShouldInitializeWithDefaultValues()
    {
        // Arrange & Act
        SelectTenantResult result = new();

        // Assert
        result.SessionToken.Should().BeEmpty();
        result.RefreshToken.Should().BeEmpty();
    }

    [Fact]
    public void SelectTenantResult_ShouldSetAllProperties()
    {
        // Arrange & Act
        SelectTenantResult result = new()
        {
            SessionToken = "session-token-123",
            RefreshToken = "refresh-token-456"
        };

        // Assert
        result.SessionToken.Should().Be("session-token-123");
        result.RefreshToken.Should().Be("refresh-token-456");
    }
}

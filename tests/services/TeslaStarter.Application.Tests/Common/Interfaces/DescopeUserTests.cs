using System.Diagnostics.CodeAnalysis;
using FluentAssertions;
using TeslaStarter.Application.Common.Interfaces;
using Xunit;

namespace TeslaStarter.Application.Tests.Common.Interfaces;

public sealed class DescopeUserTests
{
    [Fact]
    public void DescopeUser_ShouldInitializeWithDefaultValues()
    {
        // Arrange & Act
        DescopeUser user = new();

        // Assert
        user.UserId.Should().BeEmpty();
        user.Email.Should().BeEmpty();
        user.Name.Should().BeNull();
    }

    [Fact]
    public void DescopeUser_ShouldSetAllProperties()
    {
        // Arrange & Act
        DescopeUser user = new()
        {
            UserId = "descope123",
            Email = "test@example.com",
            Name = "Test User"
        };

        // Assert
        user.UserId.Should().Be("descope123");
        user.Email.Should().Be("test@example.com");
        user.Name.Should().Be("Test User");
    }
}

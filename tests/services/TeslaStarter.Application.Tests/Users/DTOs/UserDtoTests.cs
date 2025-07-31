using FluentAssertions;
using TeslaStarter.Application.Users.DTOs;
using Xunit;

namespace TeslaStarter.Application.Tests.Users.DTOs;

public sealed class UserDtoTests
{
    [Fact]
    public void Name_WhenDisplayNameIsNull_ReturnsEmail()
    {
        // Arrange
        UserDto user = new()
        {
            Id = Guid.NewGuid(),
            Email = "test@example.com",
            DisplayName = null
        };

        // Act
        string name = user.Name;

        // Assert
        name.Should().Be("test@example.com");
    }

    [Fact]
    public void Name_WhenDisplayNameIsNotNull_ReturnsDisplayName()
    {
        // Arrange
        UserDto user = new()
        {
            Id = Guid.NewGuid(),
            Email = "test@example.com",
            DisplayName = "Test User"
        };

        // Act
        string name = user.Name;

        // Assert
        name.Should().Be("Test User");
    }
}

using System.Diagnostics.CodeAnalysis;
using FluentAssertions;
using TeslaStarter.Application.Users.Queries.GetUsers;
using Xunit;

namespace TeslaStarter.Application.Tests.Users.Queries.GetUsers;

public sealed class GetUsersQueryTests
{
    [Fact]
    public void GetUsersQuery_ShouldBeCreated()
    {
        // Arrange & Act
        GetUsersQuery query = new();

        // Assert
        query.Should().NotBeNull();
    }
}

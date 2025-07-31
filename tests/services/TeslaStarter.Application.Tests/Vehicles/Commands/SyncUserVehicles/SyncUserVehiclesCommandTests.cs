using System.Diagnostics.CodeAnalysis;
using FluentAssertions;
using TeslaStarter.Application.Vehicles.Commands.SyncUserVehicles;
using Xunit;

namespace TeslaStarter.Application.Tests.Vehicles.Commands.SyncUserVehicles;

[ExcludeFromCodeCoverage(Justification = "Test class")]
public sealed class SyncUserVehiclesCommandTests
{
    [Fact]
    public void SyncUserVehiclesCommand_ShouldInitializeWithDefaultValues()
    {
        // Arrange & Act
        SyncUserVehiclesCommand command = new();

        // Assert
        command.ExternalId.Should().BeEmpty();
    }

    [Fact]
    public void SyncUserVehiclesCommand_ShouldInitializeWithGivenExternalId()
    {
        // Arrange
        string externalId = "descope123";

        // Act
        SyncUserVehiclesCommand command = new()
        {
            ExternalId = externalId
        };

        // Assert
        command.ExternalId.Should().Be(externalId);
    }
}

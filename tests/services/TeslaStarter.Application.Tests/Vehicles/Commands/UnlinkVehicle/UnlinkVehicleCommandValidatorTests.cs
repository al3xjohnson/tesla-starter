namespace TeslaStarter.Application.Tests.Vehicles.Commands.UnlinkVehicle;

public sealed class UnlinkVehicleCommandValidatorTests
{
    private readonly UnlinkVehicleCommandValidator _validator;

    public UnlinkVehicleCommandValidatorTests()
    {
        _validator = new UnlinkVehicleCommandValidator();
    }

    [Fact]
    public void Validate_ValidCommand_ShouldPassValidation()
    {
        // Arrange
        UnlinkVehicleCommand command = new()
        {
            VehicleId = Guid.NewGuid()
        };

        // Act
        FluentValidation.Results.ValidationResult result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeTrue();
        result.Errors.Should().BeEmpty();
    }

    [Fact]
    public void Validate_EmptyVehicleId_ShouldFailValidation()
    {
        // Arrange
        UnlinkVehicleCommand command = new()
        {
            VehicleId = Guid.Empty
        };

        // Act
        FluentValidation.Results.ValidationResult result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().ContainSingle();
        result.Errors[0].PropertyName.Should().Be("VehicleId");
        result.Errors[0].ErrorMessage.Should().Be("Vehicle ID is required.");
    }
}

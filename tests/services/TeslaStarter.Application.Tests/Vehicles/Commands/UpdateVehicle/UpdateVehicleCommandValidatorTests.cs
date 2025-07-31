namespace TeslaStarter.Application.Tests.Vehicles.Commands.UpdateVehicle;

public sealed class UpdateVehicleCommandValidatorTests
{
    private readonly UpdateVehicleCommandValidator _validator;

    public UpdateVehicleCommandValidatorTests()
    {
        _validator = new UpdateVehicleCommandValidator();
    }

    [Fact]
    public void Validate_ValidCommand_ShouldPassValidation()
    {
        // Arrange
        UpdateVehicleCommand command = new()
        {
            VehicleId = Guid.NewGuid(),
            DisplayName = "My Tesla"
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
        UpdateVehicleCommand command = new()
        {
            VehicleId = Guid.Empty,
            DisplayName = "My Tesla"
        };

        // Act
        FluentValidation.Results.ValidationResult result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().ContainSingle(x => x.PropertyName == "VehicleId");
        result.Errors[0].ErrorMessage.Should().Be("Vehicle ID is required.");
    }

    [Fact]
    public void Validate_DisplayNameTooLong_ShouldFailValidation()
    {
        // Arrange
        UpdateVehicleCommand command = new()
        {
            VehicleId = Guid.NewGuid(),
            DisplayName = new string('a', 101)
        };

        // Act
        FluentValidation.Results.ValidationResult result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().ContainSingle(x => x.PropertyName == "DisplayName");
        result.Errors[0].ErrorMessage.Should().Be("Display name must not exceed 100 characters.");
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    public void Validate_NullOrEmptyDisplayName_ShouldPassValidation(string? displayName)
    {
        // Arrange
        UpdateVehicleCommand command = new()
        {
            VehicleId = Guid.NewGuid(),
            DisplayName = displayName
        };

        // Act
        FluentValidation.Results.ValidationResult result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeTrue();
        result.Errors.Should().BeEmpty();
    }

    [Fact]
    public void Validate_ValidDisplayNameLength_ShouldPassValidation()
    {
        // Arrange
        UpdateVehicleCommand command = new()
        {
            VehicleId = Guid.NewGuid(),
            DisplayName = new string('a', 100) // Exactly 100 characters
        };

        // Act
        FluentValidation.Results.ValidationResult result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeTrue();
        result.Errors.Should().BeEmpty();
    }
}

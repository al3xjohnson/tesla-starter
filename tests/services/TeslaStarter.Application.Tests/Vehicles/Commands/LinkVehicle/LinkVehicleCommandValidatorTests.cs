namespace TeslaStarter.Application.Tests.Vehicles.Commands.LinkVehicle;

public sealed class LinkVehicleCommandValidatorTests
{
    private readonly LinkVehicleCommandValidator _validator;

    public LinkVehicleCommandValidatorTests()
    {
        _validator = new LinkVehicleCommandValidator();
    }

    [Fact]
    public void Validate_ValidCommand_ShouldPassValidation()
    {
        // Arrange
        LinkVehicleCommand command = new()
        {
            TeslaAccountId = "tesla123",
            VehicleIdentifier = "VIN123",
            DisplayName = "My Tesla"
        };

        // Act
        FluentValidation.Results.ValidationResult result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeTrue();
        result.Errors.Should().BeEmpty();
    }

    [Theory]
    [InlineData("")]
    [InlineData(null)]
    public void Validate_EmptyTeslaAccountId_ShouldFailValidation(string? teslaAccountId)
    {
        // Arrange
        LinkVehicleCommand command = new()
        {
            TeslaAccountId = teslaAccountId!,
            VehicleIdentifier = "VIN123",
            DisplayName = "My Tesla"
        };

        // Act
        FluentValidation.Results.ValidationResult result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().ContainSingle(x => x.PropertyName == "TeslaAccountId");
        result.Errors[0].ErrorMessage.Should().Be("Tesla account ID is required.");
    }

    [Fact]
    public void Validate_TeslaAccountIdTooLong_ShouldFailValidation()
    {
        // Arrange
        LinkVehicleCommand command = new()
        {
            TeslaAccountId = new string('a', 256),
            VehicleIdentifier = "VIN123",
            DisplayName = "My Tesla"
        };

        // Act
        FluentValidation.Results.ValidationResult result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().ContainSingle(x => x.PropertyName == "TeslaAccountId");
        result.Errors[0].ErrorMessage.Should().Be("Tesla account ID must not exceed 255 characters.");
    }

    [Theory]
    [InlineData("")]
    [InlineData(null)]
    public void Validate_EmptyVehicleIdentifier_ShouldFailValidation(string? vehicleIdentifier)
    {
        // Arrange
        LinkVehicleCommand command = new()
        {
            TeslaAccountId = "tesla123",
            VehicleIdentifier = vehicleIdentifier!,
            DisplayName = "My Tesla"
        };

        // Act
        FluentValidation.Results.ValidationResult result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().ContainSingle(x => x.PropertyName == "VehicleIdentifier");
        result.Errors[0].ErrorMessage.Should().Be("Vehicle identifier is required.");
    }

    [Fact]
    public void Validate_VehicleIdentifierTooLong_ShouldFailValidation()
    {
        // Arrange
        LinkVehicleCommand command = new()
        {
            TeslaAccountId = "tesla123",
            VehicleIdentifier = new string('a', 51),
            DisplayName = "My Tesla"
        };

        // Act
        FluentValidation.Results.ValidationResult result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().ContainSingle(x => x.PropertyName == "VehicleIdentifier");
        result.Errors[0].ErrorMessage.Should().Be("Vehicle identifier must not exceed 50 characters.");
    }

    [Fact]
    public void Validate_DisplayNameTooLong_ShouldFailValidation()
    {
        // Arrange
        LinkVehicleCommand command = new()
        {
            TeslaAccountId = "tesla123",
            VehicleIdentifier = "VIN123",
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
        LinkVehicleCommand command = new()
        {
            TeslaAccountId = "tesla123",
            VehicleIdentifier = "VIN123",
            DisplayName = displayName
        };

        // Act
        FluentValidation.Results.ValidationResult result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeTrue();
        result.Errors.Should().BeEmpty();
    }

    [Fact]
    public void Validate_MultipleErrors_ShouldReturnAllErrors()
    {
        // Arrange
        LinkVehicleCommand command = new()
        {
            TeslaAccountId = "",
            VehicleIdentifier = "",
            DisplayName = new string('a', 101)
        };

        // Act
        FluentValidation.Results.ValidationResult result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().HaveCount(3);
        result.Errors.Should().Contain(x => x.PropertyName == "TeslaAccountId");
        result.Errors.Should().Contain(x => x.PropertyName == "VehicleIdentifier");
        result.Errors.Should().Contain(x => x.PropertyName == "DisplayName");
    }
}

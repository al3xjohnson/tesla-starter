namespace TeslaStarter.Application.Tests.Users.Commands.UnlinkTeslaAccount;

public sealed class UnlinkTeslaAccountCommandValidatorTests
{
    private readonly UnlinkTeslaAccountCommandValidator _validator;

    public UnlinkTeslaAccountCommandValidatorTests()
    {
        _validator = new UnlinkTeslaAccountCommandValidator();
    }

    [Fact]
    public void Validate_ValidCommand_ShouldPassValidation()
    {
        // Arrange
        UnlinkTeslaAccountCommand command = new()
        {
            UserId = Guid.NewGuid()
        };

        // Act
        FluentValidation.Results.ValidationResult result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeTrue();
        result.Errors.Should().BeEmpty();
    }

    [Fact]
    public void Validate_EmptyUserId_ShouldFailValidation()
    {
        // Arrange
        UnlinkTeslaAccountCommand command = new()
        {
            UserId = Guid.Empty
        };

        // Act
        FluentValidation.Results.ValidationResult result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().ContainSingle();
        result.Errors[0].PropertyName.Should().Be("UserId");
        result.Errors[0].ErrorMessage.Should().Be("User ID is required.");
    }
}

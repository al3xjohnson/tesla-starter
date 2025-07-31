namespace TeslaStarter.Application.Tests.Users.Commands.LinkTeslaAccount;

public sealed class LinkTeslaAccountCommandValidatorTests
{
    private readonly LinkTeslaAccountCommandValidator _validator;

    public LinkTeslaAccountCommandValidatorTests()
    {
        _validator = new LinkTeslaAccountCommandValidator();
    }

    [Fact]
    public void Validate_ValidCommand_ShouldPassValidation()
    {
        // Arrange
        LinkTeslaAccountCommand command = new()
        {
            UserId = Guid.NewGuid(),
            TeslaAccountId = "tesla123"
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
        LinkTeslaAccountCommand command = new()
        {
            UserId = Guid.Empty,
            TeslaAccountId = "tesla123"
        };

        // Act
        FluentValidation.Results.ValidationResult result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().ContainSingle(x => x.PropertyName == "UserId");
        result.Errors[0].ErrorMessage.Should().Be("User ID is required.");
    }

    [Theory]
    [InlineData("")]
    [InlineData(null)]
    public void Validate_EmptyTeslaAccountId_ShouldFailValidation(string? teslaAccountId)
    {
        // Arrange
        LinkTeslaAccountCommand command = new()
        {
            UserId = Guid.NewGuid(),
            TeslaAccountId = teslaAccountId!
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
        LinkTeslaAccountCommand command = new()
        {
            UserId = Guid.NewGuid(),
            TeslaAccountId = new string('a', 256)
        };

        // Act
        FluentValidation.Results.ValidationResult result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().ContainSingle(x => x.PropertyName == "TeslaAccountId");
        result.Errors[0].ErrorMessage.Should().Be("Tesla account ID must not exceed 255 characters.");
    }
}

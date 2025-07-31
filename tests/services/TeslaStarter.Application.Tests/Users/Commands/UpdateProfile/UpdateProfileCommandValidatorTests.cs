namespace TeslaStarter.Application.Tests.Users.Commands.UpdateProfile;

public sealed class UpdateProfileCommandValidatorTests
{
    private readonly UpdateProfileCommandValidator _validator;

    public UpdateProfileCommandValidatorTests()
    {
        _validator = new UpdateProfileCommandValidator();
    }

    [Fact]
    public void Validate_ValidCommand_ShouldPassValidation()
    {
        // Arrange
        UpdateProfileCommand command = new()
        {
            UserId = Guid.NewGuid(),
            Email = "test@example.com",
            DisplayName = "Test User"
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
        UpdateProfileCommand command = new()
        {
            UserId = Guid.Empty,
            Email = "test@example.com",
            DisplayName = "Test User"
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
    public void Validate_EmptyEmail_ShouldFailValidation(string? email)
    {
        // Arrange
        UpdateProfileCommand command = new()
        {
            UserId = Guid.NewGuid(),
            Email = email!,
            DisplayName = "Test User"
        };

        // Act
        FluentValidation.Results.ValidationResult result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().ContainSingle(x => x.PropertyName == "Email");
        result.Errors[0].ErrorMessage.Should().Be("Email is required.");
    }

    [Theory]
    [InlineData("notanemail")]
    [InlineData("invalid@")]
    [InlineData("@invalid.com")]
    public void Validate_InvalidEmail_ShouldFailValidation(string email)
    {
        // Arrange
        UpdateProfileCommand command = new()
        {
            UserId = Guid.NewGuid(),
            Email = email,
            DisplayName = "Test User"
        };

        // Act
        FluentValidation.Results.ValidationResult result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().ContainSingle(x => x.PropertyName == "Email");
        result.Errors[0].ErrorMessage.Should().Be("Email must be a valid email address.");
    }

    [Fact]
    public void Validate_EmailTooLong_ShouldFailValidation()
    {
        // Arrange
        UpdateProfileCommand command = new()
        {
            UserId = Guid.NewGuid(),
            Email = new string('a', 247) + "@test.com",
            DisplayName = "Test User"
        };

        // Act
        FluentValidation.Results.ValidationResult result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().ContainSingle(x => x.PropertyName == "Email");
        result.Errors[0].ErrorMessage.Should().Be("Email must not exceed 255 characters.");
    }

    [Fact]
    public void Validate_DisplayNameTooLong_ShouldFailValidation()
    {
        // Arrange
        UpdateProfileCommand command = new()
        {
            UserId = Guid.NewGuid(),
            Email = "test@example.com",
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
        UpdateProfileCommand command = new()
        {
            UserId = Guid.NewGuid(),
            Email = "test@example.com",
            DisplayName = displayName
        };

        // Act
        FluentValidation.Results.ValidationResult result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeTrue();
        result.Errors.Should().BeEmpty();
    }
}

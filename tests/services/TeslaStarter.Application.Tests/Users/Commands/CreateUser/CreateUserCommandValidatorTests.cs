namespace TeslaStarter.Application.Tests.Users.Commands.CreateUser;

public sealed class CreateUserCommandValidatorTests
{
    private readonly CreateUserCommandValidator _validator;

    public CreateUserCommandValidatorTests()
    {
        _validator = new CreateUserCommandValidator();
    }

    [Fact]
    public void Validate_ValidCommand_ShouldPassValidation()
    {
        // Arrange
        CreateUserCommand command = new()
        {
            ExternalId = "ext123",
            Email = "test@example.com",
            DisplayName = "Test User"
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
    public void Validate_EmptyExternalId_ShouldFailValidation(string? externalId)
    {
        // Arrange
        CreateUserCommand command = new()
        {
            ExternalId = externalId!,
            Email = "test@example.com",
            DisplayName = "Test User"
        };

        // Act
        FluentValidation.Results.ValidationResult result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().ContainSingle(x => x.PropertyName == "ExternalId");
        result.Errors[0].ErrorMessage.Should().Be("External ID is required.");
    }

    [Fact]
    public void Validate_ExternalIdTooLong_ShouldFailValidation()
    {
        // Arrange
        CreateUserCommand command = new()
        {
            ExternalId = new string('a', 256),
            Email = "test@example.com",
            DisplayName = "Test User"
        };

        // Act
        FluentValidation.Results.ValidationResult result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().ContainSingle(x => x.PropertyName == "ExternalId");
        result.Errors[0].ErrorMessage.Should().Be("External ID must not exceed 255 characters.");
    }

    [Theory]
    [InlineData("")]
    [InlineData(null)]
    public void Validate_EmptyEmail_ShouldFailValidation(string? email)
    {
        // Arrange
        CreateUserCommand command = new()
        {
            ExternalId = "ext123",
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
        CreateUserCommand command = new()
        {
            ExternalId = "ext123",
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
        CreateUserCommand command = new()
        {
            ExternalId = "ext123",
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
        CreateUserCommand command = new()
        {
            ExternalId = "ext123",
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
        CreateUserCommand command = new()
        {
            ExternalId = "ext123",
            Email = "test@example.com",
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
        CreateUserCommand command = new()
        {
            ExternalId = "",
            Email = "invalid",
            DisplayName = new string('a', 101)
        };

        // Act
        FluentValidation.Results.ValidationResult result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().HaveCount(3);
        result.Errors.Should().Contain(x => x.PropertyName == "ExternalId");
        result.Errors.Should().Contain(x => x.PropertyName == "Email");
        result.Errors.Should().Contain(x => x.PropertyName == "DisplayName");
    }
}

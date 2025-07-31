namespace TeslaStarter.Application.Tests.Common.Exceptions;

public sealed class ValidationExceptionTests
{
    [Fact]
    public void Constructor_Default_CreatesEmptyErrorsDictionary()
    {
        // Act
        Application.Common.Exceptions.ValidationException exception = new Application.Common.Exceptions.ValidationException();

        // Assert
        exception.Message.Should().Be("One or more validation failures have occurred.");
        exception.Errors.Should().NotBeNull();
        exception.Errors.Should().BeEmpty();
    }

    [Fact]
    public void Constructor_WithValidationFailures_GroupsErrorsByPropertyName()
    {
        // Arrange
        List<ValidationFailure> failures =
        [
            new("Email", "Email is required"),
            new("Email", "Email must be valid"),
            new("Name", "Name is required")
        ];

        // Act
        Application.Common.Exceptions.ValidationException exception = new Application.Common.Exceptions.ValidationException(failures);

        // Assert
        exception.Message.Should().Be("One or more validation failures have occurred.");
        exception.Errors.Should().HaveCount(2);

        exception.Errors["Email"].Should().HaveCount(2);
        exception.Errors["Email"].Should().Contain("Email is required");
        exception.Errors["Email"].Should().Contain("Email must be valid");

        exception.Errors["Name"].Should().HaveCount(1);
        exception.Errors["Name"].Should().Contain("Name is required");
    }

    [Fact]
    public void Constructor_WithEmptyFailures_CreatesEmptyErrorsDictionary()
    {
        // Arrange
        List<ValidationFailure> failures = [];

        // Act
        Application.Common.Exceptions.ValidationException exception = new Application.Common.Exceptions.ValidationException(failures);

        // Assert
        exception.Errors.Should().NotBeNull();
        exception.Errors.Should().BeEmpty();
    }

    [Fact]
    public void Constructor_WithDuplicateErrors_IncludesAllDuplicates()
    {
        // Arrange
        List<ValidationFailure> failures =
        [
            new("Email", "Email is required"),
            new("Email", "Email is required"), // Duplicate
            new("Email", "Email is required")  // Another duplicate
        ];

        // Act
        Application.Common.Exceptions.ValidationException exception = new Application.Common.Exceptions.ValidationException(failures);

        // Assert
        exception.Errors["Email"].Should().HaveCount(3);
        exception.Errors["Email"].Should().AllBe("Email is required");
    }

    [Fact]
    public void Constructor_WithNullPropertyName_GroupsUnderEmptyString()
    {
        // Arrange
        List<ValidationFailure> failures =
        [
            new(null!, "General error"),
            new("", "Another general error")
        ];

        // Act
        Application.Common.Exceptions.ValidationException exception = new Application.Common.Exceptions.ValidationException(failures);

        // Assert
        exception.Errors.Should().HaveCount(1);
        exception.Errors[""].Should().HaveCount(2);
        exception.Errors[""].Should().Contain("General error");
        exception.Errors[""].Should().Contain("Another general error");
    }

    [Fact]
    public void Errors_IsReadOnly_CannotBeModifiedExternally()
    {
        // Arrange
        Application.Common.Exceptions.ValidationException exception = new Application.Common.Exceptions.ValidationException();

        // Act & Assert
        exception.Errors.Should().BeAssignableTo<IDictionary<string, string[]>>();
        // The dictionary itself can be modified but that's acceptable for this use case
    }
}

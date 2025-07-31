namespace TeslaStarter.Application.Tests.Common.Exceptions;

public sealed class NotFoundExceptionTests
{
    [Fact]
    public void Constructor_Default_CreatesExceptionWithNoMessage()
    {
        // Act
        NotFoundException exception = new NotFoundException();

        // Assert
        exception.Message.Should().Be("Exception of type 'TeslaStarter.Application.Common.Exceptions.NotFoundException' was thrown.");
    }

    [Fact]
    public void Constructor_WithMessage_CreatesExceptionWithMessage()
    {
        // Arrange
        const string message = "Resource not found";

        // Act
        NotFoundException exception = new NotFoundException(message);

        // Assert
        exception.Message.Should().Be(message);
    }

    [Fact]
    public void Constructor_WithMessageAndInnerException_CreatesExceptionWithBoth()
    {
        // Arrange
        const string message = "Resource not found";
        InvalidOperationException innerException = new InvalidOperationException("Inner error");

        // Act
        NotFoundException exception = new NotFoundException(message, innerException);

        // Assert
        exception.Message.Should().Be(message);
        exception.InnerException.Should().Be(innerException);
    }

    [Fact]
    public void Constructor_WithNameAndKey_CreatesFormattedMessage()
    {
        // Arrange
        const string name = "User";
        const int key = 123;

        // Act
        NotFoundException exception = new NotFoundException(name, key);

        // Assert
        exception.Message.Should().Be("Entity \"User\" (123) was not found.");
    }

    [Fact]
    public void Constructor_WithNameAndGuidKey_CreatesFormattedMessage()
    {
        // Arrange
        const string name = "Vehicle";
        Guid key = Guid.NewGuid();

        // Act
        NotFoundException exception = new NotFoundException(name, key);

        // Assert
        exception.Message.Should().Be($"Entity \"Vehicle\" ({key}) was not found.");
    }

    [Fact]
    public void Constructor_WithNameAndStringKey_CreatesFormattedMessage()
    {
        // Arrange
        const string name = "Product";
        const string key = "ABC123";

        // Act
        NotFoundException exception = new NotFoundException(name, key);

        // Assert
        exception.Message.Should().Be("Entity \"Product\" (ABC123) was not found.");
    }

    [Fact]
    public void Constructor_WithNameAndNullKey_HandlesNullProperly()
    {
        // Arrange
        const string name = "Entity";
        object? key = null;

        // Act
        NotFoundException exception = new NotFoundException(name, key!);

        // Assert
        exception.Message.Should().Be("Entity \"Entity\" () was not found.");
    }
}

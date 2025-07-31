namespace TeslaStarter.Application.Tests.Common.Exceptions;

public sealed class ForbiddenAccessExceptionTests
{
    [Fact]
    public void Constructor_Default_CreatesExceptionWithNoMessage()
    {
        // Act
        ForbiddenAccessException exception = new ForbiddenAccessException();

        // Assert
        exception.Message.Should().Be("Exception of type 'TeslaStarter.Application.Common.Exceptions.ForbiddenAccessException' was thrown.");
    }

    [Fact]
    public void Constructor_WithMessage_CreatesExceptionWithMessage()
    {
        // Arrange
        const string message = "Access denied to this resource";

        // Act
        ForbiddenAccessException exception = new ForbiddenAccessException(message);

        // Assert
        exception.Message.Should().Be(message);
    }

    [Fact]
    public void Constructor_WithMessageAndInnerException_CreatesExceptionWithBoth()
    {
        // Arrange
        const string message = "Access denied";
        UnauthorizedAccessException innerException = new UnauthorizedAccessException("Not authorized");

        // Act
        ForbiddenAccessException exception = new ForbiddenAccessException(message, innerException);

        // Assert
        exception.Message.Should().Be(message);
        exception.InnerException.Should().Be(innerException);
    }

    [Fact]
    public void Constructor_WithNullMessage_CreatesExceptionWithNullMessage()
    {
        // Act
        ForbiddenAccessException exception = new ForbiddenAccessException(null!);

        // Assert
        exception.Message.Should().Be("Exception of type 'TeslaStarter.Application.Common.Exceptions.ForbiddenAccessException' was thrown.");
    }

    [Fact]
    public void Constructor_WithEmptyMessage_CreatesExceptionWithEmptyMessage()
    {
        // Act
        ForbiddenAccessException exception = new ForbiddenAccessException(string.Empty);

        // Assert
        exception.Message.Should().Be(string.Empty);
    }

    [Fact]
    public void Constructor_WithNullInnerException_CreatesExceptionWithoutInnerException()
    {
        // Arrange
        const string message = "Access denied";

        // Act
        ForbiddenAccessException exception = new ForbiddenAccessException(message, null!);

        // Assert
        exception.Message.Should().Be(message);
        exception.InnerException.Should().BeNull();
    }
}

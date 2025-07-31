using Common.Domain.Exceptions;

namespace Common.Domain.Tests;

// Test implementation of abstract DomainException
internal sealed class TestDomainException : DomainException
{
    public TestDomainException() { }
    public TestDomainException(string message) : base(message) { }
    public TestDomainException(string message, Exception innerException) : base(message, innerException) { }
}

public class DomainExceptionTests
{
    [Fact]
    public void Constructor_Default_ShouldCreateExceptionWithDefaultMessage()
    {
        // Act
        TestDomainException exception = new();

        // Assert
        exception.Message.Should().NotBeNullOrEmpty();
        exception.InnerException.Should().BeNull();
    }

    [Fact]
    public void Constructor_WithMessage_ShouldSetMessage()
    {
        // Arrange
        string message = "Domain rule violated";

        // Act
        TestDomainException exception = new(message);

        // Assert
        exception.Message.Should().Be(message);
    }

    [Fact]
    public void Constructor_WithMessageAndInnerException_ShouldSetBoth()
    {
        // Arrange
        string message = "Domain rule violated";
        InvalidOperationException innerException = new("Inner error");

        // Act
        TestDomainException exception = new(message, innerException);

        // Assert
        exception.Message.Should().Be(message);
        exception.InnerException.Should().Be(innerException);
    }
}

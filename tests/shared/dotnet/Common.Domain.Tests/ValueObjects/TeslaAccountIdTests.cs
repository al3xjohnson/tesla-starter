using Common.Domain.ValueObjects;

namespace Common.Domain.Tests.ValueObjects;

public class TeslaAccountIdTests
{
    [Theory]
    [InlineData("12345")]
    [InlineData("tesla_account_123")]
    [InlineData("a")]
    public void Create_WithValidId_ReturnsTeslaAccountId(string validId)
    {
        // Act
        TeslaAccountId teslaAccountId = TeslaAccountId.Create(validId);

        // Assert
        teslaAccountId.Should().NotBeNull();
        teslaAccountId.Value.Should().Be(validId);
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData(null)]
    public void Create_WithNullOrWhitespace_ThrowsArgumentException(string invalidId)
    {
        // Act & Assert
        Action act = () => TeslaAccountId.Create(invalidId);
        act.Should().Throw<ArgumentException>()
            .WithMessage("Tesla account ID cannot be empty*")
            .And.ParamName.Should().Be("value");
    }

    [Fact]
    public void Create_WithIdExceeding100Characters_ThrowsArgumentException()
    {
        // Arrange
        string longId = new string('a', 101);

        // Act & Assert
        Action act = () => TeslaAccountId.Create(longId);
        act.Should().Throw<ArgumentException>()
            .WithMessage("Tesla account ID cannot exceed 100 characters*")
            .And.ParamName.Should().Be("value");
    }

    [Fact]
    public void Create_WithExactly100Characters_Succeeds()
    {
        // Arrange
        string maxLengthId = new string('a', 100);

        // Act
        TeslaAccountId teslaAccountId = TeslaAccountId.Create(maxLengthId);

        // Assert
        teslaAccountId.Value.Should().Be(maxLengthId);
    }

    [Fact]
    public void Create_DoesNotTrimWhitespace()
    {
        // Act
        TeslaAccountId teslaAccountId = TeslaAccountId.Create("  id_with_spaces  ");

        // Assert
        teslaAccountId.Value.Should().Be("  id_with_spaces  ");
    }

    [Fact]
    public void Equality_WithSameValue_ReturnsTrue()
    {
        // Arrange
        TeslaAccountId id1 = TeslaAccountId.Create("tesla123");
        TeslaAccountId id2 = TeslaAccountId.Create("tesla123");

        // Assert
        id1.Should().Be(id2);
        (id1 == id2).Should().BeTrue();
        id1.GetHashCode().Should().Be(id2.GetHashCode());
    }

    [Fact]
    public void Equality_WithDifferentValue_ReturnsFalse()
    {
        // Arrange
        TeslaAccountId id1 = TeslaAccountId.Create("tesla123");
        TeslaAccountId id2 = TeslaAccountId.Create("tesla456");

        // Assert
        id1.Should().NotBe(id2);
        (id1 != id2).Should().BeTrue();
    }

    [Fact]
    public void ToString_ReturnsValue()
    {
        // Arrange
        TeslaAccountId teslaAccountId = TeslaAccountId.Create("tesla123");

        // Act
        string result = teslaAccountId.ToString();

        // Assert
        result.Should().Be("tesla123");
    }

    [Fact]
    public void ImplicitStringConversion_ReturnsValue()
    {
        // Arrange
        TeslaAccountId teslaAccountId = TeslaAccountId.Create("tesla123");

        // Act
        string result = teslaAccountId;

        // Assert
        result.Should().Be("tesla123");
    }

}

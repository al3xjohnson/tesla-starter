namespace TeslaStarter.Domain.Tests.Users;

public class ExternalIdTests
{
    [Theory]
    [InlineData("user123")]
    [InlineData("auth0|507f1f77bcf86cd799439011")]
    [InlineData("google-oauth2|118387408996050958936")]
    [InlineData("a")]
    public void Create_WithValidId_ReturnsExternalId(string validId)
    {
        // Act
        ExternalId externalId = ExternalId.Create(validId);

        // Assert
        externalId.Should().NotBeNull();
        externalId.Value.Should().Be(validId.Trim());
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData(null)]
    public void Create_WithNullOrWhitespace_ThrowsArgumentException(string invalidId)
    {
        // Act & Assert
        Action act = () => ExternalId.Create(invalidId);
        act.Should().Throw<ArgumentException>()
            .WithMessage("External ID cannot be empty*")
            .And.ParamName.Should().Be("value");
    }

    [Fact]
    public void Create_WithIdExceeding255Characters_ThrowsArgumentException()
    {
        // Arrange
        string longId = new string('a', 256);

        // Act & Assert
        Action act = () => ExternalId.Create(longId);
        act.Should().Throw<ArgumentException>()
            .WithMessage("External ID cannot exceed 255 characters*")
            .And.ParamName.Should().Be("value");
    }

    [Fact]
    public void Create_WithExactly255Characters_Succeeds()
    {
        // Arrange
        string maxLengthId = new string('a', 255);

        // Act
        ExternalId externalId = ExternalId.Create(maxLengthId);

        // Assert
        externalId.Value.Should().Be(maxLengthId);
    }

    [Fact]
    public void Create_WithWhitespace_TrimsValue()
    {
        // Act
        ExternalId externalId = ExternalId.Create("  user123  ");

        // Assert
        externalId.Value.Should().Be("user123");
    }

    [Fact]
    public void Equality_WithSameValue_ReturnsTrue()
    {
        // Arrange
        ExternalId id1 = ExternalId.Create("user123");
        ExternalId id2 = ExternalId.Create("user123");

        // Assert
        id1.Should().Be(id2);
        (id1 == id2).Should().BeTrue();
        id1.GetHashCode().Should().Be(id2.GetHashCode());
    }

    [Fact]
    public void Equality_WithDifferentValue_ReturnsFalse()
    {
        // Arrange
        ExternalId id1 = ExternalId.Create("user123");
        ExternalId id2 = ExternalId.Create("user456");

        // Assert
        id1.Should().NotBe(id2);
        (id1 != id2).Should().BeTrue();
    }

    [Fact]
    public void ToString_ReturnsValue()
    {
        // Arrange
        ExternalId externalId = ExternalId.Create("user123");

        // Act
        string result = externalId.ToString();

        // Assert
        result.Should().Be("user123");
    }

    [Fact]
    public void ImplicitStringConversion_ReturnsValue()
    {
        // Arrange
        ExternalId externalId = ExternalId.Create("user123");

        // Act
        string result = externalId;

        // Assert
        result.Should().Be("user123");
    }

}

using Common.Domain.ValueObjects;

namespace Common.Domain.Tests.ValueObjects;

public class UserIdTests
{
    [Fact]
    public void New_CreatesUniqueUserIds()
    {
        // Act
        UserId userId1 = UserId.New();
        UserId userId2 = UserId.New();

        // Assert
        userId1.Should().NotBe(userId2);
        userId1.Value.Should().NotBe(Guid.Empty);
        userId2.Value.Should().NotBe(Guid.Empty);
    }

    [Fact]
    public void Empty_ReturnsUserIdWithEmptyGuid()
    {
        // Act
        UserId emptyUserId = UserId.Empty;

        // Assert
        emptyUserId.Value.Should().Be(Guid.Empty);
    }

    [Fact]
    public void ToString_ReturnsGuidString()
    {
        // Arrange
        Guid guid = Guid.NewGuid();
        UserId userId = new(guid);

        // Act
        string result = userId.ToString();

        // Assert
        result.Should().Be(guid.ToString());
    }

    [Fact]
    public void Constructor_WithGuid_CreatesUserId()
    {
        // Arrange
        Guid guid = Guid.NewGuid();

        // Act
        UserId userId = new UserId(guid);

        // Assert
        userId.Value.Should().Be(guid);
    }

    [Fact]
    public void Equality_WithSameValue_ReturnsTrue()
    {
        // Arrange
        Guid guid = Guid.NewGuid();
        UserId userId1 = new(guid);
        UserId userId2 = new(guid);

        // Assert
        userId1.Should().Be(userId2);
        (userId1 == userId2).Should().BeTrue();
        userId1.GetHashCode().Should().Be(userId2.GetHashCode());
    }

    [Fact]
    public void Equality_WithDifferentValue_ReturnsFalse()
    {
        // Arrange
        UserId userId1 = new(Guid.NewGuid());
        UserId userId2 = new(Guid.NewGuid());

        // Assert
        userId1.Should().NotBe(userId2);
        (userId1 != userId2).Should().BeTrue();
    }

    [Fact]
    public void WithOperator_CreatesNewInstanceWithSameValue()
    {
        // Arrange
        UserId userId = new(Guid.NewGuid());

        // Act
        UserId copy = userId with { };

        // Assert
        copy.Should().Be(userId);
        copy.Should().NotBeSameAs(userId);
    }
}

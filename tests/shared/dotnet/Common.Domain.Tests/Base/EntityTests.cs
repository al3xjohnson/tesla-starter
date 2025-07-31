using Common.Domain.Base;

namespace Common.Domain.Tests.Base;

// Test entity for testing the Entity base class
internal sealed class TestEntity : Entity<Guid>
{
    public TestEntity(Guid id) : base(id) { }

    // Constructor for EF Core - this will cover the parameterless constructor
    public TestEntity() : base() { }
}

public class EntityTests
{
    [Fact]
    public void Constructor_WithId_ShouldSetId()
    {
        // Arrange
        Guid id = Guid.NewGuid();

        // Act
        TestEntity entity = new(id);

        // Assert
        entity.Id.Should().Be(id);
    }

    [Fact]
    public void ParameterlessConstructor_ShouldSetDefaultId()
    {
        // Act
        TestEntity entity = new();

        // Assert
        entity.Id.Should().Be(default(Guid));
    }

    [Fact]
    public void Equals_WithSameId_ShouldReturnTrue()
    {
        // Arrange
        Guid id = Guid.NewGuid();
        TestEntity entity1 = new(id);
        TestEntity entity2 = new(id);

        // Act & Assert
        entity1.Equals(entity2).Should().BeTrue();
    }

    [Fact]
    public void Equals_WithDifferentId_ShouldReturnFalse()
    {
        // Arrange
        TestEntity entity1 = new(Guid.NewGuid());
        TestEntity entity2 = new(Guid.NewGuid());

        // Act & Assert
        entity1.Equals(entity2).Should().BeFalse();
    }

    [Fact]
    public void Equals_WithNull_ShouldReturnFalse()
    {
        // Arrange
        TestEntity entity = new(Guid.NewGuid());

        // Act & Assert
        entity.Equals(null).Should().BeFalse();
    }


    [Fact]
    public void EqualsObject_WithDifferentType_ShouldReturnFalse()
    {
        // Arrange
        TestEntity entity = new(Guid.NewGuid());
        object other = "not an entity";

        // Act & Assert
        entity.Equals(other).Should().BeFalse();
    }

    [Fact]
    public void GetHashCode_ShouldReturnIdHashCode()
    {
        // Arrange
        Guid id = Guid.NewGuid();
        TestEntity entity = new(id);

        // Act
        int hashCode = entity.GetHashCode();

        // Assert
        hashCode.Should().Be(id.GetHashCode());
    }


    [Fact]
    public void EqualityOperator_WithSameEntities_ShouldReturnTrue()
    {
        // Arrange
        Guid id = Guid.NewGuid();
        TestEntity entity1 = new(id);
        TestEntity entity2 = new(id);

        // Act & Assert
        (entity1 == entity2).Should().BeTrue();
    }

    [Fact]
    public void EqualityOperator_WithDifferentEntities_ShouldReturnFalse()
    {
        // Arrange
        TestEntity entity1 = new(Guid.NewGuid());
        TestEntity entity2 = new(Guid.NewGuid());

        // Act & Assert
        (entity1 == entity2).Should().BeFalse();
    }

    [Fact]
    public void InequalityOperator_WithSameEntities_ShouldReturnFalse()
    {
        // Arrange
        Guid id = Guid.NewGuid();
        TestEntity entity1 = new(id);
        TestEntity entity2 = new(id);

        // Act & Assert
        (entity1 != entity2).Should().BeFalse();
    }

    [Fact]
    public void InequalityOperator_WithDifferentEntities_ShouldReturnTrue()
    {
        // Arrange
        TestEntity entity1 = new(Guid.NewGuid());
        TestEntity entity2 = new(Guid.NewGuid());

        // Act & Assert
        (entity1 != entity2).Should().BeTrue();
    }

    [Fact]
    public void EqualityOperator_WithNullChecks_ShouldWorkCorrectly()
    {
        // Arrange
        TestEntity entity = new(Guid.NewGuid());

        // Act & Assert - Both null
        ((TestEntity?)null == (TestEntity?)null).Should().BeTrue();
        ((TestEntity?)null != (TestEntity?)null).Should().BeFalse();

        // Act & Assert - One null
        (entity == null).Should().BeFalse();
        (null == entity).Should().BeFalse();
        (entity != null).Should().BeTrue();
        (null != entity).Should().BeTrue();
    }
}

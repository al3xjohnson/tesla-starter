using Common.Domain.Base;

namespace Common.Domain.Tests.Base;

public class ValueObjectTests
{
    private sealed class TestValueObject(string property1, int property2) : ValueObject
    {
        public string Property1 { get; } = property1;
        public int Property2 { get; } = property2;

        protected override IEnumerable<object> GetEqualityComponents()
        {
            yield return Property1;
            yield return Property2;
        }
    }

    private sealed class EmptyValueObject : ValueObject
    {
        protected override IEnumerable<object> GetEqualityComponents()
        {
            yield break;
        }
    }

    private sealed class NullComponentValueObject(string? nullableProperty) : ValueObject
    {
        public string? NullableProperty { get; } = nullableProperty;

        protected override IEnumerable<object> GetEqualityComponents()
        {
            yield return NullableProperty!;
        }
    }

    [Fact]
    public void Equals_WithSameValues_ReturnsTrue()
    {
        TestValueObject vo1 = new("test", 123);
        TestValueObject vo2 = new("test", 123);

        bool result = vo1.Equals(vo2);

        result.Should().BeTrue();
    }

    [Fact]
    public void Equals_WithDifferentValues_ReturnsFalse()
    {
        TestValueObject vo1 = new("test", 123);
        TestValueObject vo2 = new("test", 456);

        bool result = vo1.Equals(vo2);

        result.Should().BeFalse();
    }

    [Fact]
    public void Equals_WithNull_ReturnsFalse()
    {
        TestValueObject vo = new("test", 123);

        bool result = vo.Equals(null);

        result.Should().BeFalse();
    }

    [Fact]
    public void ObjectEquals_WithDifferentValueObject_ReturnsFalse()
    {
        TestValueObject vo1 = new("test", 123);
        object vo2 = new TestValueObject("different", 456);

        bool result = vo1.Equals(vo2);

        result.Should().BeFalse();
    }

    [Fact]
    public void ObjectEquals_WithNonValueObjectType_ReturnsFalse()
    {
        TestValueObject vo = new("test", 123);
        object other = "not a value object";

        bool result = vo.Equals(other);

        result.Should().BeFalse();
    }

    [Fact]
    public void GetHashCode_WithSameValues_ReturnsSameHash()
    {
        TestValueObject vo1 = new("test", 123);
        TestValueObject vo2 = new("test", 123);

        int hash1 = vo1.GetHashCode();
        int hash2 = vo2.GetHashCode();

        hash1.Should().Be(hash2);
    }

    [Fact]
    public void GetHashCode_WithDifferentValues_ReturnsDifferentHash()
    {
        TestValueObject vo1 = new("test", 123);
        TestValueObject vo2 = new("test", 456);

        int hash1 = vo1.GetHashCode();
        int hash2 = vo2.GetHashCode();

        hash1.Should().NotBe(hash2);
    }

    [Fact]
    public void GetHashCode_WithNullComponent_HandlesCorrectly()
    {
        NullComponentValueObject vo1 = new(null);
        NullComponentValueObject vo2 = new(null);

        int hash1 = vo1.GetHashCode();
        int hash2 = vo2.GetHashCode();

        hash1.Should().Be(hash2);
    }

    [Fact]
    public void GetHashCode_WithEmptyComponents_ProducesConsistentHash()
    {
        EmptyValueObject vo1 = new();
        EmptyValueObject vo2 = new();

        int hash1 = vo1.GetHashCode();
        int hash2 = vo2.GetHashCode();

        // Empty components should produce consistent hash codes
        hash1.Should().Be(hash2);
    }

    [Fact]
    public void EqualityOperator_WithSameValues_ReturnsTrue()
    {
        TestValueObject vo1 = new("test", 123);
        TestValueObject vo2 = new("test", 123);

        bool result = vo1 == vo2;

        result.Should().BeTrue();
    }

    [Fact]
    public void EqualityOperator_WithDifferentValues_ReturnsFalse()
    {
        TestValueObject vo1 = new("test", 123);
        TestValueObject vo2 = new("test", 456);

        bool result = vo1 == vo2;

        result.Should().BeFalse();
    }

    [Fact]
    public void InequalityOperator_WithSameValues_ReturnsFalse()
    {
        TestValueObject vo1 = new("test", 123);
        TestValueObject vo2 = new("test", 123);

        bool result = vo1 != vo2;

        result.Should().BeFalse();
    }

    [Fact]
    public void InequalityOperator_WithDifferentValues_ReturnsTrue()
    {
        TestValueObject vo1 = new("test", 123);
        TestValueObject vo2 = new("test", 456);

        bool result = vo1 != vo2;

        result.Should().BeTrue();
    }
}

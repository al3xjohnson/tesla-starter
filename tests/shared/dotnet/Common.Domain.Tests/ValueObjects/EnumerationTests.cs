using Common.Domain.ValueObjects;

namespace Common.Domain.Tests.ValueObjects;

public class EnumerationTests
{
    private sealed class TestEnumeration : Enumeration
    {
        public static readonly TestEnumeration First = new(1, "First");
        public static readonly TestEnumeration Second = new(2, "Second");
        public static readonly TestEnumeration Third = new(3, "Third");

        private TestEnumeration(int id, string name) : base(id, name) { }
    }

    private sealed class AnotherEnumeration : Enumeration
    {
        public static readonly AnotherEnumeration One = new(1, "One");

        private AnotherEnumeration(int id, string name) : base(id, name) { }
    }

    [Fact]
    public void ToString_ShouldReturnName()
    {
        TestEnumeration enumeration = TestEnumeration.First;

        string result = enumeration.ToString();

        result.Should().Be("First");
    }

    [Fact]
    public void GetAll_ShouldReturnAllValues()
    {
        IEnumerable<TestEnumeration> all = Enumeration.GetAll<TestEnumeration>();

        all.Should().HaveCount(3);
        all.Should().Contain(TestEnumeration.First);
        all.Should().Contain(TestEnumeration.Second);
        all.Should().Contain(TestEnumeration.Third);
    }

    [Fact]
    public void Equals_WithSameTypeAndId_ShouldReturnTrue()
    {
        TestEnumeration first1 = TestEnumeration.First;
        TestEnumeration first2 = TestEnumeration.First;

        bool result = first1.Equals(first2);

        result.Should().BeTrue();
    }

    [Fact]
    public void Equals_WithDifferentId_ShouldReturnFalse()
    {
        TestEnumeration first = TestEnumeration.First;
        TestEnumeration second = TestEnumeration.Second;

        bool result = first.Equals(second);

        result.Should().BeFalse();
    }

    [Fact]
    public void Equals_WithDifferentType_ShouldReturnFalse()
    {
        TestEnumeration testEnum = TestEnumeration.First;
        AnotherEnumeration anotherEnum = AnotherEnumeration.One;

        bool result = testEnum.Equals(anotherEnum);

        result.Should().BeFalse();
    }

    [Fact]
    public void Equals_WithNull_ShouldReturnFalse()
    {
        TestEnumeration enumeration = TestEnumeration.First;

        bool result = enumeration.Equals(null);

        result.Should().BeFalse();
    }

    [Fact]
    public void GetHashCode_ShouldReturnIdHashCode()
    {
        TestEnumeration enumeration = TestEnumeration.Second;

        int hashCode = enumeration.GetHashCode();

        hashCode.Should().Be(enumeration.Id.GetHashCode());
    }

    [Fact]
    public void AbsoluteDifference_ShouldReturnCorrectValue()
    {
        TestEnumeration first = TestEnumeration.First;
        TestEnumeration third = TestEnumeration.Third;

        int difference = Enumeration.AbsoluteDifference(first, third);

        difference.Should().Be(2);
    }

    [Fact]
    public void FromValue_WithValidValue_ShouldReturnCorrectEnumeration()
    {
        TestEnumeration result = Enumeration.FromValue<TestEnumeration>(2);

        result.Should().Be(TestEnumeration.Second);
    }

    [Fact]
    public void FromValue_WithInvalidValue_ShouldThrowInvalidOperationException()
    {
        Action act = () => Enumeration.FromValue<TestEnumeration>(99);

        act.Should().Throw<InvalidOperationException>()
            .WithMessage("'99' is not a valid value in Common.Domain.Tests.ValueObjects.EnumerationTests+TestEnumeration");
    }

    [Fact]
    public void FromDisplayName_WithValidName_ShouldReturnCorrectEnumeration()
    {
        TestEnumeration result = Enumeration.FromDisplayName<TestEnumeration>("Second");

        result.Should().Be(TestEnumeration.Second);
    }

    [Fact]
    public void FromDisplayName_WithInvalidName_ShouldThrowInvalidOperationException()
    {
        Action act = () => Enumeration.FromDisplayName<TestEnumeration>("Invalid");

        act.Should().Throw<InvalidOperationException>()
            .WithMessage("'Invalid' is not a valid display name in Common.Domain.Tests.ValueObjects.EnumerationTests+TestEnumeration");
    }

    [Fact]
    public void CompareTo_WithNull_ShouldReturn1()
    {
        TestEnumeration enumeration = TestEnumeration.First;

        int result = enumeration.CompareTo(null);

        result.Should().Be(1);
    }


    [Fact]
    public void CompareTo_WithSmallerId_ShouldReturnPositive()
    {
        TestEnumeration second = TestEnumeration.Second;
        TestEnumeration first = TestEnumeration.First;

        int result = second.CompareTo(first);

        result.Should().BeGreaterThan(0);
    }

    [Fact]
    public void CompareTo_WithLargerId_ShouldReturnNegative()
    {
        TestEnumeration first = TestEnumeration.First;
        TestEnumeration second = TestEnumeration.Second;

        int result = first.CompareTo(second);

        result.Should().BeLessThan(0);
    }


    [Fact]
    public void EqualityOperator_WithOneNull_ShouldReturnFalse()
    {
        TestEnumeration? enum1 = null;
        TestEnumeration enum2 = TestEnumeration.First;

        bool result = enum1 == enum2;

        result.Should().BeFalse();
    }

    [Fact]
    public void EqualityOperator_WithSameValue_ShouldReturnTrue()
    {
        TestEnumeration enum1 = TestEnumeration.First;
        TestEnumeration enum2 = TestEnumeration.First;

        bool result = enum1 == enum2;

        result.Should().BeTrue();
    }

    [Fact]
    public void EqualityOperator_WithDifferentValue_ShouldReturnFalse()
    {
        TestEnumeration enum1 = TestEnumeration.First;
        TestEnumeration enum2 = TestEnumeration.Second;

        bool result = enum1 == enum2;

        result.Should().BeFalse();
    }

    [Fact]
    public void InequalityOperator_WithSameValue_ShouldReturnFalse()
    {
        TestEnumeration enum1 = TestEnumeration.First;
        TestEnumeration enum2 = TestEnumeration.First;

        bool result = enum1 != enum2;

        result.Should().BeFalse();
    }

    [Fact]
    public void InequalityOperator_WithDifferentValue_ShouldReturnTrue()
    {
        TestEnumeration enum1 = TestEnumeration.First;
        TestEnumeration enum2 = TestEnumeration.Second;

        bool result = enum1 != enum2;

        result.Should().BeTrue();
    }

    [Fact]
    public void LessThanOperator_ShouldWorkCorrectly()
    {
        TestEnumeration first = TestEnumeration.First;
        TestEnumeration second = TestEnumeration.Second;

        bool result1 = first < second;
        bool result2 = second < first;
#pragma warning disable CS1718
        bool result3 = first < first;
#pragma warning restore CS1718

        result1.Should().BeTrue();
        result2.Should().BeFalse();
        result3.Should().BeFalse();
    }

    [Fact]
    public void LessThanOrEqualOperator_ShouldWorkCorrectly()
    {
        TestEnumeration first = TestEnumeration.First;
        TestEnumeration second = TestEnumeration.Second;

        bool result1 = first <= second;
        bool result2 = second <= first;
#pragma warning disable CS1718
        bool result3 = first <= first;
#pragma warning restore CS1718

        result1.Should().BeTrue();
        result2.Should().BeFalse();
        result3.Should().BeTrue();
    }

    [Fact]
    public void GreaterThanOperator_ShouldWorkCorrectly()
    {
        TestEnumeration first = TestEnumeration.First;
        TestEnumeration second = TestEnumeration.Second;

        bool result1 = second > first;
        bool result2 = first > second;
#pragma warning disable CS1718
        bool result3 = first > first;
#pragma warning restore CS1718

        result1.Should().BeTrue();
        result2.Should().BeFalse();
        result3.Should().BeFalse();
    }

    [Fact]
    public void GreaterThanOrEqualOperator_ShouldWorkCorrectly()
    {
        TestEnumeration first = TestEnumeration.First;
        TestEnumeration second = TestEnumeration.Second;

        bool result1 = second >= first;
        bool result2 = first >= second;
#pragma warning disable CS1718
        bool result3 = first >= first;
#pragma warning restore CS1718

        result1.Should().BeTrue();
        result2.Should().BeFalse();
        result3.Should().BeTrue();
    }

    [Fact]
    public void Enumeration_ShouldHaveCorrectProperties()
    {
        TestEnumeration enumeration = TestEnumeration.Second;

        enumeration.Id.Should().Be(2);
        enumeration.Name.Should().Be("Second");
    }

    [Fact]
    public void TryFromValue_WithValidValue_ShouldReturnTrueAndCorrectEnumeration()
    {
        bool result = Enumeration.TryFromValue<TestEnumeration>(2, out TestEnumeration? enumeration);

        result.Should().BeTrue();
        enumeration.Should().Be(TestEnumeration.Second);
    }

    [Fact]
    public void TryFromValue_WithInvalidValue_ShouldReturnFalseAndNull()
    {
        bool result = Enumeration.TryFromValue<TestEnumeration>(99, out TestEnumeration? enumeration);

        result.Should().BeFalse();
        enumeration.Should().BeNull();
    }

    [Fact]
    public void TryFromDisplayName_WithValidName_ShouldReturnTrueAndCorrectEnumeration()
    {
        bool result = Enumeration.TryFromDisplayName<TestEnumeration>("Second", out TestEnumeration? enumeration);

        result.Should().BeTrue();
        enumeration.Should().Be(TestEnumeration.Second);
    }

    [Fact]
    public void TryFromDisplayName_WithInvalidName_ShouldReturnFalseAndNull()
    {
        bool result = Enumeration.TryFromDisplayName<TestEnumeration>("Invalid", out TestEnumeration? enumeration);

        result.Should().BeFalse();
        enumeration.Should().BeNull();
    }

}

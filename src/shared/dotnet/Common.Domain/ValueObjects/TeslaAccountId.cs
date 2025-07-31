using Common.Domain.Base;

namespace Common.Domain.ValueObjects;

public sealed class TeslaAccountId : ValueObject
{
    public string Value { get; }

    private TeslaAccountId(string value)
    {
        Value = value;
    }

    public static TeslaAccountId Create(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new ArgumentException("Tesla account ID cannot be empty", nameof(value));

        if (value.Length > 100)
            throw new ArgumentException("Tesla account ID cannot exceed 100 characters", nameof(value));

        return new TeslaAccountId(value);
    }

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Value;
    }

    public override string ToString() => Value;

    public static implicit operator string(TeslaAccountId id) => id.Value;
}

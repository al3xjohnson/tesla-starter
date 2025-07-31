using Common.Domain.Base;

namespace TeslaStarter.Domain.Users;

public sealed class ExternalId : ValueObject
{
    public string Value { get; }

    private ExternalId(string value)
    {
        Value = value;
    }

    public static ExternalId Create(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new ArgumentException("External ID cannot be empty", nameof(value));

        if (value.Length > 255)
            throw new ArgumentException("External ID cannot exceed 255 characters", nameof(value));

        return new ExternalId(value.Trim());
    }

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Value;
    }

    public override string ToString() => Value;

    public static implicit operator string(ExternalId id) => id.Value;
}

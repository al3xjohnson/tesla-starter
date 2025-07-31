using System.Text.RegularExpressions;
using Common.Domain.Base;

namespace TeslaStarter.Domain.Users;

public sealed partial class Email : ValueObject
{
    private static readonly Regex _emailRegex = EmailValidationRegex();

    public string Value { get; }

    private Email(string value)
    {
        Value = value.ToLowerInvariant();
    }

    public static Email Create(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new ArgumentException("Email cannot be empty", nameof(value));

        string trimmedValue = value.Trim();

        if (!_emailRegex.IsMatch(trimmedValue))
            throw new ArgumentException("Invalid email format", nameof(value));

        return new Email(trimmedValue);
    }

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Value;
    }

    public override string ToString() => Value;

    public static implicit operator string(Email email) => email.Value;

    [GeneratedRegex(@"^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$", RegexOptions.Compiled)]
    private static partial Regex EmailValidationRegex();
}

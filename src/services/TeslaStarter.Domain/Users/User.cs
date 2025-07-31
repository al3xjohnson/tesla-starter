using System.Diagnostics.CodeAnalysis;
using Common.Domain.Base;
using TeslaStarter.Domain.Users.Events;

namespace TeslaStarter.Domain.Users;

public sealed class User : AggregateRoot<UserId>
{
    public ExternalId ExternalId { get; private set; }
    public Email Email { get; private set; }
    public string? DisplayName { get; private set; }
    public DateTime CreatedAt { get; }
    public DateTime? LastLoginAt { get; private set; }
    public TeslaAccount? TeslaAccount { get; private set; }

    private User(
        UserId id,
        ExternalId externalId,
        Email email,
        string? displayName,
        DateTime createdAt,
        DateTime? lastLoginAt = null,
        TeslaAccount? teslaAccount = null) : base(id)
    {
        ExternalId = externalId;
        Email = email;
        DisplayName = displayName;
        CreatedAt = createdAt;
        LastLoginAt = lastLoginAt;
        TeslaAccount = teslaAccount;
    }

    [ExcludeFromCodeCoverage(Justification = "Needed for EF Core")]
    private User()
    {
        ExternalId = default!;
        Email = default!;
        CreatedAt = default;
    }

    public static User Create(string externalId, string email, string? displayName = null)
    {
        ExternalId extId = ExternalId.Create(externalId);
        Email emailValue = Email.Create(email);

        User user = new(
            UserId.New(),
            extId,
            emailValue,
            displayName,
            DateTime.UtcNow);

        user.RaiseDomainEvent(new UserCreatedDomainEvent(user.Id, user.ExternalId.Value, user.Email.Value));

        return user;
    }

    public void UpdateProfile(string email, string? displayName)
    {
        Email newEmail = Email.Create(email);

        Email? oldEmail = !Email.Equals(newEmail) ? Email : null;
        string? oldDisplayName = DisplayName != displayName ? DisplayName : null;

        Email = newEmail;
        DisplayName = displayName;

        if (oldEmail != null || oldDisplayName != null)
        {
            RaiseDomainEvent(new UserProfileUpdatedDomainEvent(Id, oldEmail?.Value, newEmail.Value, oldDisplayName, displayName));
        }
    }

    public void RecordLogin()
    {
        LastLoginAt = DateTime.UtcNow;
        RaiseDomainEvent(new UserLoggedInDomainEvent(Id, LastLoginAt.Value));
    }

    public void LinkTeslaAccount(string teslaAccountId)
    {
        if (TeslaAccount != null && TeslaAccount.IsActive)
            throw new InvalidOperationException("User already has an active Tesla account linked");

        TeslaAccount = TeslaAccount.Create(teslaAccountId);
        RaiseDomainEvent(new TeslaAccountLinkedDomainEvent(Id, teslaAccountId));
    }

    public void UnlinkTeslaAccount()
    {
        if (TeslaAccount == null)
            throw new InvalidOperationException("No Tesla account linked");

        TeslaAccountId teslaAccountId = TeslaAccount.TeslaAccountId;
        TeslaAccount = TeslaAccount.Deactivate();
        RaiseDomainEvent(new TeslaAccountUnlinkedDomainEvent(Id, teslaAccountId.Value));
    }

    public void UpdateTeslaTokens(string accessToken, string refreshToken, DateTime expiresAt)
    {
        if (TeslaAccount == null)
            throw new InvalidOperationException("No Tesla account linked");

        if (!TeslaAccount.IsActive)
            throw new InvalidOperationException("Tesla account is not active");

        TeslaAccount = TeslaAccount.UpdateTokens(accessToken, refreshToken, expiresAt);
    }

    public void UpdateTeslaRefreshToken(string refreshToken)
    {
        if (TeslaAccount == null)
            throw new InvalidOperationException("No Tesla account linked");

        if (!TeslaAccount.IsActive)
            throw new InvalidOperationException("Tesla account is not active");

        TeslaAccount = TeslaAccount.UpdateRefreshToken(refreshToken);
    }

    public void ReactivateTeslaAccount()
    {
        if (TeslaAccount == null)
            throw new InvalidOperationException("No Tesla account linked");

        if (TeslaAccount.IsActive)
            throw new InvalidOperationException("Tesla account is already active");

        TeslaAccount = TeslaAccount.Reactivate();
        RaiseDomainEvent(new TeslaAccountReactivatedDomainEvent(Id, TeslaAccount.TeslaAccountId.Value));
    }
}

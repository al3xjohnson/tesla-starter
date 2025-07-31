using Common.Domain.Base;

namespace TeslaStarter.Domain.Users;

public sealed class TeslaAccount : ValueObject
{
    public TeslaAccountId TeslaAccountId { get; }
    public DateTime LinkedAt { get; }
    public bool IsActive { get; private set; }
    public string? RefreshToken { get; private set; }
    public string? AccessToken { get; private set; }
    public DateTime? TokenExpiresAt { get; private set; }
    public DateTime? LastSyncedAt { get; private set; }

    private TeslaAccount(
        TeslaAccountId teslaAccountId,
        DateTime linkedAt,
        bool isActive = true,
        string? refreshToken = null,
        string? accessToken = null,
        DateTime? tokenExpiresAt = null,
        DateTime? lastSyncedAt = null)
    {
        TeslaAccountId = teslaAccountId;
        LinkedAt = linkedAt;
        IsActive = isActive;
        RefreshToken = refreshToken;
        AccessToken = accessToken;
        TokenExpiresAt = tokenExpiresAt;
        LastSyncedAt = lastSyncedAt;
    }

    public static TeslaAccount Create(string teslaAccountId)
    {
        return new TeslaAccount(
            TeslaAccountId.Create(teslaAccountId),
            DateTime.UtcNow);
    }

    public TeslaAccount UpdateTokens(string accessToken, string refreshToken, DateTime expiresAt)
    {
        return new TeslaAccount(
            TeslaAccountId,
            LinkedAt,
            IsActive,
            refreshToken,
            accessToken,
            expiresAt,
            DateTime.UtcNow);
    }

    public TeslaAccount UpdateRefreshToken(string refreshToken)
    {
        return new TeslaAccount(
            TeslaAccountId,
            LinkedAt,
            IsActive,
            refreshToken,
            AccessToken,
            TokenExpiresAt,
            DateTime.UtcNow);
    }

    public TeslaAccount Deactivate()
    {
        return new TeslaAccount(
            TeslaAccountId,
            LinkedAt,
            false,
            null,
            null,
            null,
            LastSyncedAt);
    }

    public TeslaAccount Reactivate()
    {
        return new TeslaAccount(
            TeslaAccountId,
            LinkedAt,
            true,
            RefreshToken,
            AccessToken,
            TokenExpiresAt,
            LastSyncedAt);
    }

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return TeslaAccountId;
        yield return LinkedAt;
        yield return IsActive;
    }
}

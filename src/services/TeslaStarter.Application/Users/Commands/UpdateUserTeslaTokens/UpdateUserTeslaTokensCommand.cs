namespace TeslaStarter.Application.Users.Commands.UpdateUserTeslaTokens;

public record UpdateUserTeslaTokensCommand : IRequest
{
    public string ExternalId { get; init; } = string.Empty;
    public string AccessToken { get; init; } = string.Empty;
    public string RefreshToken { get; init; } = string.Empty;
    public DateTime ExpiresAt { get; init; }
    public string TeslaAccountId { get; init; } = string.Empty;
}

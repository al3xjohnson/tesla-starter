namespace TeslaStarter.Infrastructure.Configuration;

public class TeslaOptions
{
    public const string SectionName = "Tesla";

    public string ClientId { get; set; } = string.Empty;
    public string ClientSecret { get; set; } = string.Empty;
    public required Uri RedirectUri { get; set; }

    public void Validate()
    {
        if (string.IsNullOrWhiteSpace(ClientId))
            throw new InvalidOperationException($"{nameof(ClientId)} is required in {SectionName} configuration");

        if (string.IsNullOrWhiteSpace(ClientSecret))
            throw new InvalidOperationException($"{nameof(ClientSecret)} is required in {SectionName} configuration");

        if (RedirectUri is null)
            throw new InvalidOperationException($"{nameof(RedirectUri)} is required in {SectionName} configuration");
    }
}

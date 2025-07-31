namespace TeslaStarter.Application.Users.DTOs;

public record TeslaAccountDto
{
    public string TeslaAccountId { get; init; } = string.Empty;
    public DateTime LinkedAt { get; init; }
    public bool IsActive { get; init; }
    public DateTime? LastSyncedAt { get; init; }
}

namespace TeslaStarter.Application.Users.DTOs;

public record UserDto
{
    public Guid Id { get; init; }
    public string ExternalId { get; init; } = string.Empty;
    public string DescopeUserId { get; init; } = string.Empty;
    public string Email { get; init; } = string.Empty;
    public string? DisplayName { get; init; }
    public string Name => DisplayName ?? Email;
    public DateTime CreatedAt { get; init; }
    public DateTime? LastLoginAt { get; init; }
    public DateTime UpdatedAt => LastLoginAt ?? CreatedAt;
    public TeslaAccountDto? TeslaAccount { get; init; }
}

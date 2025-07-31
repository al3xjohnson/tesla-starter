using TeslaStarter.Application.Users.DTOs;

namespace TeslaStarter.Application.Users.Commands.CreateUser;

public record CreateUserCommand : IRequest<UserDto>
{
    public string ExternalId { get; init; } = string.Empty;
    public string Email { get; init; } = string.Empty;
    public string? DisplayName { get; init; }
}

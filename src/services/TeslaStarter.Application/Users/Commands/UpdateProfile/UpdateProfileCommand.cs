using TeslaStarter.Application.Users.DTOs;

namespace TeslaStarter.Application.Users.Commands.UpdateProfile;

public record UpdateProfileCommand : IRequest<UserDto>
{
    public Guid UserId { get; init; }
    public string Email { get; init; } = string.Empty;
    public string? DisplayName { get; init; }
}

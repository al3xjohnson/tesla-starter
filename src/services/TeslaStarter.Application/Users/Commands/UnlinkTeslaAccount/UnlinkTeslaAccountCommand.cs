using TeslaStarter.Application.Users.DTOs;

namespace TeslaStarter.Application.Users.Commands.UnlinkTeslaAccount;

public record UnlinkTeslaAccountCommand : IRequest<UserDto>
{
    public Guid UserId { get; init; }
}

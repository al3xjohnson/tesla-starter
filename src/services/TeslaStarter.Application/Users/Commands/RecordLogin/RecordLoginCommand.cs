using TeslaStarter.Application.Users.DTOs;

namespace TeslaStarter.Application.Users.Commands.RecordLogin;

public record RecordLoginCommand : IRequest<UserDto>
{
    public Guid UserId { get; init; }
}

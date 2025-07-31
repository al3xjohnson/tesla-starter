using TeslaStarter.Application.Users.DTOs;

namespace TeslaStarter.Application.Users.Commands.LinkTeslaAccount;

public record LinkTeslaAccountCommand : IRequest<UserDto>
{
    public Guid UserId { get; init; }
    public string TeslaAccountId { get; init; } = string.Empty;
}

using TeslaStarter.Application.Users.DTOs;

namespace TeslaStarter.Application.Users.Queries.GetUserByExternalId;

public record GetUserByExternalIdQuery : IRequest<UserDto?>
{
    public string ExternalId { get; init; } = string.Empty;
}

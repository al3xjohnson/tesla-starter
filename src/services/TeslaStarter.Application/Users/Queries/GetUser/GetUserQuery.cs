using TeslaStarter.Application.Users.DTOs;

namespace TeslaStarter.Application.Users.Queries.GetUser;

public record GetUserQuery : IRequest<UserDto>
{
    public Guid UserId { get; init; }
}

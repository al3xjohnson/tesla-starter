using TeslaStarter.Application.Users.DTOs;

namespace TeslaStarter.Application.Users.Queries.GetUsers;

public record GetUsersQuery : IRequest<List<UserDto>>;

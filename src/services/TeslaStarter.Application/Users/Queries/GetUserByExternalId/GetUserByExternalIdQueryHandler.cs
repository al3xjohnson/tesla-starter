using TeslaStarter.Application.Users.DTOs;

namespace TeslaStarter.Application.Users.Queries.GetUserByExternalId;

public sealed class GetUserByExternalIdQueryHandler(
    IUserRepository userRepository,
    IMapper mapper) : IRequestHandler<GetUserByExternalIdQuery, UserDto?>
{
    public async Task<UserDto?> Handle(GetUserByExternalIdQuery request, CancellationToken cancellationToken)
    {
        User? user = await userRepository.GetByExternalIdAsync(
            ExternalId.Create(request.ExternalId),
            cancellationToken);

        return user != null ? mapper.Map<UserDto>(user) : null;
    }
}

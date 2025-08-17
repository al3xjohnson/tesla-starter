using TeslaStarter.Application.Users.DTOs;

namespace TeslaStarter.Application.Users.Queries.GetUserByExternalId;

public sealed class GetUserByExternalIdQueryHandler(
    IUserRepository userRepository,
    IMapper mapper) : IRequestHandler<GetUserByExternalIdQuery, UserDto?>
{
    private readonly IUserRepository _userRepository = userRepository;
    private readonly IMapper _mapper = mapper;
    public async Task<UserDto?> Handle(GetUserByExternalIdQuery request, CancellationToken cancellationToken)
    {
        User? user = await _userRepository.GetByExternalIdAsync(
            ExternalId.Create(request.ExternalId),
            cancellationToken);

        return user != null ? _mapper.Map<UserDto>(user) : null;
    }
}

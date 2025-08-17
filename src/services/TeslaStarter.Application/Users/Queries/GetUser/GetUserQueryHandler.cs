using TeslaStarter.Application.Common.Exceptions;
using TeslaStarter.Application.Users.DTOs;

namespace TeslaStarter.Application.Users.Queries.GetUser;

public sealed class GetUserQueryHandler(
    IUserRepository userRepository,
    IMapper mapper) : IRequestHandler<GetUserQuery, UserDto>
{
    private readonly IUserRepository _userRepository = userRepository;
    private readonly IMapper _mapper = mapper;
    public async Task<UserDto> Handle(GetUserQuery request, CancellationToken cancellationToken)
    {
        User user = await _userRepository.GetByIdAsync(
            new UserId(request.UserId),
            cancellationToken) ?? throw new NotFoundException(nameof(User), request.UserId);

        return _mapper.Map<UserDto>(user);
    }
}

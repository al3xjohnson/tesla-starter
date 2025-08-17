using TeslaStarter.Application.Common.Exceptions;
using TeslaStarter.Application.Users.DTOs;

namespace TeslaStarter.Application.Users.Commands.RecordLogin;

public sealed class RecordLoginCommandHandler(
    IUserRepository userRepository,
    IUnitOfWork unitOfWork,
    IMapper mapper,
    ILogger<RecordLoginCommandHandler> logger) : IRequestHandler<RecordLoginCommand, UserDto>
{
    private readonly IUserRepository _userRepository = userRepository;
    private readonly IUnitOfWork _unitOfWork = unitOfWork;
    private readonly IMapper _mapper = mapper;
    private readonly ILogger<RecordLoginCommandHandler> _logger = logger;
    public async Task<UserDto> Handle(RecordLoginCommand request, CancellationToken cancellationToken)
    {
        User? user = await _userRepository.GetByIdAsync(
            new UserId(request.UserId),
            cancellationToken) ?? throw new NotFoundException(nameof(User), request.UserId);

        user.RecordLogin();

        _userRepository.Update(user);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Recorded login for user {UserId}", user.Id.Value);

        return _mapper.Map<UserDto>(user);
    }
}

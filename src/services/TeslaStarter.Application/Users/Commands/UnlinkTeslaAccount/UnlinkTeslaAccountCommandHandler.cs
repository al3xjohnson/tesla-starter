using TeslaStarter.Application.Common.Exceptions;
using TeslaStarter.Application.Users.DTOs;

namespace TeslaStarter.Application.Users.Commands.UnlinkTeslaAccount;

public sealed class UnlinkTeslaAccountCommandHandler(
    IUserRepository userRepository,
    IUnitOfWork unitOfWork,
    IMapper mapper,
    ILogger<UnlinkTeslaAccountCommandHandler> logger) : IRequestHandler<UnlinkTeslaAccountCommand, UserDto>
{
    private readonly IUserRepository _userRepository = userRepository;
    private readonly IUnitOfWork _unitOfWork = unitOfWork;
    private readonly IMapper _mapper = mapper;
    private readonly ILogger<UnlinkTeslaAccountCommandHandler> _logger = logger;
    public async Task<UserDto> Handle(UnlinkTeslaAccountCommand request, CancellationToken cancellationToken)
    {
        User user = await _userRepository.GetByIdAsync(
            new UserId(request.UserId),
            cancellationToken) ?? throw new NotFoundException(nameof(User), request.UserId);

        try
        {
            user.UnlinkTeslaAccount();
        }
        catch (InvalidOperationException ex)
        {
            throw new Common.Exceptions.ValidationException([
                new FluentValidation.Results.ValidationFailure(
                    "TeslaAccount",
                    ex.Message)
            ]);
        }

        _userRepository.Update(user);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Unlinked Tesla account from user {UserId}", user.Id.Value);

        return _mapper.Map<UserDto>(user);
    }
}

using TeslaStarter.Application.Common.Exceptions;
using TeslaStarter.Application.Users.DTOs;

namespace TeslaStarter.Application.Users.Commands.LinkTeslaAccount;

public sealed class LinkTeslaAccountCommandHandler(
    IUserRepository userRepository,
    IUnitOfWork unitOfWork,
    IMapper mapper,
    ILogger<LinkTeslaAccountCommandHandler> logger) : IRequestHandler<LinkTeslaAccountCommand, UserDto>
{
    private readonly IUserRepository _userRepository = userRepository;
    private readonly IUnitOfWork _unitOfWork = unitOfWork;
    private readonly IMapper _mapper = mapper;
    private readonly ILogger<LinkTeslaAccountCommandHandler> _logger = logger;
    public async Task<UserDto> Handle(LinkTeslaAccountCommand request, CancellationToken cancellationToken)
    {
        User? user = await _userRepository.GetByIdAsync(
            new UserId(request.UserId),
            cancellationToken) ?? throw new NotFoundException(nameof(User), request.UserId);

        try
        {
            user.LinkTeslaAccount(request.TeslaAccountId);
        }
        catch (InvalidOperationException ex)
        {
            throw new Common.Exceptions.ValidationException([
                new FluentValidation.Results.ValidationFailure(
                    nameof(request.TeslaAccountId),
                    ex.Message)
            ]);
        }

        _userRepository.Update(user);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Linked Tesla account {TeslaAccountId} to user {UserId}",
            request.TeslaAccountId, user.Id.Value);

        return _mapper.Map<UserDto>(user);
    }
}

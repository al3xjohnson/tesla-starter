using TeslaStarter.Application.Users.DTOs;

namespace TeslaStarter.Application.Users.Commands.CreateUser;

public sealed class CreateUserCommandHandler(
    IUserRepository userRepository,
    IUnitOfWork unitOfWork,
    IMapper mapper,
    ILogger<CreateUserCommandHandler> logger) : IRequestHandler<CreateUserCommand, UserDto>
{
    private readonly IUserRepository _userRepository = userRepository;
    private readonly IUnitOfWork _unitOfWork = unitOfWork;
    private readonly IMapper _mapper = mapper;
    private readonly ILogger<CreateUserCommandHandler> _logger = logger;

    public async Task<UserDto> Handle(CreateUserCommand request, CancellationToken cancellationToken)
    {
        // Check if user already exists with external ID
        User? existingUserByExternalId = await _userRepository.GetByExternalIdAsync(
            ExternalId.Create(request.ExternalId),
            cancellationToken);

        if (existingUserByExternalId != null)
        {
            throw new Common.Exceptions.ValidationException([
                new FluentValidation.Results.ValidationFailure(
                    nameof(request.ExternalId),
                    "A user with this external ID already exists.")
            ]);
        }

        // Check if user already exists with email
        User? existingUserByEmail = await _userRepository.GetByEmailAsync(
            Email.Create(request.Email),
            cancellationToken);

        if (existingUserByEmail != null)
        {
            throw new Common.Exceptions.ValidationException([
                new FluentValidation.Results.ValidationFailure(
                    nameof(request.Email),
                    "A user with this email already exists.")
            ]);
        }

        // Create the user
        User user = User.Create(
            request.ExternalId,
            request.Email,
            request.DisplayName);

        _userRepository.Add(user);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Created user {UserId} with external ID {ExternalId}",
            user.Id.Value, user.ExternalId.Value);

        return _mapper.Map<UserDto>(user);
    }
}

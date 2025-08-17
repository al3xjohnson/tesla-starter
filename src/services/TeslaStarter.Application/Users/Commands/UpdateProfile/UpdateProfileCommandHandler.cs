using TeslaStarter.Application.Common.Exceptions;
using TeslaStarter.Application.Users.DTOs;

namespace TeslaStarter.Application.Users.Commands.UpdateProfile;

public sealed class UpdateProfileCommandHandler(
    IUserRepository userRepository,
    IUnitOfWork unitOfWork,
    IMapper mapper,
    ILogger<UpdateProfileCommandHandler> logger) : IRequestHandler<UpdateProfileCommand, UserDto>
{
    private readonly IUserRepository _userRepository = userRepository;
    private readonly IUnitOfWork _unitOfWork = unitOfWork;
    private readonly IMapper _mapper = mapper;
    private readonly ILogger<UpdateProfileCommandHandler> _logger = logger;
    public async Task<UserDto> Handle(UpdateProfileCommand request, CancellationToken cancellationToken)
    {
        User user = await _userRepository.GetByIdAsync(
            new UserId(request.UserId),
            cancellationToken) ?? throw new NotFoundException(nameof(User), request.UserId);

        // Check if another user already has this email
        User? existingUserWithEmail = await _userRepository.GetByEmailAsync(
            Email.Create(request.Email),
            cancellationToken);

        if (existingUserWithEmail != null && existingUserWithEmail.Id != user.Id)
        {
            throw new Common.Exceptions.ValidationException([
                new FluentValidation.Results.ValidationFailure(
                    nameof(request.Email),
                    "Another user already has this email address.")
            ]);
        }

        user.UpdateProfile(request.Email, request.DisplayName);

        _userRepository.Update(user);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Updated profile for user {UserId}", user.Id.Value);

        return _mapper.Map<UserDto>(user);
    }
}

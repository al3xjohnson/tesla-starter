using TeslaStarter.Application.Common.Exceptions;
using TeslaStarter.Application.Users.DTOs;

namespace TeslaStarter.Application.Users.Commands.UpdateProfile;

public sealed class UpdateProfileCommandHandler(
    IUserRepository userRepository,
    IUnitOfWork unitOfWork,
    IMapper mapper,
    ILogger<UpdateProfileCommandHandler> logger) : IRequestHandler<UpdateProfileCommand, UserDto>
{
    public async Task<UserDto> Handle(UpdateProfileCommand request, CancellationToken cancellationToken)
    {
        User user = await userRepository.GetByIdAsync(
            new UserId(request.UserId),
            cancellationToken) ?? throw new NotFoundException(nameof(User), request.UserId);

        // Check if another user already has this email
        User? existingUserWithEmail = await userRepository.GetByEmailAsync(
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

        userRepository.Update(user);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        logger.LogInformation("Updated profile for user {UserId}", user.Id.Value);

        return mapper.Map<UserDto>(user);
    }
}

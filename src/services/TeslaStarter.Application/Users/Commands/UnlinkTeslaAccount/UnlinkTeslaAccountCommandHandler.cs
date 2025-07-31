using TeslaStarter.Application.Common.Exceptions;
using TeslaStarter.Application.Users.DTOs;

namespace TeslaStarter.Application.Users.Commands.UnlinkTeslaAccount;

public sealed class UnlinkTeslaAccountCommandHandler(
    IUserRepository userRepository,
    IUnitOfWork unitOfWork,
    IMapper mapper,
    ILogger<UnlinkTeslaAccountCommandHandler> logger) : IRequestHandler<UnlinkTeslaAccountCommand, UserDto>
{
    public async Task<UserDto> Handle(UnlinkTeslaAccountCommand request, CancellationToken cancellationToken)
    {
        User user = await userRepository.GetByIdAsync(
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

        userRepository.Update(user);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        logger.LogInformation("Unlinked Tesla account from user {UserId}", user.Id.Value);

        return mapper.Map<UserDto>(user);
    }
}

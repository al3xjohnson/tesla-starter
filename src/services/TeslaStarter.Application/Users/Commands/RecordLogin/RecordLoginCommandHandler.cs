using TeslaStarter.Application.Common.Exceptions;
using TeslaStarter.Application.Users.DTOs;

namespace TeslaStarter.Application.Users.Commands.RecordLogin;

public sealed class RecordLoginCommandHandler(
    IUserRepository userRepository,
    IUnitOfWork unitOfWork,
    IMapper mapper,
    ILogger<RecordLoginCommandHandler> logger) : IRequestHandler<RecordLoginCommand, UserDto>
{
    public async Task<UserDto> Handle(RecordLoginCommand request, CancellationToken cancellationToken)
    {
        User? user = await userRepository.GetByIdAsync(
            new UserId(request.UserId),
            cancellationToken) ?? throw new NotFoundException(nameof(User), request.UserId);

        user.RecordLogin();

        userRepository.Update(user);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        logger.LogInformation("Recorded login for user {UserId}", user.Id.Value);

        return mapper.Map<UserDto>(user);
    }
}

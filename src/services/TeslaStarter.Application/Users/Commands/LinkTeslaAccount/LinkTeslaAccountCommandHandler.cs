using TeslaStarter.Application.Common.Exceptions;
using TeslaStarter.Application.Users.DTOs;

namespace TeslaStarter.Application.Users.Commands.LinkTeslaAccount;

public sealed class LinkTeslaAccountCommandHandler(
    IUserRepository userRepository,
    IUnitOfWork unitOfWork,
    IMapper mapper,
    ILogger<LinkTeslaAccountCommandHandler> logger) : IRequestHandler<LinkTeslaAccountCommand, UserDto>
{
    public async Task<UserDto> Handle(LinkTeslaAccountCommand request, CancellationToken cancellationToken)
    {
        User? user = await userRepository.GetByIdAsync(
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

        userRepository.Update(user);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        logger.LogInformation("Linked Tesla account {TeslaAccountId} to user {UserId}",
            request.TeslaAccountId, user.Id.Value);

        return mapper.Map<UserDto>(user);
    }
}

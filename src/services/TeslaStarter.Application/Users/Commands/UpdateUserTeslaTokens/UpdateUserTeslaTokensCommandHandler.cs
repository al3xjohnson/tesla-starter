namespace TeslaStarter.Application.Users.Commands.UpdateUserTeslaTokens;

public class UpdateUserTeslaTokensCommandHandler(IUserRepository userRepository, IUnitOfWork unitOfWork) : IRequestHandler<UpdateUserTeslaTokensCommand>
{
    private readonly IUserRepository _userRepository = userRepository;
    private readonly IUnitOfWork _unitOfWork = unitOfWork;

    public async Task Handle(UpdateUserTeslaTokensCommand request, CancellationToken cancellationToken)
    {
        ExternalId externalId = ExternalId.Create(request.ExternalId);
        User? user = await _userRepository.GetByExternalIdAsync(externalId, cancellationToken)
            ?? throw new InvalidOperationException($"User with External ID {request.ExternalId} not found");

        // If no Tesla account linked, we need to link it first
        if (user.TeslaAccount == null || !user.TeslaAccount.IsActive)
        {
            user.LinkTeslaAccount(request.TeslaAccountId);
        }

        // Update Tesla tokens
        user.UpdateTeslaTokens(request.AccessToken, request.RefreshToken, request.ExpiresAt);

        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }
}

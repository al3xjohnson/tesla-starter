using Common.Domain.Persistence;
using TeslaStarter.Application.Common.Interfaces;
using TeslaStarter.Domain.Users;

namespace TeslaStarter.Infrastructure.Authentication;

public class UserSynchronizationService(IUserRepository userRepository, IUnitOfWork unitOfWork) : IUserSynchronizationService
{
    private readonly IUserRepository _userRepository = userRepository;
    private readonly IUnitOfWork _unitOfWork = unitOfWork;

    public async Task SynchronizeUserAsync(string descopeUserId, string email, string? name, CancellationToken cancellationToken = default)
    {
        ExternalId externalId = ExternalId.Create(descopeUserId);

        // Check if user already exists
        User? existingUser = await _userRepository.GetByExternalIdAsync(externalId, cancellationToken);

        if (existingUser != null)
        {
            // Update profile if changed
            if (existingUser.Email?.Value != email || existingUser.DisplayName != name)
            {
                existingUser.UpdateProfile(email, name);
                _userRepository.Update(existingUser);
                await _unitOfWork.SaveChangesAsync(cancellationToken);
            }

            return;
        }

        // Create new user
        User newUser = User.Create(descopeUserId, email, name);
        _userRepository.Add(newUser);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }
}

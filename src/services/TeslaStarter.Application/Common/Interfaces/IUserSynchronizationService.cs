namespace TeslaStarter.Application.Common.Interfaces;

public interface IUserSynchronizationService
{
    Task SynchronizeUserAsync(string descopeUserId, string email, string? name, CancellationToken cancellationToken = default);
}

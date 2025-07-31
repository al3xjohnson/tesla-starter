using Microsoft.EntityFrameworkCore;
using TeslaStarter.Domain.Users;

namespace TeslaStarter.Infrastructure.Persistence.Repositories;

public class UserRepository(TeslaStarterDbContext context) : IUserRepository
{
    public async Task<User?> GetByIdAsync<TId>(TId id, CancellationToken cancellationToken = default) where TId : notnull
    {
        if (id is UserId userId)
        {
            return await GetByIdAsync(userId, cancellationToken);
        }
        return null;
    }

    public async Task<User?> GetByIdAsync(UserId id, CancellationToken cancellationToken = default)
    {
        return await context.Users
            .FirstOrDefaultAsync(u => u.Id == id, cancellationToken);
    }

    public async Task<User?> GetByExternalIdAsync(ExternalId externalId, CancellationToken cancellationToken = default)
    {
        return await context.Users
            .FirstOrDefaultAsync(u => u.ExternalId == externalId, cancellationToken);
    }

    public async Task<bool> ExistsAsync(UserId id, CancellationToken cancellationToken = default)
    {
        return await context.Users
            .AnyAsync(u => u.Id == id, cancellationToken);
    }

    public async Task<bool> ExistsByExternalIdAsync(ExternalId externalId, CancellationToken cancellationToken = default)
    {
        return await context.Users
            .AnyAsync(u => u.ExternalId == externalId, cancellationToken);
    }

    public async Task<User?> GetByEmailAsync(Email email, CancellationToken cancellationToken = default)
    {
        return await context.Users
            .FirstOrDefaultAsync(u => u.Email == email, cancellationToken);
    }

    public async Task<bool> ExistsByEmailAsync(Email email, CancellationToken cancellationToken = default)
    {
        return await context.Users
            .AnyAsync(u => u.Email == email, cancellationToken);
    }

    public async Task<List<User>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await context.Users
            .Include(u => u.TeslaAccount)
            .OrderBy(u => u.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public void Add(User user)
    {
        context.Users.Add(user);
    }

    public void Update(User user)
    {
        context.Users.Update(user);
    }

    public void Remove(User user)
    {
        context.Users.Remove(user);
    }
}

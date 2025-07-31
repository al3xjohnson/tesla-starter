namespace Common.Domain.Persistence;

public interface IRepository<T> where T : class
{
    Task<T?> GetByIdAsync<TId>(TId id, CancellationToken cancellationToken = default) where TId : notnull;
    void Add(T entity);
    void Update(T entity);
    void Remove(T entity);
}

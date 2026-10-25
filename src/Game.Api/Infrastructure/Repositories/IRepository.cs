using Game.Api.Infrastructure.Specifications;

namespace Game.Api.Infrastructure.Repositories;

public interface IRepository<T> where T : class
{
    Task<T?> FirstOrDefaultAsync(ISpecification<T> specification, CancellationToken cancellationToken = default);
    Task<List<T>> ListAsync(ISpecification<T> specification, CancellationToken cancellationToken = default);
    Task AddAsync(T entity, CancellationToken cancellationToken = default);

    /// <summary>
    /// Tracks a new entity without saving, so several entities can be saved together with <see cref="SaveChangesAsync"/>.
    /// </summary>
    void Add(T entity);

    /// <summary>
    /// Stops tracking an entity, e.g. a new one whose save failed and will be retried with different values.
    /// </summary>
    void Detach(T entity);

    Task SaveChangesAsync(CancellationToken cancellationToken = default);
}
using Game.Api.Infrastructure.Specifications;
using Microsoft.EntityFrameworkCore;

namespace Game.Api.Infrastructure.Repositories;

public class Repository<T>(GameDbContext context) : IRepository<T> where T : class
{
    public async Task<T?> FirstOrDefaultAsync(ISpecification<T> specification, CancellationToken cancellationToken = default)
    {
        return await Apply(specification).FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<List<T>> ListAsync(ISpecification<T> specification, CancellationToken cancellationToken = default)
    {
        return await Apply(specification).ToListAsync(cancellationToken);
    }

    public async Task AddAsync(T entity, CancellationToken cancellationToken = default)
    {
        await context.Set<T>().AddAsync(entity, cancellationToken);
        await context.SaveChangesAsync(cancellationToken);
    }

    public void Add(T entity)
    {
        context.Set<T>().Add(entity);
    }

    public void Detach(T entity)
    {
        context.Entry(entity).State = EntityState.Detached;
    }

    public async Task SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        await context.SaveChangesAsync(cancellationToken);
    }

    private IQueryable<T> Apply(ISpecification<T> specification)
    {
        IQueryable<T> query = context.Set<T>();

        foreach (var include in specification.Includes)
        {
            query = query.Include(include);
        }

        query = query.Where(specification.Criteria);

        if (specification.OrderBy is not null)
        {
            query = query.OrderBy(specification.OrderBy);
        }

        if (specification.Take is { } take)
        {
            query = query.Take(take);
        }

        return query;
    }
}
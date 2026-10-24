using Game.Api.Infrastructure.Specifications;
using Microsoft.EntityFrameworkCore;

namespace Game.Api.Infrastructure.Repositories;

public class Repository<T>(GameDbContext context) : IRepository<T> where T : class
{
    public async Task<T?> FirstOrDefaultAsync(ISpecification<T> specification, CancellationToken cancellationToken = default)
    {
        return await ApplyIncludes(specification).FirstOrDefaultAsync(specification.Criteria, cancellationToken);
    }

    public async Task<List<T>> ListAsync(ISpecification<T> specification, CancellationToken cancellationToken = default)
    {
        return await ApplyIncludes(specification).Where(specification.Criteria).ToListAsync(cancellationToken);
    }

    public async Task AddAsync(T entity, CancellationToken cancellationToken = default)
    {
        await context.Set<T>().AddAsync(entity, cancellationToken);
        await context.SaveChangesAsync(cancellationToken);
    }

    public async Task SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        await context.SaveChangesAsync(cancellationToken);
    }

    private IQueryable<T> ApplyIncludes(ISpecification<T> specification)
    {
        IQueryable<T> query = context.Set<T>();

        foreach (var include in specification.Includes)
        {
            query = query.Include(include);
        }

        return query;
    }
}
using DatabaseApp.Domain.Models;
using DatabaseApp.Domain.Repositories;
using DatabaseApp.Persistence.DatabaseContext;

namespace DatabaseApp.Persistence.Repositories;

public class GenericRepository<TEntity>(IDatabaseContext context) : IGenericRepository<TEntity> 
    where TEntity : class, IEntity
{
    // ReSharper disable once InconsistentNaming
    protected readonly IDatabaseContext _context = context;

    public async Task AddAsync(TEntity entity, CancellationToken cancellationToken) =>
        await _context.SetEntity<TEntity>().AddAsync(entity, cancellationToken);

    public void Delete(TEntity entity) =>
        _context.SetEntity<TEntity>().Remove(entity);
    
    public void Update(TEntity entity) =>
        _context.SetEntity<TEntity>().Update(entity);
    
    public async Task SaveDbChangesAsync(CancellationToken cancellationToken) =>
        await _context.SaveDbChangesAsync(cancellationToken);
}
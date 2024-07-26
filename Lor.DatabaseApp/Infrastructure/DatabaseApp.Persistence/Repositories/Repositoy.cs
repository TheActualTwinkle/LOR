﻿using DatabaseApp.Domain.Models;
using DatabaseApp.Persistence.DatabaseContext;

namespace DatabaseApp.Persistence.Repositories;

public abstract class RepositoryBase<TEntity>(IDatabaseContext context)
    where TEntity : class, IEntity
{
    protected readonly IDatabaseContext _context = context;

    public async Task SaveDbChangesAsync(CancellationToken cancellationToken) =>
        await _context.SaveDbChangesAsync(cancellationToken);

    public async Task AddAsync(TEntity entity, CancellationToken cancellationToken) =>
        await _context.SetEntity<TEntity>().AddAsync(entity, cancellationToken);

    public void Delete(TEntity entity) =>
        _context.SetEntity<TEntity>().Remove(entity);
}
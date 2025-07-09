using System.Collections.Concurrent;
using DatabaseApp.Domain.Repositories;
using DatabaseApp.Persistence.DatabaseContext;

namespace DatabaseApp.Persistence.UnitOfWorkContext;

public sealed class UnitOfWork(
    IDatabaseContext context,
    IServiceProvider serviceProvider)
    : IUnitOfWork
{
    private bool _disposed;
    
    private readonly ConcurrentDictionary<Type, object> _repositories = new();

    public T GetRepository<T>()
        where T : IRepository
    {
        ObjectDisposedException.ThrowIf(_disposed, nameof(UnitOfWork));

        var type = (typeof(T));

        return (T)_repositories.GetOrAdd(
            type, _ =>
            {
                var repo = (T)serviceProvider.GetService(type)!
                           ?? throw new InvalidOperationException($"Repository {type.Name} is not registered");

                return repo;
            });
    }

    public Task SaveDbChangesAsync(CancellationToken cancellationToken) =>
        context.SaveDbChangesAsync(cancellationToken);

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    ~UnitOfWork() =>
        Dispose(false);

    private void Dispose(bool disposing)
    {
        if (_disposed) 
            return;
        
        if (disposing)
            context.DisposeResources();

        _disposed = true;
    }
}
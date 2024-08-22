using DatabaseApp.Domain.Repositories;
using DatabaseApp.Persistence.DatabaseContext;

namespace DatabaseApp.Persistence.UnitOfWorkContext;

public sealed class UnitOfWork(
    IDatabaseContext context,
    IClassRepository classRepository,
    IGroupRepository groupRepository,
    IQueueRepository queueRepository,
    ISubscriberRepository subscriberRepository,
    IUserRepository userRepository) : IUnitOfWork
{
    private bool _disposed;

    public IClassRepository ClassRepository => classRepository;
    public IGroupRepository GroupRepository => groupRepository;
    public IQueueRepository QueueRepository => queueRepository;
    public ISubscriberRepository SubscriberRepository => subscriberRepository;
    public IUserRepository UserRepository => userRepository;

    public Task SaveDbChangesAsync(CancellationToken cancellationToken)
    {
        return context.SaveDbChangesAsync(cancellationToken);
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    ~UnitOfWork()
    {
        Dispose(false);
    }

    private void Dispose(bool disposing)
    {
        if (_disposed) return;
        if (disposing) context.DisposeResources();

        _disposed = true;
    }
}
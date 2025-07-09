using DatabaseApp.Domain.Models;

namespace DatabaseApp.Domain.Repositories;

public interface IUnitOfWork : IDisposable
{
    public T GetRepository<T>()
        where T : IRepository;

    Task SaveDbChangesAsync(CancellationToken cancellationToken);
}

public interface IRepository;

public interface IGenericRepository<TEntity> where TEntity : class, IEntity
{
    Task AddAsync(TEntity entity, CancellationToken cancellationToken);

    void Delete(TEntity entity);

    void Update(TEntity entity);

    Task SaveDbChangesAsync(CancellationToken cancellationToken);
}

public interface IClassRepository : IGenericRepository<Class>, IRepository
{
    public Task<Class?> GetClassByNameAndDate(string className, DateOnly classDate, CancellationToken cancellationToken);
    
    public Task<Class?> GetClassById(int classId, CancellationToken cancellationToken);

    public Task<List<Class>?> GetClassesByGroupId(int groupId, CancellationToken cancellationToken);
    
    public Task<List<Class>?> GetClassesByGroupName(string groupName, CancellationToken cancellationToken);

    public Task<List<int>> GetOutdatedClassesId(CancellationToken cancellationToken);
}

public interface IGroupRepository : IGenericRepository<Group>, IRepository
{
    public Task<List<Group>?> GetGroups(CancellationToken cancellationToken);

    public Task<Group?> GetGroupByGroupId(int groupId, CancellationToken cancellationToken);

    public Task<Group?> GetGroupByGroupName(string groupName, CancellationToken cancellationToken);
}

public interface IQueueEntryRepository : IGenericRepository<QueueEntry>, IRepository
{
    public Task<int> GetCurrentQueueNum(int classId);
    
    public Task<List<QueueEntry>?> GetQueueByClassId(int classId, CancellationToken cancellationToken);

    public Task<List<QueueEntry>?> GetOutdatedQueueListByClassId(int classId, CancellationToken cancellationToken);
    
    public Task<uint> GetUserQueueNum(long telegramId, int classId, CancellationToken cancellationToken);
    
    public Task<bool> IsUserInQueue(int userId, int classId, CancellationToken cancellationToken);
}

public interface ISubscriberRepository : IGenericRepository<Subscriber>, IRepository
{
    public Task<List<Subscriber>?> GetAllSubscribers(CancellationToken cancellationToken);
    
    public Task<Subscriber?> GetSubscriberByUserId(int userId, CancellationToken cancellationToken);
}

public interface IUserRepository : IGenericRepository<User>, IRepository
{
    public Task<User?> IsUserExists(long telegramId, string fullName, CancellationToken cancellationToken);

    public Task<User?> GetUserByFullName(string fullName, CancellationToken cancellationToken);
    
    public Task<User?> GetUserByTelegramId(long telegramId, CancellationToken cancellationToken);
}
using DatabaseApp.Domain.Models;

namespace DatabaseApp.Domain.Repositories;

public interface IUnitOfWork : IDisposable
{
    IClassRepository ClassRepository { get; }
    IGroupRepository GroupRepository { get; }
    IQueueEntryRepository QueueEntryRepository { get; }
    ISubscriberRepository SubscriberRepository { get; }
    IUserRepository UserRepository { get; }

    Task SaveDbChangesAsync(CancellationToken cancellationToken);
}

public interface IRepository;

public interface IClassRepository : IRepository
{
    public Task AddAsync(Class @class, CancellationToken cancellationToken);

    public void Delete(Class @class);

    public Task<Class?> GetClassByNameAndDate(string className, DateOnly classDate, CancellationToken cancellationToken);
    
    public Task<Class?> GetClassById(int classId, CancellationToken cancellationToken);

    public Task<List<Class>?> GetClassesByGroupId(int groupId, CancellationToken cancellationToken);
    
    public Task<List<Class>?> GetClassesByGroupName(string groupName, CancellationToken cancellationToken);

    public Task<List<int>> GetOutdatedClassesId(CancellationToken cancellationToken);
    
    public Task<List<Class>?> GetUpcomingClasses(CancellationToken cancellationToken);
}

public interface IGroupRepository : IRepository
{
    public Task AddAsync(Group group, CancellationToken cancellationToken);
    
    public Task<List<Group>?> GetGroups(CancellationToken cancellationToken);

    public Task<Group?> GetGroupByGroupId(int groupId, CancellationToken cancellationToken);

    public Task<Group?> GetGroupByGroupName(string groupName, CancellationToken cancellationToken);
}

public interface IQueueEntryRepository : IRepository
{
    public Task AddAsync(QueueEntry queueEntry, CancellationToken cancellationToken);
    
    public void Delete(QueueEntry queueEntry);

    public Task<int> GetCurrentQueueNum(int classId);
    
    public Task<List<QueueEntry>?> GetQueueByClassId(int classId, CancellationToken cancellationToken);

    public Task<List<QueueEntry>?> GetOutdatedQueueListByClassId(int classId, CancellationToken cancellationToken);
    
    public Task<uint> GetUserQueueNum(long telegramId, int classId, CancellationToken cancellationToken);
    
    public Task<bool> IsUserInQueue(int userId, int classId, CancellationToken cancellationToken);
    
    public void Update(QueueEntry queueEntry);
}

public interface ISubscriberRepository : IRepository
{
    public Task AddAsync(Subscriber subscriber, CancellationToken cancellationToken);
    
    public void Delete(Subscriber subscriber);

    public Task<List<Subscriber>?> GetAllSubscribers(CancellationToken cancellationToken);
    
    public Task<Subscriber?> GetSubscriberByUserId(int userId, CancellationToken cancellationToken);
}

public interface IUserRepository : IRepository
{
    public Task AddAsync(User user, CancellationToken cancellationToken);
   
    public Task<User?> IsUserExists(long telegramId, string fullName, CancellationToken cancellationToken);

    public Task<User?> GetUserByFullName(string fullName, CancellationToken cancellationToken);
    
    public Task<User?> GetUserByTelegramId(long telegramId, CancellationToken cancellationToken);
}
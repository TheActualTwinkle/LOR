using DatabaseApp.Domain.Models;

namespace DatabaseApp.Domain.Repositories;

public interface IUnitOfWork : IDisposable
{
    IClassRepository ClassRepository { get; }
    IGroupRepository GroupRepository { get; }
    IQueueRepository QueueRepository { get; }
    ISubscriberRepository SubscriberRepository { get; }
    IUserRepository UserRepository { get; }

    Task SaveDbChangesAsync(CancellationToken cancellationToken);
}

public interface IRepository;

public interface IClassRepository : IRepository
{
    public Task AddAsync(Class someClass, CancellationToken cancellationToken);

    public Task<bool> CheckClass(string className, DateOnly date, CancellationToken cancellationToken);

    public void Delete(Class someClass);

    public Task<Class?> GetClassById(int classId, CancellationToken cancellationToken);

    public Task<List<Class>?> GetClassesByGroupId(int groupId, CancellationToken cancellationToken);

    public Task<List<int>?> GetOutdatedClassesId(CancellationToken cancellationToken);
}

public interface IGroupRepository : IRepository
{
    public Task AddAsync(Group group, CancellationToken cancellationToken);
    
    public Task<List<Group>?> GetGroups(CancellationToken cancellationToken);

    public Task<Group?> GetGroupByGroupId(int groupId, CancellationToken cancellationToken);

    public Task<Group?> GetGroupByGroupName(string groupName, CancellationToken cancellationToken);
}

public interface IQueueRepository : IRepository
{
    public Task AddAsync(Queue queue, CancellationToken cancellationToken);

    public Task<bool> CheckQueue(int userId, int groupId, int classId, CancellationToken cancellationToken);
    
    public void Delete(Queue queue);

    public Task<int> GetCurrentQueueNum(int groupId, int classId, CancellationToken cancellationToken);
    
    public Task<List<Queue>?> GetQueueList(int groupId, int classId,
        CancellationToken cancellationToken);

    public Task<List<Queue>?> GetUserQueueList(uint queueNum, int groupId, int classId,
        CancellationToken cancellationToken);

    public Task<List<Queue>?> GetOutdatedQueueListByClassId(int classId, CancellationToken cancellationToken);
    
    public Task<uint> GetUserQueueNum(int userId, int groupId, int classId, CancellationToken cancellationToken);
}

public interface ISubscriberRepository : IRepository
{
    public Task AddAsync(Subscriber subscriber, CancellationToken cancellationToken);
    
    public void Delete(Subscriber subscriber);

    public Task<List<Subscriber>> GetAllSubscribers(CancellationToken cancellationToken);
    
    public Task<Subscriber?> GetSubscriberByUserId(int userId, CancellationToken cancellationToken);
}

public interface IUserRepository : IRepository
{
    public Task AddAsync(User user, CancellationToken cancellationToken);
    
    public Task<User?> CheckUser(long telegramId, string fullName, CancellationToken cancellationToken);
    public Task<User?> GetUserByTelegramId(long telegramId, CancellationToken cancellationToken);
}
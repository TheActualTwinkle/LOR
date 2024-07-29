using DatabaseApp.Domain.Models;
using FluentResults;

namespace DatabaseApp.Domain.Repositories;

public interface IUnitOfWork : IDisposable
{
    IClassRepository ClassRepository { get; }
    IGroupRepository GroupRepository { get; }
    IQueueRepository QueueRepository { get; }
    IUserRepository UserRepository { get; }

    Task SaveDbChangesAsync(CancellationToken cancellationToken);
}

public interface IRepository;

public interface IClassRepository : IRepository
{
    public Task AddAsync(Class someClass, CancellationToken cancellationToken);

    public void Delete(Class someClass);
    
    public Task<Dictionary<int, string>?> GetClassesByGroupId(int groupId, CancellationToken cancellationToken);

    public Task<List<Class>?> GetOutdatedClasses(CancellationToken cancellationToken);
}

public interface IGroupRepository : IRepository
{
    public Task AddAsync(Group group, CancellationToken cancellationToken);
    
    public Task<Dictionary<int, string>?> GetGroups(CancellationToken cancellationToken);

    public Task<Group?> GetGroupByGroupId(int groupId, CancellationToken cancellationToken);

    public Task<Group?> GetGroupByGroupName(string groupName, CancellationToken cancellationToken);
}

public interface IQueueRepository : IRepository
{
    public Task AddAsync(Queue queue, CancellationToken cancellationToken);

    public Task<bool> CheckQueue(int userId, int groupId, int classId, CancellationToken cancellationToken);
    
    public void Delete(Queue queue);

    public Task<int> GetCurrentQueueNum(int groupId, int classId, CancellationToken cancellationToken);

    public Task<List<string>?> GetQueueList(uint queueNum, int groupId, int classId,
        CancellationToken cancellationToken);

    public Task<List<Queue>?> GetOutdatedQueueListByClassId(int classId, CancellationToken cancellationToken);
    
    public Task<uint> GetUserQueueNum(int userId, int groupId, int classId, CancellationToken cancellationToken);
}

public interface IUserRepository : IRepository
{
    public Task AddAsync(User user, CancellationToken cancellationToken);

    public Task<User?> GetUserByTelegramId(long telegramId, CancellationToken cancellationToken);
}
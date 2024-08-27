using DatabaseApp.Domain.Models;
using DatabaseApp.Domain.Repositories;
using DatabaseApp.Persistence.DatabaseContext;
using Microsoft.EntityFrameworkCore;

namespace DatabaseApp.Persistence.Repositories;

public class QueueRepository(IDatabaseContext context)
    : RepositoryBase<Queue>(context), IQueueRepository
{
    public async Task<bool> CheckQueue(int userId, int groupId, int classId, CancellationToken cancellationToken) =>
        await _context.Queues
            .AnyAsync(q => q.UserId == userId && q.Class.GroupId == groupId && q.ClassId == classId,cancellationToken);

    public async Task<int> GetCurrentQueueNum(int groupId, int classId, CancellationToken cancellationToken) =>
        await Task.FromResult(_context.Queues
            .Count(q => q.Class.GroupId == groupId && q.ClassId == classId));

    public async Task<List<Queue>?> GetQueueList(int groupId, int classId, CancellationToken cancellationToken) =>
        await _context.Queues
            .Where(q => q.Class.GroupId == groupId && q.ClassId == classId)
            .ToListAsync(cancellationToken);
    
    public async Task<List<Queue>?> GetUserQueueList(uint queueNum, int groupId, int classId,
        CancellationToken cancellationToken) =>
        await _context.Queues
            .Include(q => q.User)
            .Where(q => q.QueueNum <= queueNum && q.Class.GroupId == groupId && q.ClassId == classId)
            .ToListAsync(cancellationToken);

    public async Task<List<Queue>?> GetOutdatedQueueListByClassId(int classId, CancellationToken cancellationToken) =>
        await _context.Queues
            .Where(q => q.ClassId == classId)
            .ToListAsync(cancellationToken);

    public async Task<uint> GetUserQueueNum(int userId, int groupId, int classId, CancellationToken cancellationToken) =>
        await _context.Queues
            .Where(q => q.UserId == userId && q.Class.GroupId == groupId && q.ClassId == classId)
            .Select(q => q.QueueNum)
            .FirstOrDefaultAsync(cancellationToken);

    public async Task<bool> IsUserInQueue(int userId, int classId, CancellationToken cancellationToken) => await _context.Queues
        .AnyAsync(q => q.UserId == userId && q.ClassId == classId, cancellationToken);
}
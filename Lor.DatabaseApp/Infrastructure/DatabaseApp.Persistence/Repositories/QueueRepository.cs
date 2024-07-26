using DatabaseApp.Domain.Models;
using DatabaseApp.Domain.Repositories;
using DatabaseApp.Persistence.DatabaseContext;
using Microsoft.EntityFrameworkCore;

namespace DatabaseApp.Persistence.Repositories;

public class QueueRepository(IDatabaseContext context)
    : RepositoryBase<Queue>(context), IQueueRepository
{
    public async Task<int> GetCurrentQueueNum(int groupId, int classId, CancellationToken cancellationToken) =>
        await Task.FromResult(_context.Queues
            .Count(q => q.GroupId == groupId && q.ClassId == classId));

    public async Task<List<string>?> GetQueueList(uint queueNum, int groupId, int classId,
        CancellationToken cancellationToken) =>
        await _context.Queues
            .Include(q => q.User)
            .Where(q => q.QueueNum < queueNum && q.GroupId == groupId && q.ClassId == classId)
            .Select(q => q.User.FullName)
            .ToListAsync(cancellationToken);

    public async Task<List<Queue>?> GetOutdatedQueueListByClassId(int classId, CancellationToken cancellationToken) =>
        await _context.Queues
            .Where(q => q.ClassId == classId)
            .ToListAsync(cancellationToken);

    public Task<uint> GetUserQueueNum(long telegramId, int groupId, int classId, CancellationToken cancellationToken) =>
        _context.Queues
            .Where(q => q.TelegramId == telegramId && q.GroupId == groupId && q.ClassId == classId)
            .Select(q => q.QueueNum)
            .FirstOrDefaultAsync(cancellationToken);
}
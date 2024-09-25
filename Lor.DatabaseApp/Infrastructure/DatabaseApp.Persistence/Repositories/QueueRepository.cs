using DatabaseApp.Domain.Models;
using DatabaseApp.Domain.Repositories;
using DatabaseApp.Persistence.DatabaseContext;
using Microsoft.EntityFrameworkCore;

namespace DatabaseApp.Persistence.Repositories;

public class QueueRepository(IDatabaseContext context)
    : RepositoryBase<Queue>(context), IQueueRepository
{
    public async Task<int> GetCurrentQueueNum(int classId) =>
        await Task.FromResult(_context.Queues
            .Count(q => q.ClassId == classId));

    public async Task<List<Queue>?> GetQueueList(int classId, CancellationToken cancellationToken) =>
        await _context.Queues
            .Where(q => q.ClassId == classId)
            .ToListAsync(cancellationToken);

    public async Task<List<Queue>?> GetQueueByClassId(int classId, CancellationToken cancellationToken)
    {
        if (_context.Classes.Any(x => x.Id == classId) == false)
        {
            return null;
        }
        
        return await _context.Queues
            .Include(q => q.User)
            .Where(q => q.ClassId == classId)
            .OrderBy(q => q.QueueNum)
            .ToListAsync(cancellationToken);
    }

    public async Task<List<Queue>?> GetOutdatedQueueListByClassId(int classId, CancellationToken cancellationToken) =>
        await _context.Queues
            .Where(q => q.ClassId == classId)
            .ToListAsync(cancellationToken);

    public async Task<bool> IsUserInQueue(int userId, int classId, CancellationToken cancellationToken) => await _context.Queues
        .AnyAsync(q => q.UserId == userId && q.ClassId == classId, cancellationToken);
}
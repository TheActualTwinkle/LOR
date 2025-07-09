using DatabaseApp.Domain.Models;
using DatabaseApp.Domain.Repositories;
using DatabaseApp.Persistence.DatabaseContext;
using Microsoft.EntityFrameworkCore;

namespace DatabaseApp.Persistence.Repositories;

public class SubscriberGenericRepository(IDatabaseContext context)
    : GenericRepository<Subscriber>(context), ISubscriberRepository
{
    public async Task<List<Subscriber>?> GetAllSubscribers(CancellationToken cancellationToken) =>
        await _context.Subscribers
            .Include(s => s.User.Group)
            .ToListAsync(cancellationToken);

    public async Task<Subscriber?> GetSubscriberByUserId(int userId, CancellationToken cancellationToken) =>
        await _context.Subscribers
            .FirstOrDefaultAsync(s => s.UserId == userId, cancellationToken);
}
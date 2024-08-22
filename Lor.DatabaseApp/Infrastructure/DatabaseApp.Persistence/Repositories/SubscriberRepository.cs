using DatabaseApp.Domain.Models;
using DatabaseApp.Domain.Repositories;
using DatabaseApp.Persistence.DatabaseContext;
using Microsoft.EntityFrameworkCore;

namespace DatabaseApp.Persistence.Repositories;

public class SubscriberRepository(IDatabaseContext context)
    : RepositoryBase<Subscriber>(context), ISubscriberRepository
{
    public async Task<List<Subscriber>> GetAllSubscribers(CancellationToken cancellationToken) =>
        await _context.Subscribers
            .ToListAsync(cancellationToken);

    public async Task<Subscriber?> GetSubscriberByTelegramId(long telegramId, CancellationToken cancellationToken) =>
        await _context.Subscribers
            .FirstOrDefaultAsync(s => s.TelegramId == telegramId, cancellationToken);
}
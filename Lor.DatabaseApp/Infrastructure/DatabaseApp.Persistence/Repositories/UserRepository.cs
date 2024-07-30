using DatabaseApp.Domain.Models;
using DatabaseApp.Domain.Repositories;
using DatabaseApp.Persistence.DatabaseContext;
using Microsoft.EntityFrameworkCore;

namespace DatabaseApp.Persistence.Repositories;

public class UserRepository(IDatabaseContext context)
    : RepositoryBase<User>(context), IUserRepository
{
    public Task<User?> CheckUser(long telegramId, string fullName, CancellationToken cancellationToken) =>
        _context.Users
            .FirstOrDefaultAsync(u => u.TelegramId == telegramId || u.FullName == fullName, cancellationToken);

    public Task<User?> GetUserByTelegramId(long telegramId, CancellationToken cancellationToken) =>
        _context.Users
            .FirstOrDefaultAsync(u => u.TelegramId == telegramId, cancellationToken);
}
using DatabaseApp.Domain.Models;
using DatabaseApp.Domain.Repositories;
using DatabaseApp.Persistence.DatabaseContext;
using Microsoft.EntityFrameworkCore;

namespace DatabaseApp.Persistence.Repositories;

public class UserRepository(IDatabaseContext context)
    : GenericRepository<User>(context), IUserRepository
{
    public async Task<User?> IsUserExists(long telegramId, string fullName, CancellationToken cancellationToken) =>
        await _context.Users
            .FirstOrDefaultAsync(u => u.TelegramId == telegramId || u.FullName == fullName, cancellationToken);

    public async Task<User?> GetUserByFullName(string fullName, CancellationToken cancellationToken) =>
        await _context.Users
            .FirstOrDefaultAsync(u => u.FullName == fullName, cancellationToken);
    public async Task<User?> GetUserByTelegramId(long telegramId, CancellationToken cancellationToken) =>
        await _context.Users
            .FirstOrDefaultAsync(u => u.TelegramId == telegramId, cancellationToken);
}
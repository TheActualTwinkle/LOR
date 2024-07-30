using DatabaseApp.Domain.Models;
using DatabaseApp.Domain.Repositories;
using DatabaseApp.Persistence.DatabaseContext;
using Microsoft.EntityFrameworkCore;

namespace DatabaseApp.Persistence.Repositories;

public class ClassRepository(IDatabaseContext context)
    : RepositoryBase<Class>(context), IClassRepository
{
    public async Task<string?> GetClassNameById(int classId, CancellationToken cancellationToken) =>
        await Task.FromResult(_context.Classes
            .Where(c => c.Id == classId)
            .Select(c => c.ClassName).FirstOrDefault()?.ToString());

    public async Task<Dictionary<int, string>?> GetClassesByGroupId(int groupId, CancellationToken cancellationToken) =>
        await _context.Classes
            .Include(c => c.Group)
            .Where(c => c.Group.Id == groupId)
            .ToDictionaryAsync(c => c.Id, c => c.ClassName, cancellationToken);

    public async Task<List<Class>?> GetOutdatedClasses(CancellationToken cancellationToken) =>
        await _context.Classes
            .Where(c => c.Date < DateOnly.FromDateTime(DateTime.Now))
            .ToListAsync(cancellationToken);
}
using DatabaseApp.Domain.Models;
using DatabaseApp.Domain.Repositories;
using DatabaseApp.Persistence.DatabaseContext;
using Microsoft.EntityFrameworkCore;

namespace DatabaseApp.Persistence.Repositories;

public class ClassRepository(IDatabaseContext context)
    : RepositoryBase<Class>(context), IClassRepository
{
    public async Task<Class?> GetClassByNameAndDate(string className, DateOnly classDate, CancellationToken cancellationToken) =>
        await _context.Classes
            .FirstOrDefaultAsync(c => c.Name == className && c.Date == classDate, cancellationToken);

    public async Task<Class?> GetClassById(int classId, CancellationToken cancellationToken) =>
        await _context.Classes
            .FirstOrDefaultAsync(c => c.Id == classId, cancellationToken);

    public async Task<List<Class>?> GetClassesByGroupId(int groupId, CancellationToken cancellationToken) =>
        await _context.Classes
            .Where(c => c.GroupId == groupId)
            .ToListAsync(cancellationToken);

    public async Task<List<Class>?> GetClassesByGroupName(string groupName, CancellationToken cancellationToken) =>
        await _context.Classes
            .Where(c => c.Group.Name == groupName)
            .ToListAsync(cancellationToken);
    
    public async Task<List<int>> GetOutdatedClassesId(CancellationToken cancellationToken) =>
        await _context.Classes
            .Where(c => c.Date < DateOnly.FromDateTime(DateTime.Now))
            .Select(c => c.Id)
            .ToListAsync(cancellationToken);

    public async Task<List<Class>?> GetUpcomingClasses(CancellationToken cancellationToken) => 
        await _context.Classes.Where(c => c.Date == DateOnly.FromDateTime(DateTime.Now.AddDays(1).Date)).ToListAsync(cancellationToken);
}
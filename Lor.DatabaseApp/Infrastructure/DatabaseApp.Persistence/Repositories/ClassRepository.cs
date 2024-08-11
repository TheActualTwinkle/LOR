using DatabaseApp.AppCommunication.Class;
using DatabaseApp.Domain.Models;
using DatabaseApp.Domain.Repositories;
using DatabaseApp.Persistence.DatabaseContext;
using Microsoft.EntityFrameworkCore;

namespace DatabaseApp.Persistence.Repositories;

public class ClassRepository(IDatabaseContext context)
    : RepositoryBase<Class>(context), IClassRepository
{
    public async Task<bool> CheckClass(string className, DateOnly date, CancellationToken cancellationToken) =>
        await _context.Classes
            .AnyAsync(c => c.ClassName == className && c.Date == date, cancellationToken);

    public async Task<Class?> GetClassById(int classId, CancellationToken cancellationToken) =>
        await _context.Classes
            .FirstOrDefaultAsync(c => c.Id == classId, cancellationToken);

    public async Task<List<ClassInfoDto>?> GetClassesByGroupId(int groupId, CancellationToken cancellationToken) =>
        await _context.Classes
            .Where(c => c.GroupId == groupId)
            .Select(c => new ClassInfoDto
            {
                ClassId = c.Id,
                ClassName = c.ClassName,
                ClassDate = ((DateTimeOffset)c.Date.ToDateTime(TimeOnly.MinValue)).ToUnixTimeSeconds()
            })
            .ToListAsync(cancellationToken);
    
public async Task<List<Class>?> GetOutdatedClasses(CancellationToken cancellationToken) =>
        await _context.Classes
            .Where(c => c.Date < DateOnly.FromDateTime(DateTime.Now))
            .ToListAsync(cancellationToken);
}
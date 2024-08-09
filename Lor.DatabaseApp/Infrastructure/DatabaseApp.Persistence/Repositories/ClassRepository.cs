using DatabaseApp.AppCommunication.Grpc;
using DatabaseApp.Domain.Models;
using DatabaseApp.Domain.Repositories;
using DatabaseApp.Persistence.DatabaseContext;
using Google.Protobuf.WellKnownTypes;
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

    public async Task<List<ClassInformation>?> GetClassesByGroupId(int groupId, CancellationToken cancellationToken) =>
        await _context.Classes
            .Where(c => c.Group.Id == groupId)
            .Select(c => new ClassInformation
            {
                ClassId = c.Id,
                ClassName = c.ClassName,
                ClassDate = Timestamp.FromDateTime(c.Date.ToDateTime(TimeOnly.MinValue))
            })
            .ToListAsync(cancellationToken);
    
public async Task<List<Class>?> GetOutdatedClasses(CancellationToken cancellationToken) =>
        await _context.Classes
            .Where(c => c.Date < DateOnly.FromDateTime(DateTime.Now))
            .ToListAsync(cancellationToken);
}
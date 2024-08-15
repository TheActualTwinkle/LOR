using DatabaseApp.Domain.Models;
using DatabaseApp.Domain.Repositories;
using DatabaseApp.Persistence.DatabaseContext;
using Microsoft.EntityFrameworkCore;

namespace DatabaseApp.Persistence.Repositories;

public class GroupRepository(IDatabaseContext context)
    : RepositoryBase<Group>(context), IGroupRepository
{
    public async Task<Group?> GetGroupByGroupName(string groupName, CancellationToken cancellationToken) =>
        await _context.Groups
            .FirstOrDefaultAsync(u => u.Name == groupName, cancellationToken);

    public async Task<Group?> GetGroupByGroupId(int groupId, CancellationToken cancellationToken) => 
        await _context.Groups
            .FirstOrDefaultAsync(g => g.Id == groupId, cancellationToken);

    public async Task<List<Group>?> GetGroups(CancellationToken cancellationToken) => 
        await _context.Groups
            .ToListAsync(cancellationToken);
}
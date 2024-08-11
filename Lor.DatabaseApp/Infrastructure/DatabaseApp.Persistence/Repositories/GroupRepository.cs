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
            .FirstOrDefaultAsync(u => u.GroupName == groupName, cancellationToken);

    public async Task<Group?> GetGroupByGroupId(int groupId, CancellationToken cancellationToken) => 
        await _context.Groups
            .FirstOrDefaultAsync(g => g.Id == groupId, cancellationToken);

    public async Task<Dictionary<int, string>?> GetGroups(CancellationToken cancellationToken) => 
        await _context.Groups
            .ToDictionaryAsync(g => g.Id, g => g.GroupName, cancellationToken);
}
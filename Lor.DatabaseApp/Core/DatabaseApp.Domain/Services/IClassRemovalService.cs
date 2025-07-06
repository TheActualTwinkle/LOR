using DatabaseApp.Domain.Models;

namespace DatabaseApp.Domain.Services.RemovalService;

public interface IClassRemovalService
{
    public Task ScheduleRemoval(IEnumerable<Class> classes, CancellationToken cancellationToken = default);
}
using DatabaseApp.Domain.Models;

namespace DatabaseApp.Domain.Services.RemovalService;

public interface IClassRemovalService
{
    public Task ScheduleRemoval(IEnumerable<Class> classesDto, CancellationToken cancellationToken = default);
}
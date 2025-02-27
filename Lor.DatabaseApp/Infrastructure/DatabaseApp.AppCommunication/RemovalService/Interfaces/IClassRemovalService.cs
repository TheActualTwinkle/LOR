using DatabaseApp.Application.Class;

namespace DatabaseApp.AppCommunication.RemovalService.Interfaces;

public interface IClassRemovalService
{
    public Task ScheduleRemoval(IEnumerable<ClassDto> classesDto, CancellationToken cancellationToken = default);
}
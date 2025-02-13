namespace DatabaseApp.AppCommunication.RemovalService.Interfaces;

public interface IClassRemovalService
{
    public Task StartAsync(CancellationToken cancellationToken = default);
}
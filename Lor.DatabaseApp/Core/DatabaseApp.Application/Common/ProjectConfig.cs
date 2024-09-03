namespace DatabaseApp.Application.Common;

public class ProjectConfig(TimeSpan defaultCancellationTimeout)
{
    public TimeSpan DefaultCancellationTimeout { get; private set; } = defaultCancellationTimeout;
}
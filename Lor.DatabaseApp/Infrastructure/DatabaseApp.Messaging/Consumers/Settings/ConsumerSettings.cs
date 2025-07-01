namespace DatabaseApp.Messaging.Consumers.Settings;

public class ConsumerSettings
{
    public required TimeSpan DefaultCancellationTimeout { get; init; }
}
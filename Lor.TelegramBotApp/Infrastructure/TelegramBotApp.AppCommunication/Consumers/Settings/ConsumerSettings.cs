namespace TelegramBotApp.AppCommunication.Consumers.Settings;

public class ConsumerSettings(TimeSpan defaultCancellationTimeout)
{
    public TimeSpan DefaultCancellationTimeout { get; private set; } = defaultCancellationTimeout;
}
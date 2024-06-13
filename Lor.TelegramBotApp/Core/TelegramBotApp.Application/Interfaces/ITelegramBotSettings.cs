namespace TelegramBotApp.Application.Interfaces;

public interface ITelegramBotSettings
{
    public TimeSpan Timeout { get; }
    public CancellationToken Token { get; }
}
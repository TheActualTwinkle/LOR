namespace TelegramBotApp.Domain.Models;

public interface ITelegramBotSettings
{
    public TimeSpan Timeout { get; }
    public CancellationToken Token { get; }
}
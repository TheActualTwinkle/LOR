using TelegramBotApp.Domain.Models;

namespace TelegramBotApp.Application.Settings;

public class TelegramBotSettings : ITelegramBotSettings
{
    private readonly CancellationTokenSource _cts;
    public TimeSpan Timeout => TimeSpan.FromSeconds(100);
    public CancellationToken Token => _cts.Token;

    private TelegramBotSettings() => _cts = new CancellationTokenSource(Timeout);
    
    public static ITelegramBotSettings CreateDefault() => new TelegramBotSettings();
}
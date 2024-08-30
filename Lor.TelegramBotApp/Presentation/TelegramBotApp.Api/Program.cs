using TelegramBotApp.Api.AppPipeline;

namespace TelegramBotApp.Api;

public class Program
{
    public static async Task Main(string[] args)
    {
        DefaultAppPipeline pipeline = new();
        await pipeline.Run();
    }
}
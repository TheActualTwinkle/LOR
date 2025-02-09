using Microsoft.Extensions.DependencyInjection;
using TelegramBotApp.Authorization.Interfaces;

namespace TelegramBotApp.Authorization;

public static class DependencyInjection
{
    public static IServiceCollection AddNstuAuthorization(this IServiceCollection services)
    {
        services.AddSingleton<IAuthorizationService, NstuAuthorizationService>();
        return services;
    }
}
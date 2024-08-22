using Microsoft.Extensions.DependencyInjection;
using TelegramBotApp.Authorization.Interfaces;

namespace TelegramBotApp.Authorization;

public static class DependencyInjection
{
    public static IServiceCollection AddAuthorization(this IServiceCollection services)
    {
        services.AddScoped<IAuthorizationService, NstuAuthorizationService>();
        return services;
    }
}
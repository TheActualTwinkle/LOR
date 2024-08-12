using DatabaseApp.Domain.Repositories;
using DatabaseApp.Persistence.UnitOfWorkContext;
using MediatR;
using Microsoft.Extensions.DependencyInjection;

namespace DatabaseApp.AppCommunication;

public static class DependencyInjection
{
    public static IServiceCollection AddCommunication(this IServiceCollection services)
    {
        services.AddScoped<IUnitOfWork, UnitOfWork>();
        services.AddScoped<IMediator, Mediator>();

        return services;
    }
}


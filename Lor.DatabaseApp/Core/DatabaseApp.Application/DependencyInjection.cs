using System.Reflection;
using DatabaseApp.Application.Common.Behaviors;
using DatabaseApp.Application.Common.Mapping;
using DatabaseApp.Domain.Repositories;
using DatabaseApp.Persistence.UnitOfWorkContext;
using FluentValidation;
using Mapster;
using MapsterMapper;
using Microsoft.Extensions.DependencyInjection;

namespace DatabaseApp.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        TypeAdapterConfig? config = new TypeAdapterConfig();
        new RegisterMapper().Register(config);

        services.AddSingleton(config);
        services.AddScoped<IMapper, ServiceMapper>();
        services.AddScoped<IUnitOfWork, UnitOfWork>();
        services.AddMediatR(cfg =>
        {
            cfg.RegisterServicesFromAssembly(Assembly.GetExecutingAssembly());
            cfg.AddOpenBehavior(typeof(ValidationBehavior<,>));
        });
        services.AddValidatorsFromAssembly(Assembly.GetExecutingAssembly());

        return services;
    }
}
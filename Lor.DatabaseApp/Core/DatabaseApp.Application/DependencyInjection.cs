using System.Reflection;
using DatabaseApp.Application.Common.Behaviors;
using DatabaseApp.Application.Common.Mapping;
using DatabaseApp.Application.Services.ReminderService;
using DatabaseApp.Application.Services.ReminderService.Settings;
using DatabaseApp.Application.Services.RemovalService;
using DatabaseApp.Application.Services.RemovalService.Settings;
using DatabaseApp.Domain.Repositories;
using DatabaseApp.Domain.Services.ReminderService;
using DatabaseApp.Domain.Services.RemovalService;
using DatabaseApp.Persistence.UnitOfWorkContext;
using FluentValidation;
using Mapster;
using MapsterMapper;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace DatabaseApp.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        TypeAdapterConfig.GlobalSettings.Apply(new RegisterMapper());
        services.AddSingleton(TypeAdapterConfig.GlobalSettings);

        services.AddScoped<IMapper, ServiceMapper>();
        services.AddScoped<IUnitOfWork, UnitOfWork>();
        services.AddMediatR(cfg =>
        {
            cfg.RegisterServicesFromAssembly(Assembly.GetExecutingAssembly());
            cfg.AddOpenBehavior(typeof(ValidationBehavior<,>));
            cfg.AddOpenBehavior(typeof(TracingBehavior<,>));
        });
        services.AddValidatorsFromAssembly(Assembly.GetExecutingAssembly());

        return services;
    }
    
    public static IServiceCollection AddDomainServices(this IServiceCollection services, IConfiguration configuration)
    {
        AddClassReminderService(services, configuration);

        AddClassRemovalService(services, configuration);

        return services;
    }

    private static void AddClassReminderService(IServiceCollection services, IConfiguration configuration)
    {
        var advanceNoticeTime = configuration
            .GetRequiredSection("ClassReminderServiceSettings")
            .GetValue<TimeSpan>("AdvanceNoticeTime");

        services.AddSingleton(_ => new ClassReminderServiceSettings
        {
            AdvanceNoticeTime = advanceNoticeTime
        });

        services.AddScoped<IClassReminderService, ClassReminderService>();
    }

    private static void AddClassRemovalService(IServiceCollection services, IConfiguration configuration)
    {
        var removalAdvanceTime = configuration
            .GetRequiredSection("ClassRemovalServiceSettings")
            .GetValue<TimeSpan>("RemovalAdvanceTime");

        services.AddSingleton(_ => new ClassRemovalServiceSettings
        {
            RemovalAdvanceTime = removalAdvanceTime
        });
        
        services.AddScoped<IClassRemovalService, ClassRemovalService>();
    }
}
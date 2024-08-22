using DatabaseApp.Domain.Repositories;
using DatabaseApp.Persistence.DatabaseContext;
using DatabaseApp.Persistence.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace DatabaseApp.Persistence;

public static class DependencyInjection
{
    // ReSharper disable once UnusedMethodReturnValue.Global
    public static IServiceCollection AddPersistence(this IServiceCollection services, IConfiguration configuration)
    {
        string? connectionString = configuration.GetConnectionString("DefaultConnection");

        services.AddDbContext<ApplicationDbContext>(options =>
            options.UseNpgsql(connectionString, builder => builder.MigrationsAssembly(typeof(ApplicationDbContext).Assembly.FullName)).EnableSensitiveDataLogging());
        services.AddScoped<IDatabaseContext, ApplicationDbContext>();
        services.AddScoped<IClassRepository, ClassRepository>();
        services.AddScoped<IGroupRepository, GroupRepository>();
        services.AddScoped<IQueueRepository, QueueRepository>();
        services.AddScoped<ISubscriberRepository, SubscriberRepository>();
        services.AddScoped<IUserRepository, UserRepository>();

        return services;
    }
}
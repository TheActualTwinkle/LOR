using DatabaseApp.Domain.Models;
using DatabaseApp.Persistence.EntityTypeConfiguration;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.Configuration;

namespace DatabaseApp.Persistence.DatabaseContext;

public sealed class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
    : DbContext(options), IDatabaseContext
{
    public required DbSet<Class> Classes { get; init; }
    public required DbSet<Group> Groups { get; init; }
    public required DbSet<Queue> Queues { get; init; }
    public required DbSet<Subscriber> Subscribers { get; init; }
    public required DbSet<User> Users { get; init; }
    public DatabaseFacade Db => Database;

    public DbSet<TEntity> SetEntity<TEntity>() where TEntity : class, IEntity => Set<TEntity>();

    public Task SaveDbChangesAsync(CancellationToken cancellationToken) => SaveChangesAsync(cancellationToken);

    public void DisposeResources() => Dispose();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfiguration(new ClassConfiguration());
        modelBuilder.ApplyConfiguration(new GroupConfiguration());
        modelBuilder.ApplyConfiguration(new QueueConfiguration());
        modelBuilder.ApplyConfiguration(new SubscriberConfiguration());
        modelBuilder.ApplyConfiguration(new UserConfiguration());

        base.OnModelCreating(modelBuilder);
    }
}
namespace DatabaseApp.Domain.Models;

public class Group : IEntity
{
    public int Id { get; init; }
    public string Name { get; init; } = null!;
    public virtual ICollection<User> Users { get; init; } = null!;
    public virtual ICollection<Class> Classes { get; init; } = null!;
    public virtual ICollection<Queue> Queues { get; init; } = null!;
}
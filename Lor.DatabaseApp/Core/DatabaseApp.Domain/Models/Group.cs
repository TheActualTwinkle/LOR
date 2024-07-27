namespace DatabaseApp.Domain.Models;

public class Group : IEntity
{
    public int Id { get; init; }
    public string GroupName { get; init; }
    public virtual ICollection<User> Users { get; init; }
    public virtual ICollection<Class> Classes { get; init; }
    public virtual ICollection<Queue> Queues { get; init; }
}
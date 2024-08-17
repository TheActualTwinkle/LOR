namespace DatabaseApp.Domain.Models;

public class Class : IEntity
{    
    public int Id { get; init; }
    public string Name { get; init; } = null!;
    public DateOnly Date { get; init; }
    public int GroupId { get; init; }
    public Group Group { get; init; } = null!;
    public ICollection<Queue> Queues { get; init; } = null!;
}
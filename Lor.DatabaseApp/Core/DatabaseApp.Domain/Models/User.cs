namespace DatabaseApp.Domain.Models;

public class User : IEntity
{
    public int Id { get; init; }
    public string FullName { get; init; }
    public long TelegramId { get; init; }
    public int GroupId { get; init; }
    public Group Group { get; init; } = null!;
    public ICollection<Queue> Queues { get; init; } = null!;
}
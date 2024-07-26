namespace DatabaseApp.Domain.Models;

public class Queue : IEntity
{
    public int Id { get; init; }
    public uint QueueNum { get; init; }
    public int GroupId { get; init; }
    public int ClassId { get; init; }
    public long TelegramId { get; init; }
    public Class Class { get; init; }
    public Group Group { get; init; }
    public User User { get; init; }
}
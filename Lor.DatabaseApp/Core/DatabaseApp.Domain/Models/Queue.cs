namespace DatabaseApp.Domain.Models;

public class Queue : IEntity
{
    public int Id { get; init; }
    public int ClassId { get; init; }
    public int UserId { get; init; }
    public uint QueueNum { get; set; }
    public Class Class { get; init; } = null!;
    public User User { get; init; } = null!;
}
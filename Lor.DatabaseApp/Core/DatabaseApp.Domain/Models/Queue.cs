namespace DatabaseApp.Domain.Models;

public class Queue : IEntity
{
    public int Id { get; init; }
    public int ClassId { get; init; }
    public int UserId { get; init; }
    public uint QueueNum { get; init; }
    public Class Class { get; init; }
    public User User { get; init; }
}
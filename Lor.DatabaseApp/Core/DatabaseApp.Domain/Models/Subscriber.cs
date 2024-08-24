namespace DatabaseApp.Domain.Models;

public class Subscriber : IEntity
{
    public int Id { get; init; }
    public int UserId { get; init; }
    public User User { get; init; } = null!;
}
namespace Lor.Shared.Messaging.Models;

public record ClassModel
{
    public required int Id { get; init; }

    public required string Name { get; init; }

    public required DateOnly Date { get; init; }
}
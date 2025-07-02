namespace DatabaseApp.Application.Classes;

public record ClassDto
{
    public required int Id { get; set; }
    public required string Name { get; set; }
    public required DateOnly Date { get; set; }
}
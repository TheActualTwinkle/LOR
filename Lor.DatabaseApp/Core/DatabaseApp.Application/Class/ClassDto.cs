namespace DatabaseApp.Application.Class;

public struct ClassDto
{
    public required int Id { get; set; }
    public required string Name { get; set; }
    public required DateOnly Date { get; set; }
    public required int GroupId { get; set; }
}
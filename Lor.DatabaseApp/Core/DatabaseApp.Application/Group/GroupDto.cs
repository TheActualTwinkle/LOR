namespace DatabaseApp.Application.Group;

public record GroupDto
{
    public required int Id { get; set; }
    public required string GroupName { get; set; }
}
namespace DatabaseApp.Application.Group;

public struct GroupDto
{
    public int Id { get; set; } //TODO: подумать переделать как сделать без id
    public required string GroupName { get; set; }
}
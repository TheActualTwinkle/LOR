namespace DatabaseApp.Application.Queue;

public struct QueueDto
{
    public required int ClassId { get; set; }
    public required string FullName { get; set; }
    public required uint QueueNum { get; set; }
}
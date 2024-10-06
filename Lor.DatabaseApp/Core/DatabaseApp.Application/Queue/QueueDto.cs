// ReSharper disable UnusedAutoPropertyAccessor.Global
namespace DatabaseApp.Application.Queue;

// ReSharper disable once ClassNeverInstantiated.Global
public record QueueDto
{
    public required int ClassId { get; set; }
    public required string FullName { get; set; }
}
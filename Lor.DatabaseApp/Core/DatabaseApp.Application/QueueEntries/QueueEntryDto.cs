// ReSharper disable UnusedAutoPropertyAccessor.Global
namespace DatabaseApp.Application.QueueEntries;

// ReSharper disable once ClassNeverInstantiated.Global
public record QueueEntryDto
{
    public required int ClassId { get; set; }
    public required string FullName { get; set; }
}
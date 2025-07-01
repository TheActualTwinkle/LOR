namespace DatabaseApp.Application.Services.RemovalService.Settings;

public record ClassRemovalServiceSettings
{
    public required TimeSpan RemovalAdvanceTime { get; init; }
}
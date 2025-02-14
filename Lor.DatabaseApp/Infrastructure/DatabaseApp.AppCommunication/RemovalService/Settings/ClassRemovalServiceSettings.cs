namespace DatabaseApp.AppCommunication.RemovalService.Settings;

public record ClassRemovalServiceSettings
{
    public required TimeSpan RemovalAdvanceTime { get; init; }
}
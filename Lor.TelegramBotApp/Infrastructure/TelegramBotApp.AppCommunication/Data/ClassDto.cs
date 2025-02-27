namespace TelegramBotApp.AppCommunication.Data;

public record ClassDto
{
    public required int Id { get; init; }
    public required string Name { get; init; }
    public required DateOnly Date { get; init; }
}
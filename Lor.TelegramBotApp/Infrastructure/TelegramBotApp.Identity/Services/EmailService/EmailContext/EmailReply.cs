namespace TelegramBotApp.Identity.Services.EmailService.EmailContext;

public record EmailReply
{
    public string Message { get; }
    
    private EmailReply(string message) => Message = message;
    
    public static EmailReply Create(string value) => new(value); 
}
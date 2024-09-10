namespace TelegramBotApp.Identity.Services.RegistrationService.RegistrationContext;

public record RegistrationReply(string Value)
{
    public static RegistrationReply Create(string value) => new(value);
}
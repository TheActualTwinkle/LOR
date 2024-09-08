namespace TelegramBotApp.Identity.Services.RegistrationService.RegistrationContext;

public record RegistrationRequest(string FullName, string Email, long TelegramId);
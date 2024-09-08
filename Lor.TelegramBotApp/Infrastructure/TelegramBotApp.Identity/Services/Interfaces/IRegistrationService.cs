using FluentResults;
using TelegramBotApp.AppCommunication.Interfaces;
using TelegramBotApp.Identity.Services.RegistrationService.RegistrationContext;

namespace TelegramBotApp.Identity.Services.Interfaces;

/// <summary>
/// Represents a service for registration.
/// </summary>
public interface IRegistrationService
{
    /// <summary>
    /// Try to register a new user.
    /// </summary>
    /// <param name="request">The registration request.</param>
    /// <param name="databaseCommunicationClient">The database communication client.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The result of the registration.</returns>
    Task<Result<RegistrationReply>> RegisterAsync(
        RegistrationRequest request,
        IDatabaseCommunicationClient databaseCommunicationClient,
        CancellationToken cancellationToken = default);
}
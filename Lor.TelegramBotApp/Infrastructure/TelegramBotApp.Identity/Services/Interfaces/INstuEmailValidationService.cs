using FluentResults;
using TelegramBotApp.AppCommunication.Interfaces;
using TelegramBotApp.Identity.Services.NstuEmailValidationService.NstuEmailValidationContext;

namespace TelegramBotApp.Identity.Services.Interfaces;

/// <summary>
/// The service for validating the student.
/// </summary>
public interface INstuEmailValidationService
{
    /// <summary>
    /// Validates the student.
    /// </summary>
    /// <param name="request">The request.</param>
    /// <param name="databaseCommunicator">The database communicator.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The validation reply.</returns>
    Task<Result> ValidateAsync(
        NstuValidationRequest request,
        IDatabaseCommunicationClient databaseCommunicator,
        CancellationToken cancellationToken = default);
}
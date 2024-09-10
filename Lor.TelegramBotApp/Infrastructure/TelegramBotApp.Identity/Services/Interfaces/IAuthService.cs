using FluentResults;
using TelegramBotApp.Identity.Services.AuthService.AuthContext;

namespace TelegramBotApp.Identity.Services.Interfaces;

/// <summary>
/// The service for authentication.
/// </summary>
public interface IAuthService
{
    /// <summary>
    /// Authenticates the user.
    /// </summary>
    /// <param name="request">The request.</param>
    /// <returns>The authentication reply.</returns>
    Task<Result<AuthReply>> AuthAsync(AuthRequest request);
}
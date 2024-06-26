using FluentResults;

namespace TelegramBotApp.Authorization.Interfaces;

public interface IAuthorizationService
{
    public Task<Result<AuthorizationReply>> TryAuthorize(AuthorizationRequest request);
}
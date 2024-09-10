using FluentResults;
using TelegramBotApp.Identity.Services.NstuGroupService.NstuGroupContext;

namespace TelegramBotApp.Identity.Services.Interfaces;

/// <summary>
/// The service for getting the student group.
/// </summary>
public interface INstuGroupService
{
    /// <summary>
    /// Returns the student group.
    /// </summary>
    /// <param name="request">The request.</param>
    /// <returns>The group reply.</returns>
    Task<Result<NstuGroupReply>> GetGroupAsync(NstuGroupRequest request);
}
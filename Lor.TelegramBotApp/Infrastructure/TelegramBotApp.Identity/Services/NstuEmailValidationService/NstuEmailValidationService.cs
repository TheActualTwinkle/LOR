using System.Text.RegularExpressions;
using AnyAscii;
using FluentResults;
using FuzzySharp;
using TelegramBotApp.AppCommunication.Interfaces;
using TelegramBotApp.Identity.Services.Interfaces;
using TelegramBotApp.Identity.Services.NstuEmailValidationService.NstuEmailValidationContext;

namespace TelegramBotApp.Identity.Services.NstuEmailValidationService;

public partial class NstuEmailValidationService : INstuEmailValidationService
{
    private const string EmailDomain = "@stud.nstu.ru";
    private const int MatchingRatio = 80;

    public async Task<Result> ValidateAsync(
        NstuValidationRequest request,
        IDatabaseCommunicationClient databaseCommunicator,
        CancellationToken cancellationToken = default)
    {
        var userInfo = await databaseCommunicator.GetUserInfoAsync(request.TelegramId, cancellationToken);
        
        if (userInfo.Value.IsEmailConfirmed)
        {
            return Result.Fail(new Error("Email уже подтвержден"));
        }

        if (!request.Email.EndsWith(EmailDomain, StringComparison.InvariantCultureIgnoreCase))
        {
            return Result.Fail(new Error("Email не принадлежит домену @stud.nstu.ru"));
        }

        var spaceIndex = request.FullName.IndexOf(' ');

        if (Fuzz.Ratio(MyRegex().Replace(request.Email.Split('@')[0], string.Empty),
                request.FullName[..spaceIndex].Transliterate().ToLower()) < MatchingRatio)
        {
            return Result.Fail(new Error("Email не содержит Вашу фамилию"));
        }

        var checkEmailResult = await databaseCommunicator.CheckUserEmailStatusAsync(request.TelegramId, cancellationToken);

        return checkEmailResult.IsFailed ? Result.Fail(checkEmailResult.Errors.First().Message) : Result.Ok();
    }

    [GeneratedRegex(@"[\d.]")]
    private static partial Regex MyRegex();
}
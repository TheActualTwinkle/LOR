using System.Text.Json;
using System.Text.RegularExpressions;
using FluentResults;
using TelegramBotApp.Identity.Services.Interfaces;
using TelegramBotApp.Identity.Services.NstuGroupService.NstuGroupContext;

namespace TelegramBotApp.Identity.Services.NstuGroupService;

/// <summary>
/// The service for getting the student group. See <see cref="INstuGroupService"/>.
/// </summary>
public partial class NstuGroupService(HttpClient client) : INstuGroupService
{
    private const string NstuUrl = "https://id.nstu.ru/user_lookup";
    private const string FullNameShortKey = "fio";
    private const string DateOfBirthKey = "dob";
    private const string FullNameKey = "fullname";
    private const string FullNameAndInfoKey = "fullname_and_info";
    private const string FindUserKey = "find_user";

    /// <summary>
    /// Gets the student group.
    /// </summary>
    /// <param name="request">The request.</param>
    /// <returns>The group reply.</returns>
    public async Task<Result<NstuGroupReply>> GetGroupAsync(NstuGroupRequest request)
    {
        MultipartFormDataContent formData = new();
        formData.Add(new StringContent(request.FullName), FullNameShortKey);
        formData.Add(new StringContent(request.DateOfBirth?.ToString() ?? string.Empty), DateOfBirthKey);
        formData.Add(new StringContent(string.Empty), FindUserKey);

        HttpResponseMessage response = await client.PostAsync(NstuUrl, formData);

        response.EnsureSuccessStatusCode();

        string responseBody = await response.Content.ReadAsStringAsync();

        using JsonDocument document = JsonDocument.Parse(responseBody);
        
        if (document.RootElement.TryGetProperty(FullNameAndInfoKey, out JsonElement fullnameAndInfoElement))
        {
            string fullnameAndInfo = fullnameAndInfoElement.GetString()!;

            fullnameAndInfo = fullnameAndInfo.Replace(request.FullName, string.Empty).Trim();
            Match match = MyRegex().Match(fullnameAndInfo);

            if (match.Success)
            {
                if (document.RootElement.TryGetProperty(FullNameKey, out JsonElement fullNameElement))
                {
                    string? fullNameFromData = fullNameElement.GetString()?.Trim();

                    return fullNameFromData == null
                        ? Result.Fail("Can't get fullname from the response.")
                        : Result.Ok(NstuGroupReply.Create(match.Groups[1].Value.Split(' ').Last(), fullNameFromData));
                }
            }

            Console.WriteLine("Pattern not found in the fullname_and_info.");
        }
        else
        {
            Console.WriteLine("Property 'fullname_and_info' not found in the response.");
        }

        return Result.Fail($"Не удалось получить информацию о студенте {request.FullName}");
    }

    [GeneratedRegex(@"\((.*?)(?:,\s|\))")]
    private static partial Regex MyRegex();
}
using System.Text.Json;
using System.Text.RegularExpressions;
using FluentResults;
using Microsoft.Extensions.Logging;
using TelegramBotApp.Authorization.Dto;
using TelegramBotApp.Authorization.Interfaces;

namespace TelegramBotApp.Authorization;

public partial class NstuAuthorizationService(ILogger<NstuAuthorizationService> logger) : IAuthorizationService
{
    public async Task<Result<AuthorizationReply>> TryAuthorize(AuthorizationRequest request)
    {
        try
        {
            var fullName = request.FullName;
            var dateOfBirth = request.DateOfBirth;

            var result = await ParseUserDto(fullName, dateOfBirth);
            return result.IsSuccess ? Result.Ok(new AuthorizationReply(result.Value.FullName, result.Value.GroupName)) : Result.Fail(result.Errors.First());
        }
        catch (Exception e)
        {
            return Result.Fail($"Error: {e.Message}");
        }
    }
    
    private async Task<Result<UserDto>> ParseUserDto(string fullName, DateTime? dateOfBirth = default)
    {
        using HttpClient client = new();
        
        MultipartFormDataContent formData = new();
        formData.Add(new StringContent(fullName), "fio");
        formData.Add(new StringContent(dateOfBirth.ToString() ?? string.Empty), "dob");
        formData.Add(new StringContent(string.Empty), "find_user");

        var response = await client.PostAsync("https://id.nstu.ru/user_lookup", formData);

        response.EnsureSuccessStatusCode();

        var responseBody = await response.Content.ReadAsStringAsync();

        using var document = JsonDocument.Parse(responseBody);
        if (document.RootElement.TryGetProperty("fullname", out var fullNameElement))
        {
            var fullNameFromData = fullNameElement.GetString()?.Trim();

            if (fullNameFromData == null) return Result.Fail("Can't get fullname from the response.");
            
            fullName = fullNameFromData;
        }
        
        if (document.RootElement.TryGetProperty("fullname_and_info", out var fullnameAndInfoElement))
        {
            var fullnameAndInfo = fullnameAndInfoElement.GetString()!;
            
            fullnameAndInfo = fullnameAndInfo.Replace(fullName, string.Empty).Trim();
            var match = NstuFullNameAndInfoRegex().Match(fullnameAndInfo);
            if (match.Success)
            {
                var groupName = match.Groups[1].Value.Split(' ').Last();
                return Result.Ok(new UserDto
                {
                    FullName = fullName, 
                    GroupName = groupName
                });
            }
            
            logger.LogError("Pattern not found in the fullname_and_info.");
        }
        else
            logger.LogError("Property 'fullname_and_info' not found in the response.");

        return Result.Fail($"Не удалось получить информацию о студенте {fullName}");
    }

    [GeneratedRegex(@"\((.*?)(?:,\s|\))")]
    private static partial Regex NstuFullNameAndInfoRegex();
}
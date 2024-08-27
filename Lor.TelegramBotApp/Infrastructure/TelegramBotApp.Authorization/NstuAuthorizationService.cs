using System.Text.Json;
using System.Text.RegularExpressions;
using FluentResults;
using TelegramBotApp.Authorization.Dto;
using TelegramBotApp.Authorization.Interfaces;

namespace TelegramBotApp.Authorization;

public class NstuAuthorizationService : IAuthorizationService
{
    public async Task<Result<AuthorizationReply>> TryAuthorize(AuthorizationRequest request)
    {
        try
        {
            string fullName = request.FullName;
            DateTime? dateOfBirth = request.DateOfBirth;

            Result<UserDto> result = await ParseUserDto(fullName, dateOfBirth);
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

        HttpResponseMessage response = await client.PostAsync("https://id.nstu.ru/user_lookup", formData);

        response.EnsureSuccessStatusCode();

        string responseBody = await response.Content.ReadAsStringAsync();

        using JsonDocument document = JsonDocument.Parse(responseBody);
        if (document.RootElement.TryGetProperty("fullname", out JsonElement fullNameElement))
        {
            string? fullNameFromData = fullNameElement.GetString()?.Trim();

            if (fullNameFromData == null) return Result.Fail("Can't get fullname from the response.");
            
            fullName = fullNameFromData;
        }
        
        if (document.RootElement.TryGetProperty("fullname_and_info", out JsonElement fullnameAndInfoElement))
        {
            string fullnameAndInfo = fullnameAndInfoElement.GetString()!;
            
            fullnameAndInfo = fullnameAndInfo.Replace(fullName, string.Empty).Trim();
            const string pattern = @"\((.*?)\)";
            Match match = Regex.Match(fullnameAndInfo, pattern);
            if (match.Success)
            {
                string groupName = match.Groups[1].Value.Split(' ').Last();
                return Result.Ok(new UserDto
                {
                    FullName = fullName, 
                    GroupName = groupName
                });
            }
            
            Console.WriteLine("Pattern not found in the fullname_and_info.");
        }
        else
        {
            Console.WriteLine("Property 'fullname_and_info' not found in the response.");
        }
        
        return Result.Fail($"Не удалось получить информацию о студенте {fullName}");
    }
}
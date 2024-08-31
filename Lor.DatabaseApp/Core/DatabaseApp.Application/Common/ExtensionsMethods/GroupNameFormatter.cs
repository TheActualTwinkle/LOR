using System.Text.RegularExpressions;

namespace DatabaseApp.Application.Common.ExtensionsMethods;

public static class GroupNameFormatter
{
    public static Task<string> GroupNameFormat(this string groupName)
    {
        if (string.IsNullOrEmpty(groupName))
        {
            return Task.FromResult(string.Empty);
        }
        
        Match match = Regex.Match(groupName, @"^\D*\d+");
        
        return Task.FromResult(match.Success ? match.Value : groupName);
    }
}
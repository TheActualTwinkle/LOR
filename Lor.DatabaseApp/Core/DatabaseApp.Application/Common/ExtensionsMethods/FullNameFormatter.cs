using System.Text.RegularExpressions;

namespace DatabaseApp.Application.Common.ExtensionsMethods;

public static partial class FullNameFormatter
{
    public static async Task<string> Format(this string fullName)
    {
        var match = FullNameRegex().Match(fullName);

        if (!match.Success)
            throw new ArgumentException("Некорректное ФИО");

        var lastName = match.Groups["lastName"].Value;
        var firstName = match.Groups["firstName"].Value;
        var middleName = match.Groups["middleName"].Value;

        var formattedLastName = await lastName.FormatPart();
        var formattedFirstName = await firstName.FormatPart();
        var formattedMiddleName = string.IsNullOrEmpty(middleName) ? string.Empty : await middleName.FormatPart();

        return $"{formattedLastName} {formattedFirstName} {formattedMiddleName}".Trim();

    }

    private static async Task<string> FormatPart(this string part) =>
        await Task.FromResult(part[..1].ToUpper() + part[1..].ToLower());
    
    [GeneratedRegex(@"^(?<lastName>\p{L}+)\s+(?<firstName>\p{L}+)(\s+(?<middleName>\p{L}+))?$")]
    private static partial Regex FullNameRegex();
}
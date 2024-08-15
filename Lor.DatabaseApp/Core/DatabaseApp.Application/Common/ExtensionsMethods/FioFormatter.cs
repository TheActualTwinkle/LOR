using System.Text.RegularExpressions;

public static class FioFormatter
{
    public static async Task<string> FormatFio(this string fullName)
    {
        Match match = Regex.Match(fullName,
            @"^(?<lastName>\p{L}+)\s+(?<firstName>\p{L}+)(\s+(?<middleName>\p{L}+))?$");

        if (!match.Success)
        {
            throw new ArgumentException("Некорректное ФИО");
        }

        string lastName = match.Groups["lastName"].Value;
        string firstName = match.Groups["firstName"].Value;
        string middleName = match.Groups["middleName"].Value;

        string formattedLastName = await lastName.FormatPart();
        string formattedFirstName = await firstName.FormatPart();
        string formattedMiddleName = string.IsNullOrEmpty(middleName) ? string.Empty : await middleName.FormatPart();

        return $"{formattedLastName} {formattedFirstName} {formattedMiddleName}".Trim();

    }

    private static async Task<string> FormatPart(this string part) =>
        await Task.FromResult(part[..1].ToUpper() + part[1..].ToLower());
}
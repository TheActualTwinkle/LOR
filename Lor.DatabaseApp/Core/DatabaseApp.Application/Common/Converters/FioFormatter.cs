using System.Text.RegularExpressions;

namespace DatabaseApp.Application.Common.Converters;

public static class FioFormatter
{
    public static async Task<string> FormatFio(this string fio)
    {
        Match match = Regex.Match(fio,
            @"^(?<lastName>[А-Яа-я]+)\s+(?<firstName>[А-Яа-я]+)\s+(?<middleName>[А-Яа-я]+)$");

        if (match.Success)
        {
            string lastName = match.Groups["lastName"].Value;
            string firstName = match.Groups["firstName"].Value;
            string middleName = match.Groups["middleName"].Value;

            string formattedLastName = await lastName.FormatPart();
            string formattedFirstName = await firstName.FormatPart();
            string formattedMiddleName = await middleName.FormatPart();

            return await Task.FromResult($"{formattedLastName} {formattedFirstName} {formattedMiddleName}");
        }
        else
        {
            throw new ArgumentException("Некорректное ФИО");
        }
    }

    private static async Task<string> FormatPart(this string part) =>
        await Task.FromResult(part.Substring(0, 1).ToUpper() + part.Substring(1).ToLower());
}
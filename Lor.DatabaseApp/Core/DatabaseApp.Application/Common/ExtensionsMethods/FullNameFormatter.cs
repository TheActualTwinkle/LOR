using System.Text.RegularExpressions;

namespace DatabaseApp.Application.Common.ExtensionsMethods;

public static partial class FullNameFormatter
{
    public static string Format(string fullName)
    {
        if (string.IsNullOrWhiteSpace(fullName))
            return fullName;

        fullName = NamePartRegex().Replace(fullName.Trim(), " ");

        var nameParts = fullName.Split(' ', StringSplitOptions.RemoveEmptyEntries);

        for (var i = 0; i < nameParts.Length; i++)
            nameParts[i] = FormatNamePart(nameParts[i]);

        return string.Join(" ", nameParts);
    }

    private static string FormatNamePart(string namePart)
    {
        if (string.IsNullOrWhiteSpace(namePart))
            return namePart;

        var subParts = namePart.Split(['-', '\''], StringSplitOptions.RemoveEmptyEntries);

        for (var i = 0; i < subParts.Length; i++)
            if (subParts[i].Length > 0)
                subParts[i] = char.ToUpper(subParts[i][0]) + subParts[i][1..].ToLower();

        return string.Join(namePart.Contains('-') ? "-" : "'", subParts);
    }

    [GeneratedRegex(@"\s+")]
    private static partial Regex NamePartRegex();
}
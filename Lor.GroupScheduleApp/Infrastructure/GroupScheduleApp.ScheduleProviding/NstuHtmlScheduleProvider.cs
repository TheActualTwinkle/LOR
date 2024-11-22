using System.Globalization;
using System.Text.RegularExpressions;
using GroupScheduleApp.ScheduleProviding.Interfaces;
using GroupScheduleApp.Shared;
using HtmlAgilityPack;

namespace GroupScheduleApp.ScheduleProviding;

public partial class NstuHtmlScheduleProvider(IEnumerable<string> urls) : IScheduleProvider
{
    private static class Constants
    {
        public const string WeekHeaderValue = "schedule__title-label";
        public const string TableBody = "schedule__table-body";
        public const string TableBodyRow = "schedule__table-row";
        public const string ClassRow = "schedule__table-item";
        public const string ClassDate = "schedule__table-date";
        public const string GroupName = "schedule__title-h1";
    }

    private readonly HttpClient _httpClient = new();
    
    // TODO: DI
    private TimeSpan ScheduleFetchDateOffset => TimeSpan.FromDays(7);

    public async Task<IEnumerable<string>> GetAvailableGroupsAsync()
    {
        List<string> groups = [];

        var htmlDocument = new HtmlDocument();

        foreach (var url in urls)
        {
            var response = await _httpClient.GetStringAsync(url);

            htmlDocument.LoadHtml(response);

            var groupName = GetGroupName(htmlDocument);

            groups.Add(groupName);
        }

        return groups;
    }

    // TODO: Issue at https://github.com/TheActualTwinkle/LOR/issues/1
    public async Task<IEnumerable<GroupClassesData>> GetGroupClassesDataAsync()
    {
        List<GroupClassesData> groupClassesData = [];
        
        var htmlDocument = new HtmlDocument();

        foreach (var url in urls)
        {
            List<ClassData> classesData = [];

            var response = await _httpClient.GetStringAsync(url);
            htmlDocument.LoadHtml(response);

            var groupName = GetGroupName(htmlDocument);
            
            var weekNumber = GetWeekNumber(
                htmlDocument.DocumentNode
                .SelectSingleNode($"//span[contains(@class, '{Constants.WeekHeaderValue}')]")
                .InnerText
                );

            if (weekNumber == null) continue;

            response = await _httpClient.GetStringAsync($"{url}&week={weekNumber}");
            htmlDocument.LoadHtml(response);
            classesData.AddRange(ParseForWeek(htmlDocument));

            response = await _httpClient.GetStringAsync($"{url}&week={++weekNumber}");
            htmlDocument.LoadHtml(response);
            classesData.AddRange(ParseForWeek(htmlDocument));

            classesData = classesData
                .Where(d => d.Date >= Today() && d.Date < Today() + ScheduleFetchDateOffset)
                .ToList();

            groupClassesData.Add(new GroupClassesData(groupName, classesData));
        }

        return groupClassesData.AsEnumerable();
    }

    private IEnumerable<ClassData> ParseForWeek(HtmlDocument htmlDocument)
    {
        List<ClassData> classesData = [];

        var body = htmlDocument.DocumentNode.SelectSingleNode($"//div[contains(@class, '{Constants.TableBody}')]");

        var rows = body.SelectNodes($"./div[contains(@class, '{Constants.TableBodyRow}')]").ToList();

        foreach (var row in rows.Where(r => !string.IsNullOrEmpty(r.InnerText)))
        {
            var classNames = row.SelectNodes($".//div[contains(@class, '{Constants.ClassRow}')]")
                .Where(e => !string.IsNullOrWhiteSpace(e.InnerText))
                .Where(e => e.InnerText.Contains("Лабораторная"))
                .Select(e =>
                {
                    var indexOfSeparator = e.InnerText.IndexOf("&middot", StringComparison.Ordinal);
                    return new string(e.InnerText.Take(indexOfSeparator).ToArray()).Trim();
                })
                .Distinct()
                .ToList();

            if (classNames.Count == 0) continue;

            var dateRaw = row.SelectSingleNode($".//span[contains(@class, '{Constants.ClassDate}')]").InnerText;
            var date = DateTime.ParseExact($"{dateRaw}.{DateTime.Now.Year}", "dd.MM.yyyy", CultureInfo.InvariantCulture);

            foreach (var className in classNames)
                classesData.Add(new ClassData(className, date));
        }

        return classesData.AsEnumerable();
    }

    private string GetGroupName(HtmlDocument htmlDocument) =>
        htmlDocument.DocumentNode
            .SelectSingleNode($"//h1[contains(@class, '{Constants.GroupName}')]")
            .InnerText
            .Split(' ')
            .Last();

    private int? GetWeekNumber(string text)
    {
        var match = WeekNumberRegex().Match(text);

        return match.Success ? int.Parse(match.Value) : null;
    }

    private DateTime Today() =>
        DateTime.Now.Date;

    [GeneratedRegex(@"\d+")]
    private static partial Regex WeekNumberRegex();
}
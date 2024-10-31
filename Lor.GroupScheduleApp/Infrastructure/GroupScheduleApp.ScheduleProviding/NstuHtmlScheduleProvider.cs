using System.Globalization;
using System.Text.RegularExpressions;
using GroupScheduleApp.ScheduleProviding.Interfaces;
using GroupScheduleApp.Shared;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;

namespace GroupScheduleApp.ScheduleProviding;

public partial class NstuHtmlScheduleProvider : IScheduleProvider
{
    private static class Constants
    {
        public const string WeekHeader = "schedule__title-desc";
        public const string WeekHeaderValue = "schedule__title-label";
        public const string TableBody = "schedule__table-body";
        public const string TableBodyRow = "schedule__table-row";
        public const string ClassRow = "schedule__table-item";
        public const string ClassDate = "schedule__table-date";
        public const string GroupName = "schedule__title-h1";
    }
    
    // TODO: DI
    private TimeSpan ScheduleFetchDateOffset => TimeSpan.FromDays(7);
    
    private readonly ChromeDriver _chromeDriver;
    private readonly IEnumerable<string> _urls;
    
    public NstuHtmlScheduleProvider(IEnumerable<string> urls)
    {
        _urls = urls;
        
        ChromeOptions options = new();
        options.AddArgument("--headless"); // Ensure headless mode is enabled
        options.AddArgument("disable-infobars"); // Disabling info bars
        options.AddArgument("--disable-extensions"); // Disabling extensions
        options.AddArgument("--disable-gpu"); // Applicable to Windows OS only
        options.AddArgument("--disable-dev-shm-usage"); // Overcome limited resource problems
        options.AddArgument("--no-sandbox"); // Bypass OS security model
        options.AddArgument("--log-level=3");
        
        _chromeDriver = new ChromeDriver(options);
    }

    public async Task<IEnumerable<string>> GetAvailableGroupsAsync()
    {
        List<string> groups = [];
        
        foreach (var url in _urls)
        {
            await _chromeDriver.Navigate().GoToUrlAsync(url);
            var groupName = GetGroupName();
            groups.Add(groupName);
        }

        return groups;
    }

    public async Task<IEnumerable<GroupClassesData>> GetGroupClassesDataAsync()
    {
        List<GroupClassesData> groupClassesData = [];

        foreach (var url in _urls)
        {
            List<ClassData> classesData = [];
            
            await _chromeDriver.Navigate().GoToUrlAsync(url);
            var weekNumber = GetWeekNumber(_chromeDriver.FindElement(By.ClassName(Constants.WeekHeader)).FindElement(By.ClassName(Constants.WeekHeaderValue)).Text);
            var groupName = GetGroupName();

            if (weekNumber == null) continue;

            await _chromeDriver.Navigate().GoToUrlAsync($"{url}&week={weekNumber}");
            classesData.AddRange(ParseForWeek());
            
            await _chromeDriver.Navigate().GoToUrlAsync($"{url}&week={++weekNumber}");
            classesData.AddRange(ParseForWeek());
            
            classesData = classesData.Where(d => d.Date >= Today() && d.Date < Today() + ScheduleFetchDateOffset).ToList();

            groupClassesData.Add(new GroupClassesData(groupName, classesData));
        }
        
        return groupClassesData.AsEnumerable();
    }

    private IEnumerable<ClassData> ParseForWeek()
    {
        List<ClassData> classesData = [];
        
        var body = _chromeDriver.FindElement(By.ClassName(Constants.TableBody));
        var rows = body.FindElements(By.XPath($"./div[contains(@class, '{Constants.TableBodyRow}')]")).ToList();
        
        foreach (var row in rows.Where(r => string.IsNullOrEmpty(r.Text) == false))
        {
            var classNames = row.FindElements(By.ClassName(Constants.ClassRow))
                .Where(e => string.IsNullOrWhiteSpace(e.Text) == false)
                .Where(e => e.Text.Contains("Лабораторная") == true)
                .Select(e => new string(e.Text.TakeWhile(x => x != '·').ToArray()))
                .Distinct()
                .ToList();

            if (classNames.Count == 0) continue;
            
            var dateRaw = row.FindElement(By.ClassName(Constants.ClassDate)).Text;
            var date = DateTime.ParseExact($"{dateRaw}.{DateTime.Now.Year}", "dd.MM.yyyy", CultureInfo.InvariantCulture);

            foreach (var className in classNames)
                classesData.Add(new ClassData(className, date));
        }

        return classesData.AsEnumerable();
    }
    
    private string GetGroupName()
    {
        return _chromeDriver.FindElement(By.ClassName(Constants.GroupName)).Text.Split(' ').Last();
    }
    
    private int? GetWeekNumber(string text)
    {
        var match = WeekNumberRegex().Match(text);
        
        return match.Success ? int.Parse(match.Value) : null;
    }
    
    private DateTime Today()
    {
        return DateTime.Now.Date;
    }

    [GeneratedRegex(@"\d+")]
    private static partial Regex WeekNumberRegex();
}
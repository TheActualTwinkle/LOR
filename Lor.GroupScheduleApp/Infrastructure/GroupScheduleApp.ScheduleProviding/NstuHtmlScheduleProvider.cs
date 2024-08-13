using System.Globalization;
using System.Text.RegularExpressions;
using GroupScheduleApp.ScheduleProviding.Interfaces;
using GroupScheduleApp.Shared;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;

namespace GroupScheduleApp.ScheduleProviding;

public class NstuHtmlScheduleProvider : IScheduleProvider
{
    private static class Constants
    {
        public const string WeekHeader = "schedule__title-desc";
        public const string WeekHeaderValue = "schedule__title-label";
        public const string TableBody = "schedule__table-body";
        public const string TableBodyRow = "schedule__table-row";
        public const string ClassRow = "schedule__table-item";
        public const string ClassDate = "schedule__table-day";
        public const string GroupName = "schedule__title-h1";
    }
    
    // Week fetch interval
    private TimeSpan ScheduleFetchInterval => TimeSpan.FromDays(7);
    
    private readonly ChromeDriver _chromeDriver;
    private readonly IEnumerable<string> _urls;
    
    public NstuHtmlScheduleProvider(IEnumerable<string> urls)
    {
        _urls = urls;
        
        ChromeOptions options = new();
        options.AddArgument("--headless");
        options.AddArgument("start-maximized"); // open Browser in maximized mode
        options.AddArgument("disable-infobars"); // disabling info bars
        options.AddArgument("--disable-extensions"); // disabling extensions
        options.AddArgument("--disable-gpu"); // applicable to Windows os only
        options.AddArgument("--disable-dev-shm-usage"); // overcome limited resource problems
        options.AddArgument("--no-sandbox"); // Bypass OS security model
        options.AddArgument("--log-level=3");
        _chromeDriver = new ChromeDriver(options);
    }

    public async Task<IEnumerable<string>> GetAvailableGroupsAsync()
    {
        List<string> groups = [];
        
        foreach (string url in _urls)
        {
            await _chromeDriver.Navigate().GoToUrlAsync(url);
            string groupName = GetGroupName();
            groups.Add(groupName);
        }

        return groups;
    }

    public async Task<IEnumerable<GroupClassesData>> GetGroupClassesDataAsync()
    {
        List<GroupClassesData> groupClassesData = [new GroupClassesData("АВТ-218", [
            new ClassData("Вычмат", new DateTime(2024, 8, 13, 0, 0, 0)), 
            new ClassData("ООП", new DateTime(2024, 8, 20, 0, 0, 0))
        ])]; // [] TODO: Testing

        return groupClassesData; // remove TODO: Testing
        foreach (string url in _urls)
        {
            List<ClassData> classesData = [];
            
            await _chromeDriver.Navigate().GoToUrlAsync(url);
            int? weekNumber = GetWeekNumber(_chromeDriver.FindElement(By.ClassName(Constants.WeekHeader)).FindElement(By.ClassName(Constants.WeekHeaderValue)).Text);
            string groupName = GetGroupName();

            if (weekNumber == null) continue;

            await _chromeDriver.Navigate().GoToUrlAsync($"{url}&week={weekNumber}");
            classesData.AddRange(ParseForWeek());
            
            await _chromeDriver.Navigate().GoToUrlAsync($"{url}&week={++weekNumber}");
            classesData.AddRange(ParseForWeek());
            
            classesData = classesData.Where(d => d.Date >= Today() && d.Date <= Today() + ScheduleFetchInterval).ToList();

            groupClassesData.Add(new GroupClassesData(groupName, classesData));
        }
        
        return groupClassesData.AsEnumerable();
    }

    private IEnumerable<ClassData> ParseForWeek()
    {
        List<ClassData> classesData = [];
        
        IWebElement body = _chromeDriver.FindElement(By.ClassName(Constants.TableBody));
        List<IWebElement> rows = body.FindElements(By.XPath($"./div[contains(@class, '{Constants.TableBodyRow}')]")).ToList();
        
        foreach (IWebElement row in rows.Where(r => string.IsNullOrEmpty(r.Text) == false))
        {
            List<string> classNames = row.FindElements(By.ClassName(Constants.ClassRow))
                .Where(e => string.IsNullOrWhiteSpace(e.Text) == false)
                .Where(e => e.Text.Contains("Лабораторная") == true)
                .Select(e => new string(e.Text.TakeWhile(x => x != '·').ToArray()))
                .Distinct()
                .ToList();

            if (classNames.Count == 0) continue;
            
            string dateRaw = row.FindElement(By.ClassName(Constants.ClassDate)).Text;
            DateTime date = DateTime.ParseExact($"{dateRaw[2..]}.{DateTime.Now.Year}", "dd.MM.yyyy", CultureInfo.InvariantCulture);

            foreach (string className in classNames)
            {
                classesData.Add(new ClassData(className, new DateTime(2024, 8, 3))); // TODO: Testing
            }
        }

        return classesData.AsEnumerable();
    }
    
    private string GetGroupName()
    {
        return _chromeDriver.FindElement(By.ClassName(Constants.GroupName)).Text.Split(' ').Last();
    }
    
    private int? GetWeekNumber(string text)
    {
        text = "14 неделя"; // TODO: Testing
        
        const string pattern = @"\d+";
        Match match = Regex.Match(text, pattern);
        
        return match.Success ? int.Parse(match.Value) : null;
    }
    
    private DateTime Today()
    {
        return DateTime.Now.Date;
    }
}
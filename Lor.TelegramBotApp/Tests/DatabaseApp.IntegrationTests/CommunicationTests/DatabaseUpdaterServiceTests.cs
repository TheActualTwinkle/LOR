using FluentResults;
using GroupScheduleApp.Shared;

// ReSharper disable SuggestVarOrType_Elsewhere

namespace DatabaseApp.Tests.CommunicationTests;

[TestFixture]
public class DatabaseUpdaterServiceTests
{
    private const string GroupName = "АУЕ-228";
    private readonly ClassData[] _classesData = [new ClassData("Рубка леса", DateTime.Today.AddDays(1)), new ClassData("Качалка", DateTime.Today.AddDays(2))];

    private const long DefaultUserId = 1;
    private const string DefaultUserFullName = "Мистер Бист Младший";

    [TearDown]
    public async Task TearDown()
    {
        await IntegrationTestSharedContext.DatabaseAppFactory.ResetDatabaseAsync();
    }
    
    [Test]
    public async Task UpdateAvailableGroups()
    {
        await IntegrationTestSharedContext.DatabaseUpdaterCommunicationClient.SetAvailableGroups([GroupName]);

        Result<Dictionary<int,string>> availableGroups = await IntegrationTestSharedContext.DatabaseCommunication.GetAvailableGroups();
        
        Assert.Multiple(() =>
        {
            Assert.That(availableGroups.IsSuccess, Is.True);
            Assert.That(availableGroups.Value, Has.Count.EqualTo(1));
            Assert.That(availableGroups.Value.Values.First(), Is.EqualTo(GroupName));
        });
    }
    
    [Test]
    public async Task UpdateAvailableLabClasses()
    {
        await IntegrationTestSharedContext.DatabaseUpdaterCommunicationClient.SetAvailableGroups([GroupName]);
        await IntegrationTestSharedContext.DatabaseUpdaterCommunicationClient.SetAvailableLabClasses(new GroupClassesData(GroupName, _classesData));

        await IntegrationTestSharedContext.DatabaseCommunication.TrySetGroup(DefaultUserId, GroupName, DefaultUserFullName);
        
        var classes = await IntegrationTestSharedContext.DatabaseCommunication.GetAvailableLabClasses(DefaultUserId);
        
        Assert.Multiple(() =>
        {
            Assert.That(classes.IsSuccess, Is.True);
            Assert.That(classes.Value.ToList(), Has.Count.EqualTo(_classesData.Length));
            Assert.That(classes.Value.Select(x => x.ClassName), Is.EqualTo(_classesData.Select(x => x.Name)));
            Assert.That(classes.Value.Select(x => DateTimeOffset.FromUnixTimeSeconds(x.ClassDateUnixTimestamp).DateTime), Is.EqualTo(_classesData.Select(x => x.Date)));
        });
    }
}
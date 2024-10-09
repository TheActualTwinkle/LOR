using FluentResults;
using GroupScheduleApp.Shared;
using TelegramBotApp.AppCommunication.Data;
// ReSharper disable SuggestVarOrType_Elsewhere

namespace DatabaseApp.Tests.CommunicationTests;

[TestFixture]
public class DatabaseServiceTests
{
    private const string GroupName = "АУЕ-228";
    private readonly ClassData[] _classesData = [new ClassData("Рубка леса", DateTime.Today.AddDays(1)), new ClassData("Качалка", DateTime.Today.AddDays(2))];

    private const long DefaultUserId = 1;
    private const string DefaultUserFullName = "Мистер Бист Младший";
    
    [SetUp]
    public async Task Setup()
    {
        await IntegrationTestSharedContext.DatabaseUpdaterCommunicationClient.SetAvailableGroups([GroupName]);
        await IntegrationTestSharedContext.DatabaseUpdaterCommunicationClient.SetAvailableLabClasses(new GroupClassesData(GroupName, _classesData));
        
        await IntegrationTestSharedContext.DatabaseCommunication.SetGroup(DefaultUserId, GroupName, DefaultUserFullName);
    }
    
    [TearDown]
    public async Task TearDown()
    {
        await IntegrationTestSharedContext.DatabaseAppFactory.ResetDatabaseAsync();
    }
    
    [Test]
    public async Task GetAvailableGroups()
    {
        Result<Dictionary<int, string>> result = await IntegrationTestSharedContext.DatabaseCommunication.GetAvailableGroups();
        
        Assert.Multiple(() =>
        {
            Assert.That(result.IsSuccess, Is.True);
            Assert.That(result.Value.Values.All(x => x == GroupName), Is.True);
        });
    }
    
    [Test]
    public async Task GetAvailableLabClasses()
    {
        var classesResult = await IntegrationTestSharedContext.DatabaseCommunication.GetAvailableLabClasses(DefaultUserId);
        Assert.Multiple(() =>
        {
            Assert.That(classesResult.IsSuccess);
            Assert.That(classesResult.Value.Select(x => x.ClassName), Is.EqualTo(_classesData.Select(x => x.Name)));
            Assert.That(classesResult.Value.Select(x => DateTimeOffset.FromUnixTimeSeconds(x.ClassDateUnixTimestamp).DateTime), Is.EqualTo(_classesData.Select(x => x.Date)));
        });
    }
    
    [Test]
    public async Task EnqueueInClass_ManyDifferentUsers_ShouldReturnSortedListOfEnqueuedUsers()
    {
        const string secondUserFullName = "Мистер Бист Средний";
        const string thirdUserFullName = "Мистер Бист Старший";
        
        // Add two more users to the group
        Assert.Multiple(async () =>
        {
            Assert.That((await IntegrationTestSharedContext.DatabaseCommunication.SetGroup(DefaultUserId + 1, GroupName, secondUserFullName)).IsSuccess, Is.True);
            Assert.That((await IntegrationTestSharedContext.DatabaseCommunication.SetGroup(DefaultUserId + 2, GroupName, thirdUserFullName)).IsSuccess, Is.True);
        });

        var classesResult = await IntegrationTestSharedContext.DatabaseCommunication.GetAvailableLabClasses(DefaultUserId);
        Assert.That(classesResult.IsSuccess, Is.True);

        int classId = classesResult.Value.First().ClassId;
        Result<EnqueueInClassResult> result1 = await IntegrationTestSharedContext.DatabaseCommunication.EnqueueInClass(classId, DefaultUserId);
        Result<EnqueueInClassResult> result2 = await IntegrationTestSharedContext.DatabaseCommunication.EnqueueInClass(classId, DefaultUserId + 1);
        Result<EnqueueInClassResult> result3 = await IntegrationTestSharedContext.DatabaseCommunication.EnqueueInClass(classId, DefaultUserId + 2);
        
        Assert.Multiple(() =>
        {
            Assert.That(result1.IsSuccess, Is.True);
            Assert.That(result2.IsSuccess, Is.True);
            Assert.That(result3.IsSuccess, Is.True);
            
            Assert.That(result3.Value.StudentsQueue.ToArray()[0], Is.EqualTo(DefaultUserFullName));
            Assert.That(result3.Value.StudentsQueue.ToArray()[1], Is.EqualTo(secondUserFullName));
            Assert.That(result3.Value.StudentsQueue.ToArray()[2], Is.EqualTo(thirdUserFullName));
        });
    }
    
    [Test]
    public async Task EnqueueInClass_SameUserTwice_ShouldReturnSingleUser()
    {
        var classesResult = await IntegrationTestSharedContext.DatabaseCommunication.GetAvailableLabClasses(DefaultUserId);
        Assert.That(classesResult.IsSuccess, Is.True);

        int classId = classesResult.Value.First().ClassId;
        Result<EnqueueInClassResult> result1 = await IntegrationTestSharedContext.DatabaseCommunication.EnqueueInClass(classId, DefaultUserId);
        Result<EnqueueInClassResult> result2 = await IntegrationTestSharedContext.DatabaseCommunication.EnqueueInClass(classId, DefaultUserId);
        
        Assert.Multiple(() =>
        {
            Assert.That(result1.IsSuccess, Is.True);
            Assert.That(result2.IsSuccess, Is.True);

            Assert.That(result2.Value.StudentsQueue.Count, Is.EqualTo(1));
            Assert.That(result2.Value.StudentsQueue.ToArray()[0], Is.EqualTo(DefaultUserFullName));
        });
    }
    
    [Test]
    public async Task AddSubscriber()
    {
        Result result1 = await IntegrationTestSharedContext.DatabaseCommunication.AddSubscriber(DefaultUserId);
        Result result2 = await IntegrationTestSharedContext.DatabaseCommunication.AddSubscriber(DefaultUserId);
        
        Assert.Multiple(() =>
        {
            Assert.That(result1.IsSuccess, Is.True);
            Assert.That(result2.IsSuccess, Is.False);
        });
    }
    
    [Test]
    public async Task RemoveSubscriber()
    {
        Result result1 = await IntegrationTestSharedContext.DatabaseCommunication.AddSubscriber(DefaultUserId);
        Result result2 = await IntegrationTestSharedContext.DatabaseCommunication.DeleteSubscriber(DefaultUserId);
        Result result3 = await IntegrationTestSharedContext.DatabaseCommunication.DeleteSubscriber(DefaultUserId);
        
        Assert.Multiple(() =>
        {
            Assert.That(result1.IsSuccess, Is.True);
            Assert.That(result2.IsSuccess, Is.True);
            Assert.That(result3.IsSuccess, Is.False);
        });
    }
}
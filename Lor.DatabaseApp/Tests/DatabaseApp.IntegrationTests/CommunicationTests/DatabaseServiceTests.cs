using DatabaseApp.Domain.Models;
using FluentResults;
using GroupScheduleApp.Shared;

// ReSharper disable SuggestVarOrType_Elsewhere

namespace DatabaseApp.Tests.CommunicationTests;

[TestFixture]
public class DatabaseServiceTests
{
    private const string DefaultGroupName = "АУЕ-228";
    private readonly ClassData[] _classesData = [new ClassData("Рубка леса", DateTime.Today.AddDays(1)), new ClassData("Качалка", DateTime.Today.AddDays(2))];

    private const long DefaultUserId = 1;
    private const string DefaultUserFullName = "Мистер Бист Младший";
    
    [SetUp]
    public async Task Setup()
    {
        await IntegrationTestsSharedContext.DatabaseUpdaterCommunicationClient.SetAvailableGroups([DefaultGroupName]);
        await IntegrationTestsSharedContext.DatabaseUpdaterCommunicationClient.SetAvailableLabClasses(new GroupClassesData(DefaultGroupName, _classesData));
        
        await IntegrationTestsSharedContext.DatabaseCommunication.SetGroup(DefaultUserId, DefaultGroupName, DefaultUserFullName);
    }
    
    [TearDown]
    public async Task TearDown()
    {
        await IntegrationTestsSharedContext.DatabaseAppFactory.ResetDatabaseAsync();
    }
    
    [Test]
    public async Task GetAvailableGroups()
    {
        var result = await IntegrationTestsSharedContext.DatabaseCommunication.GetAvailableGroups();
        
        Assert.Multiple(() =>
        {
            Assert.That(result.IsSuccess, Is.True);
            Assert.That(result.Value.Values.All(x => x == DefaultGroupName), Is.True);
        });
    }
    
    [Test]
    public async Task GetAvailableLabClasses()
    {
        var classesResult = await IntegrationTestsSharedContext.DatabaseCommunication.GetAvailableLabClasses(DefaultUserId);
        
        Assert.Multiple(() =>
        {
            Assert.That(classesResult.IsSuccess);
            Assert.That(classesResult.Value.Select(x => x.Name), Is.EqualTo(_classesData.Select(x => x.Name)));
            Assert.That(classesResult.Value.Select(x => x.Date.ToDateTime(TimeOnly.MinValue)), Is.EqualTo(_classesData.Select(x => x.Date)));
        });
    }
    
    [Test]
    public async Task EnqueueInClass_ManyDifferentUsers_SortedListOfEnqueuedUsers()
    {
        var users = new List<User>
        {
            new()
            {
                FullName = "Мистер Бист Средний",
                Group = new Group {Name = DefaultGroupName},
                TelegramId = DefaultUserId + 1
            },
            new()
            {
                FullName = "Мистер Бист Старший",
                Group = new Group {Name = DefaultGroupName},
                TelegramId = DefaultUserId + 2
            }
        };
        
        Assert.That((await AddUsersToGroup(users)).IsSuccess, Is.True);
        
        var classesResult = await IntegrationTestsSharedContext.DatabaseCommunication.GetAvailableLabClasses(DefaultUserId);
        Assert.That(classesResult.IsSuccess, Is.True);

        var classId = classesResult.Value.First().Id;
        
        var result1 = await IntegrationTestsSharedContext.DatabaseCommunication.EnqueueInClass(classId, DefaultUserId);
        var result2 = await IntegrationTestsSharedContext.DatabaseCommunication.EnqueueInClass(classId, DefaultUserId + 1);
        var result3 = await IntegrationTestsSharedContext.DatabaseCommunication.EnqueueInClass(classId, DefaultUserId + 2);
        
        Assert.Multiple(() =>
        {
            Assert.That(result1.IsSuccess, Is.True);
            Assert.That(result2.IsSuccess, Is.True);
            Assert.That(result3.IsSuccess, Is.True);
            
            Assert.That(result3.Value.StudentsQueue.ToArray()[0], Is.EqualTo(DefaultUserFullName));
            Assert.That(result3.Value.StudentsQueue.ToArray()[1], Is.EqualTo(users[0].FullName));
            Assert.That(result3.Value.StudentsQueue.ToArray()[2], Is.EqualTo(users[1].FullName));
        });
    }
    
    [Test]
    public async Task EnqueueInClass_SameUserTwice_SingleUser()
    {
        var classesResult = await IntegrationTestsSharedContext.DatabaseCommunication.GetAvailableLabClasses(DefaultUserId);
        Assert.That(classesResult.IsSuccess, Is.True);

        var classId = classesResult.Value.First().Id;
        
        var result1 = await IntegrationTestsSharedContext.DatabaseCommunication.EnqueueInClass(classId, DefaultUserId);
        var result2 = await IntegrationTestsSharedContext.DatabaseCommunication.EnqueueInClass(classId, DefaultUserId);
        
        Assert.Multiple(() =>
        {
            Assert.That(result1.IsSuccess, Is.True);
            Assert.That(result2.IsSuccess, Is.True);

            Assert.That(result2.Value.StudentsQueue.Count, Is.EqualTo(1));
            Assert.That(result2.Value.StudentsQueue.ToArray().First(), Is.EqualTo(DefaultUserFullName));
        });
    }
    
    [Test]
    [TestCase(0)]
    [TestCase(1)]
    public async Task DequeueFromClassByIndex(int index)
    {
        var users = new List<User>
        {
            new()
            {
                FullName = "Мистер Бист Средний",
                Group = new Group {Name = DefaultGroupName},
                TelegramId = DefaultUserId + 1
            },
            new()
            {
                FullName = "Мистер Бист Старший",
                Group = new Group {Name = DefaultGroupName},
                TelegramId = DefaultUserId + 2
            }
        };
        
        Assert.That((await AddUsersToGroup(users)).IsSuccess, Is.True);
        
        var classesResult = await IntegrationTestsSharedContext.DatabaseCommunication.GetAvailableLabClasses(DefaultUserId);
        Assert.That(classesResult.IsSuccess, Is.True);

        var classId = classesResult.Value.First().Id;
        
        var result1 = await IntegrationTestsSharedContext.DatabaseCommunication.EnqueueInClass(classId, DefaultUserId);
        var result2 = await IntegrationTestsSharedContext.DatabaseCommunication.EnqueueInClass(classId, DefaultUserId + 1);
        var result3 = await IntegrationTestsSharedContext.DatabaseCommunication.EnqueueInClass(classId, DefaultUserId + 2);

        var queueBeforeDelete = result3.Value.StudentsQueue.ToList();
        
        var dequeueResult = await IntegrationTestsSharedContext.DatabaseCommunication.DequeueFromClass(classId, DefaultUserId + index);

        var queueAfterDelete = dequeueResult.Value.StudentsQueue;
        
        var expectedQueue = queueBeforeDelete.ToList();
        expectedQueue.RemoveAt(index);

        Assert.Multiple(() =>
        {
            Assert.That(result1.IsSuccess, Is.True);
            Assert.That(result2.IsSuccess, Is.True);
            Assert.That(result3.IsSuccess, Is.True);
            
            Assert.That(dequeueResult.IsSuccess, Is.True);

            Assert.That(queueAfterDelete, Is.EquivalentTo(expectedQueue));
        });
    }

    [Test]
    public async Task ViewClassQueue()
    {
        var users = new List<User>
        {
            new()
            {
                FullName = "Мистер Бист Средний",
                Group = new Group {Name = DefaultGroupName},
                TelegramId = DefaultUserId + 1
            },
            new()
            {
                FullName = "Мистер Бист Старший",
                Group = new Group {Name = DefaultGroupName},
                TelegramId = DefaultUserId + 2
            }
        };
        
        Assert.That((await AddUsersToGroup(users)).IsSuccess, Is.True);
        
        var classesResult = await IntegrationTestsSharedContext.DatabaseCommunication.GetAvailableLabClasses(DefaultUserId);
        Assert.That(classesResult.IsSuccess, Is.True);

        var classId = classesResult.Value.First().Id;
        
        var result1 = await IntegrationTestsSharedContext.DatabaseCommunication.EnqueueInClass(classId, DefaultUserId);
        var result2 = await IntegrationTestsSharedContext.DatabaseCommunication.EnqueueInClass(classId, DefaultUserId + 1);
        var result3 = await IntegrationTestsSharedContext.DatabaseCommunication.EnqueueInClass(classId, DefaultUserId + 2);
        
        var viewQueueClassResult = await IntegrationTestsSharedContext.DatabaseCommunication.ViewClassQueue(classId);
        
        Assert.Multiple(() =>
        {
            Assert.That(result1.IsSuccess, Is.True);
            Assert.That(result2.IsSuccess, Is.True);
            Assert.That(result3.IsSuccess, Is.True);
            
            Assert.That(viewQueueClassResult.IsSuccess, Is.True);

            Assert.That(viewQueueClassResult.Value.StudentsQueue, Is.EquivalentTo( result3.Value.StudentsQueue.ToList()));
        });
    }
    
    [Test]
    public async Task AddSubscriber()
    {
        var result1 = await IntegrationTestsSharedContext.DatabaseCommunication.AddSubscriber(DefaultUserId);
        var result2 = await IntegrationTestsSharedContext.DatabaseCommunication.AddSubscriber(DefaultUserId);
        
        Assert.Multiple(() =>
        {
            Assert.That(result1.IsSuccess, Is.True);
            Assert.That(result2.IsSuccess, Is.False);
        });
    }
    
    [Test]
    public async Task RemoveSubscriber()
    {
        var result1 = await IntegrationTestsSharedContext.DatabaseCommunication.AddSubscriber(DefaultUserId);
        var result2 = await IntegrationTestsSharedContext.DatabaseCommunication.DeleteSubscriber(DefaultUserId);
        var result3 = await IntegrationTestsSharedContext.DatabaseCommunication.DeleteSubscriber(DefaultUserId);
        
        Assert.Multiple(() =>
        {
            Assert.That(result1.IsSuccess, Is.True);
            Assert.That(result2.IsSuccess, Is.True);
            Assert.That(result3.IsSuccess, Is.False);
        });
    }
    
    private async Task<Result<IEnumerable<string>>> AddUsersToGroup(IEnumerable<User> users)
    {
        var results = new List<Result<string>>();
        
        foreach (var user in users)
            results.Add(await IntegrationTestsSharedContext.DatabaseCommunication.SetGroup(user.TelegramId, user.Group.Name, user.FullName));

        return results.All(x => x.IsSuccess) ? Result.Ok() : Result.Fail("Failed to add users to the group.");
    }
}
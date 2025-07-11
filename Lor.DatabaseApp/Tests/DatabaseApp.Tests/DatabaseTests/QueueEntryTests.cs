﻿using DatabaseApp.Application.Classes.Command;
using DatabaseApp.Application.Classes.Queries;
using DatabaseApp.Application.Groups.Command.CreateGroup;
using DatabaseApp.Application.Groups.Queries;
using DatabaseApp.Application.QueueEntries.Commands.CreateEntry;
using DatabaseApp.Application.QueueEntries.Commands.DeleteOutdatedQueues;
using DatabaseApp.Application.QueueEntries.Commands.DeleteEntry;
using DatabaseApp.Application.QueueEntries.Queries;
using DatabaseApp.Application.Users.Command.CreateUser;
using DatabaseApp.Domain.Repositories;
using DatabaseApp.Tests.TestContext;
using MediatR;
using Microsoft.Extensions.DependencyInjection;

namespace DatabaseApp.Tests.DatabaseTests;

public class QueueEntryTests
{
    private readonly WebAppFactory _factory = new();
    
    private ISender _sender;
    private IUnitOfWork _unitOfWork;
    
    private const long TestTelegramId = 123456789;
    private const string TestFullName = "John Doe";
    private const string TestGroupName = "ОО-АА";
    private const string TestClassName = "Math";
    
    [OneTimeSetUp]
    public async Task OneTimeSetup()
    {
        await _factory.InitializeAsync();

        var scope = _factory.Services.CreateScope();

        _sender = scope.ServiceProvider.GetRequiredService<ISender>();
        _unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
    }
    
    [TearDown]
    public async Task TearDown()
    {
        await _factory.ResetDatabaseAsync();
    }
    
    [OneTimeTearDown]
    public async Task OneTimeTearDown()
    {
        await _factory.DisposeAsync();
        _unitOfWork.Dispose();
    }
    
    [Test]
    public async Task CreateQueue_WhenQueueNotExistAndOtherDataValid_Success()
    {
        // Arrange
        await _sender.Send(new CreateGroupsCommand
        {
            GroupNames = [TestGroupName]
        });
        
        await _sender.Send(new CreateUserCommand
        {
            TelegramId = TestTelegramId,
            FullName = TestFullName,
            GroupName = TestGroupName
        });

        await _sender.Send(new CreateClassesCommand
        {
            Classes = new Dictionary<string, DateOnly> { { TestClassName, DateOnly.FromDateTime(DateTime.Now.AddDays(1)) } },
            GroupName = TestGroupName
        });

        var getGroupResult = await _sender.Send(new GetGroupQuery { GroupName = TestGroupName });
        
        var getClassesResult = await _sender.Send(new GetClassesQuery { GroupName = TestGroupName });

        // Act
        var createResult = await _sender.Send(new CreateQueueEntryCommand
        {
            TelegramId = TestTelegramId,
            ClassId = getClassesResult.Value.First().Id
        });

        var queue = await _sender.Send(new GetClassQueueQuery { ClassId = getClassesResult.Value.First().Id });

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(createResult.IsSuccess, Is.True);
            Assert.That(getGroupResult.IsSuccess, Is.True);
            Assert.That(queue.Value, Has.Count.EqualTo(1));
        });
    }
    
    [Test]
    public async Task CreateQueue_WhenUserNotExist_Fail()
    {
        // Arrange
        await _sender.Send(new CreateGroupsCommand
        {
            GroupNames = [TestGroupName]
        });
        
        await _sender.Send(new CreateClassesCommand
        {
            Classes = new Dictionary<string, DateOnly> { { TestClassName, DateOnly.FromDateTime(DateTime.Now.AddDays(1)) } },
            GroupName = TestGroupName
        });
        
        var getGroupResult = await _sender.Send(new GetGroupQuery { GroupName = TestGroupName });

        var getClassesResult = await _sender.Send(new GetClassesQuery { GroupName = TestGroupName });

        // Act
        var createResult = await _sender.Send(new CreateQueueEntryCommand
        {
            TelegramId = TestTelegramId,
            ClassId = getClassesResult.Value.First().Id
        });

        var queue = await _sender.Send(new GetClassQueueQuery { ClassId = getClassesResult.Value.First().Id });
        
        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(createResult.IsFailed, Is.True);
            Assert.That(getGroupResult.IsSuccess, Is.True);
            Assert.That(queue.Value, Has.Count.EqualTo(0));
        });
    }
    
    [Test]
    public async Task CreateQueue_WhenClassNotExist_Fail()
    {
        // Arrange
        await _sender.Send(new CreateGroupsCommand
        {
            GroupNames = [TestGroupName]
        });

        await _sender.Send(new CreateUserCommand
        {
            TelegramId = TestTelegramId,
            FullName = TestFullName,
            GroupName = TestGroupName
        });

        // Act
        var createResult = await _sender.Send(new CreateQueueEntryCommand
        {
            TelegramId = TestTelegramId,
            ClassId = 1337
        });
        
        // Assert
        Assert.That(createResult.IsFailed, Is.True);
    }
    
    [Test]
    public async Task CreateQueue_WhenUserAlreadyInQueue_SuccessAndMarkedAsRequeuingAttempt()
    {
        // Arrange
        await CreateUserAndClasses();
        
        var getClassesResult = await _sender.Send(new GetClassesQuery { GroupName = TestGroupName });

        await _sender.Send(new CreateQueueEntryCommand
        {
            TelegramId = TestTelegramId,
            ClassId = getClassesResult.Value.First().Id
        });

        // Act
        var createResult = await _sender.Send(new CreateQueueEntryCommand
        {
            TelegramId = TestTelegramId,
            ClassId = getClassesResult.Value.First().Id
        });

        var queue = await _sender.Send(new GetClassQueueQuery { ClassId = getClassesResult.Value.First().Id });
        
        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(createResult.IsSuccess, Is.True);
            Assert.That(createResult.Value.Class.Id, Is.EqualTo(getClassesResult.Value.First().Id)); 
            Assert.That(createResult.Value.WasAlreadyEnqueued, Is.True);
            Assert.That(queue.Value, Has.Count.EqualTo(1));
        });
    }
    
    [Test]
    public async Task DeleteQueue_WhenQueueExist_Success()
    {
        // Arrange
        await CreateUserAndClasses();
        
        var getClassesResult = await _sender.Send(new GetClassesQuery { GroupName = TestGroupName });

        await _sender.Send(new CreateQueueEntryCommand
        {
            TelegramId = TestTelegramId,
            ClassId = getClassesResult.Value.First().Id
        });
        
        // Act
        var deleteResult = await _sender.Send(new DeleteQueueEntryCommand
        {
            TelegramId = TestTelegramId,
            ClassId = getClassesResult.Value.First().Id
        });

        var queueAfterDelete = await _sender.Send(new GetClassQueueQuery { ClassId = getClassesResult.Value.First().Id });
        
        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(deleteResult.IsSuccess, Is.True);
            Assert.That(queueAfterDelete.Value, Has.Count.EqualTo(0));
        });
    }

    [Test]
    public async Task DeleteOutdatedQueues_WhenQueueOutdated_NonOutdatedQueuesAfterDelete()
    {
        // Arrange
        await CreateUserAndClasses();
        
        await _sender.Send(new CreateClassesCommand
        {
            Classes = new Dictionary<string, DateOnly> { { TestClassName + "_outdated", DateOnly.FromDateTime(DateTime.Now.AddDays(-1)) } },
            GroupName = TestGroupName
        });
        
        var getClassesResult = await _sender.Send(new GetClassesQuery { GroupName = TestGroupName });
        var outdatedClassId = getClassesResult.Value.Last().Id;
        
        await _sender.Send(new CreateQueueEntryCommand
        {
            TelegramId = TestTelegramId,
            ClassId = outdatedClassId
        });
        
        // Act
        var queueBeforeDelete = await _sender.Send(new GetClassQueueQuery { ClassId = outdatedClassId });
        
        var deleteResult = await _sender.Send(new DeleteQueueForClassCommand {ClassId = outdatedClassId});
        
        var queueAfterDelete = await _sender.Send(new GetClassQueueQuery { ClassId = outdatedClassId });
        
        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(deleteResult.IsSuccess, Is.True);
            Assert.That(queueAfterDelete.Value, Has.Count.EqualTo(queueBeforeDelete.Value.Count - 1));
        });
    }
    
    [Test]
    public async Task GetClassQueue_WhenClassExist_Queue()
    {
        // Arrange
        await CreateUserAndClasses();

        var getGroupResult = await _sender.Send(new GetGroupQuery { GroupName = TestGroupName });
        
        var getClassesResult = await _sender.Send(new GetClassesQuery { GroupName = TestGroupName });

        await _sender.Send(new CreateQueueEntryCommand
        {
            TelegramId = TestTelegramId,
            ClassId = getClassesResult.Value.First().Id
        });
        
        // Act
        var queue = await _sender.Send(new GetClassQueueQuery { ClassId = getClassesResult.Value.First().Id });
        
        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(getGroupResult.IsSuccess, Is.True);
            Assert.That(queue.Value, Has.Count.EqualTo(1));
        });
    }
    
    [Test]
    public async Task GetClassQueue_WhenClassNotExist_Fail()
    {
        // Arrange
        await CreateUserAndClasses();

        // Act
        var queue = await _sender.Send(new GetClassQueueQuery { ClassId = 1337 });
        
        // Assert
        Assert.That(queue.IsFailed, Is.True);
    }
    
    [Test]
    public async Task IsUserInQueue_WhenUserInQueue_True()
    {
        // Arrange
        await CreateUserAndClasses();
        
        var getClassesResult = await _sender.Send(new GetClassesQuery { GroupName = TestGroupName });

        await _sender.Send(new CreateQueueEntryCommand
        {
            TelegramId = TestTelegramId,
            ClassId = getClassesResult.Value.First().Id
        });

        // Act
        var isUserInQueue = await _sender.Send(new GetUserInQueueQuery
        {
            TelegramId = TestTelegramId,
            ClassId = getClassesResult.Value.First().Id
        });
        
        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(isUserInQueue.Value, Is.Not.Null);
        });
    }
    
    [Test]
    public async Task IsUserInQueue_WhenUserNotInQueue_False()
    {
        // Arrange
        await CreateUserAndClasses();
        
        var getClassesResult = await _sender.Send(new GetClassesQuery { GroupName = TestGroupName });

        // Act
        var isUserInQueue = await _sender.Send(new GetUserInQueueQuery
        {
            TelegramId = TestTelegramId,
            ClassId = getClassesResult.Value.First().Id
        });
        
        // Assert
        Assert.That(isUserInQueue.Value, Is.Null);
    }

    private async Task CreateUserAndClasses()
    {
        await _sender.Send(new CreateGroupsCommand
        {
            GroupNames = [TestGroupName]
        });

        await _sender.Send(new CreateUserCommand
        {
            TelegramId = TestTelegramId,
            FullName = TestFullName,
            GroupName = TestGroupName
        });

        await _sender.Send(new CreateClassesCommand
        {
            Classes = new Dictionary<string, DateOnly> { { TestClassName, DateOnly.FromDateTime(DateTime.Now.AddDays(1)) } },
            GroupName = TestGroupName
        });
    }
}
using DatabaseApp.Application.Class;
using DatabaseApp.Application.Class.Command.CreateClass;
using DatabaseApp.Application.Class.Queries.GetClasses;
using DatabaseApp.Application.Dto;
using DatabaseApp.Application.Group.Command.CreateGroup;
using DatabaseApp.Application.Queue;
using DatabaseApp.Application.Queue.Commands.CreateQueue;
using DatabaseApp.Application.Queue.Commands.DeleteQueue;
using DatabaseApp.Application.Queue.Queries.GetQueue;
using DatabaseApp.Application.Queue.Queries.GetUserInQueue;
using DatabaseApp.Application.User.Command.CreateUser;
using DatabaseApp.Domain.Repositories;
using DatabaseApp.Tests.TestContext;
using FluentResults;
using MediatR;
using Microsoft.Extensions.DependencyInjection;

namespace DatabaseApp.Tests.DatabaseTests;

public class QueueTests
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

        IServiceScope scope = _factory.Services.CreateScope();

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
    public async Task CreateQueue_WhenQueueNotExistAndOtherDataValid_ShouldReturnSuccess()
    {
        // Arrange
        await _sender.Send(new CreateGroupCommand
        {
            GroupName = TestGroupName
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

        Result<List<ClassDto>> getClassesResult = await _sender.Send(new GetClassesQuery { GroupName = TestGroupName });

        // Act
        Result createResult = await _sender.Send(new CreateQueueCommand
        {
            TelegramId = TestTelegramId,
            ClassId = getClassesResult.Value.First().Id
        });

        Result<List<QueueDto>> queue = await _sender.Send(new GetClassQueueQuery() { ClassId = getClassesResult.Value.First().Id });

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(createResult.IsSuccess, Is.True);
            Assert.That(queue.Value, Has.Count.EqualTo(1));
        });
    }
    
    [Test]
    public async Task CreateQueue_WhenUserNotExist_ShouldReturnFail()
    {
        // Arrange
        await _sender.Send(new CreateGroupCommand
        {
            GroupName = TestGroupName
        });

        await _sender.Send(new CreateClassesCommand
        {
            Classes = new Dictionary<string, DateOnly> { { TestClassName, DateOnly.FromDateTime(DateTime.Now.AddDays(1)) } },
            GroupName = TestGroupName
        });

        Result<List<ClassDto>> getClassesResult = await _sender.Send(new GetClassesQuery { GroupName = TestGroupName });

        // Act
        Result createResult = await _sender.Send(new CreateQueueCommand
        {
            TelegramId = TestTelegramId,
            ClassId = getClassesResult.Value.First().Id
        });

        Result<List<QueueDto>> queue = await _sender.Send(new GetClassQueueQuery { ClassId = getClassesResult.Value.First().Id });
        
        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(createResult.IsFailed, Is.True);
            Assert.That(queue.Value, Has.Count.EqualTo(0));
        });
    }
    
    [Test]
    public async Task CreateQueue_WhenClassNotExist_ShouldReturnFail()
    {
        // Arrange
        await _sender.Send(new CreateGroupCommand
        {
            GroupName = TestGroupName
        });

        await _sender.Send(new CreateUserCommand
        {
            TelegramId = TestTelegramId,
            FullName = TestFullName,
            GroupName = TestGroupName
        });

        // Act
        Result createResult = await _sender.Send(new CreateQueueCommand
        {
            TelegramId = TestTelegramId,
            ClassId = 1337
        });
        
        // Assert
        Assert.That(createResult.IsFailed, Is.True);
    }
    
    [Test]
    public async Task CreateQueue_WhenUserAlreadyInQueue_ShouldReturnFail()
    {
        // Arrange
        await CreateUserAndClasses();

        Result<List<ClassDto>> getClassesResult = await _sender.Send(new GetClassesQuery { GroupName = TestGroupName });

        await _sender.Send(new CreateQueueCommand
        {
            TelegramId = TestTelegramId,
            ClassId = getClassesResult.Value.First().Id
        });

        // Act
        Result createResult = await _sender.Send(new CreateQueueCommand
        {
            TelegramId = TestTelegramId,
            ClassId = getClassesResult.Value.First().Id
        });

        Result<List<QueueDto>> queue = await _sender.Send(new GetClassQueueQuery { ClassId = getClassesResult.Value.First().Id });
        
        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(createResult.IsFailed, Is.True);
            Assert.That(queue.Value, Has.Count.EqualTo(1));
        });
    }
    
    [Test]
    public async Task DeleteQueue_WhenQueueExist_ShouldReturnSuccess()
    {
        // Arrange
        await CreateUserAndClasses();

        Result<List<ClassDto>> getClassesResult = await _sender.Send(new GetClassesQuery { GroupName = TestGroupName });

        await _sender.Send(new CreateQueueCommand
        {
            TelegramId = TestTelegramId,
            ClassId = getClassesResult.Value.First().Id
        });

        Result<List<QueueDto>> queue = await _sender.Send(new GetClassQueueQuery { ClassId = getClassesResult.Value.First().Id });

        // Act
        Result deleteResult = await _sender.Send(new DeleteQueueCommand
        {
            OutdatedClassList = queue.Value.Select(x => x.ClassId).ToList()
        });

        Result<List<QueueDto>> queueAfterDelete = await _sender.Send(new GetClassQueueQuery { ClassId = getClassesResult.Value.First().Id });
        
        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(deleteResult.IsSuccess, Is.True);
            Assert.That(queueAfterDelete.Value, Has.Count.EqualTo(0));
        });
    }

    [Test]
    public async Task GetClassQueue_WhenClassExist_ShouldReturnQueue()
    {
        // Arrange
        await CreateUserAndClasses();

        Result<List<ClassDto>> getClassesResult = await _sender.Send(new GetClassesQuery { GroupName = TestGroupName });

        await _sender.Send(new CreateQueueCommand
        {
            TelegramId = TestTelegramId,
            ClassId = getClassesResult.Value.First().Id
        });
        
        // Act
        Result<List<QueueDto>> queue = await _sender.Send(new GetClassQueueQuery { ClassId = getClassesResult.Value.First().Id });
        
        // Assert
        Assert.That(queue.Value, Has.Count.EqualTo(1));
    }
    
    [Test]
    public async Task GetClassQueue_WhenClassNotExist_ShouldReturnFail()
    {
        // Arrange
        await CreateUserAndClasses();

        // Act
        Result<List<QueueDto>> queue = await _sender.Send(new GetClassQueueQuery { ClassId = 1337 });
        
        // Assert
        Assert.That(queue.IsFailed, Is.True);
    }
    
    [Test]
    public async Task IsUserInQueue_WhenUserInQueue_ShouldReturnTrue()
    {
        // Arrange
        await CreateUserAndClasses();

        Result<List<ClassDto>> getClassesResult = await _sender.Send(new GetClassesQuery { GroupName = TestGroupName });

        await _sender.Send(new CreateQueueCommand
        {
            TelegramId = TestTelegramId,
            ClassId = getClassesResult.Value.First().Id
        });

        // Act
        Result<UserDto?> isUserInQueue = await _sender.Send(new GetUserInQueueQuery
        {
            TelegramId = TestTelegramId,
            ClassId = getClassesResult.Value.First().Id
        });
        
        // Assert
        Assert.That(isUserInQueue.Value, Is.Not.Null);
    }
    
    [Test]
    public async Task IsUserInQueue_WhenUserNotInQueue_ShouldReturnFalse()
    {
        // Arrange
        await CreateUserAndClasses();

        Result<List<ClassDto>> getClassesResult = await _sender.Send(new GetClassesQuery { GroupName = TestGroupName });

        // Act
        Result<UserDto?> isUserInQueue = await _sender.Send(new GetUserInQueueQuery
        {
            TelegramId = TestTelegramId,
            ClassId = getClassesResult.Value.First().Id
        });
        
        // Assert
        Assert.That(isUserInQueue.Value, Is.Null);
    }

    private async Task CreateUserAndClasses()
    {
        await _sender.Send(new CreateGroupCommand
        {
            GroupName = TestGroupName
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
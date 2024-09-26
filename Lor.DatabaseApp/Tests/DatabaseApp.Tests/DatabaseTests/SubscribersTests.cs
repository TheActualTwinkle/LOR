/*using DatabaseApp.Application.Group.Command.CreateGroup;
using DatabaseApp.Application.Subscriber;
using DatabaseApp.Application.Subscriber.Command.CreateSubscriber;
using DatabaseApp.Application.Subscriber.Command.DeleteSubscriber;
using DatabaseApp.Application.Subscriber.Queries.GetSubscribers;
using DatabaseApp.Application.User.Command.CreateUser;
using DatabaseApp.Domain.Repositories;
using DatabaseApp.Tests.TestContext;
using FluentResults;
using MediatR;
using Microsoft.Extensions.DependencyInjection;

namespace DatabaseApp.Tests.DatabaseTests;

public class SubscribersTests
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
    public async Task CreateSubscriber_WhenUserExistAndNotSubscribed_ShouldReturnSuccess()
    {
        // Arrange
        await CreateUserAndGroup();
        
        // Act
        Result result = await _sender.Send(new CreateSubscriberCommand
        {
            TelegramId = TestTelegramId
        });

        Result<List<SubscriberDto>> subscribers = await _sender.Send(new GetAllSubscribersQuery());

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(result.IsSuccess, Is.True);
            Assert.That(subscribers.Value, Has.Count.EqualTo(1));
            Assert.That(subscribers.Value.First().TelegramId, Is.EqualTo(TestTelegramId));
        });
    }
    
    [Test]
    public async Task CreateSubscriber_WhenUserExistAndAlreadySubscribed_ShouldReturnFailure()
    {
        // Arrange
        await CreateUserAndGroup();
        
        await _sender.Send(new CreateSubscriberCommand
        {
            TelegramId = TestTelegramId
        });

        // Act
        Result result = await _sender.Send(new CreateSubscriberCommand
        {
            TelegramId = TestTelegramId
        });

        Result<List<SubscriberDto>> subscribers = await _sender.Send(new GetAllSubscribersQuery());

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(result.IsFailed, Is.True);
            Assert.That(subscribers.Value, Has.Count.EqualTo(1));
            Assert.That(subscribers.Value.First().TelegramId, Is.EqualTo(TestTelegramId));
        });
    }
    
    [Test]
    public async Task CreateSubscriber_WhenUserNotExist_ShouldReturnFailure()
    {
        // Arrange
        await CreateUserAndGroup();
        
        // Act
        Result result = await _sender.Send(new CreateSubscriberCommand
        {
            TelegramId = 987654321
        });

        Result<List<SubscriberDto>> subscribers = await _sender.Send(new GetAllSubscribersQuery());

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(result.IsFailed, Is.True);
            Assert.That(subscribers.Value, Has.Count.EqualTo(0));
        });
    }
    
    [Test]
    public async Task DeleteSubscriber_WhenUserExistAndSubscribed_ShouldReturnSuccess()
    {
        // Arrange
        await CreateUserAndGroup();
        
        await _sender.Send(new CreateSubscriberCommand
        {
            TelegramId = TestTelegramId
        });

        // Act
        Result result = await _sender.Send(new DeleteSubscriberCommand
        {
            TelegramId = TestTelegramId
        });

        Result<List<SubscriberDto>> subscribers = await _sender.Send(new GetAllSubscribersQuery());

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(result.IsSuccess, Is.True);
            Assert.That(subscribers.Value, Has.Count.EqualTo(0));
        });
    }
    
    [Test]
    public async Task DeleteSubscriber_WhenUserExistAndNotSubscribed_ShouldReturnFailure()
    {
        // Arrange
        await CreateUserAndGroup();
        
        // Act
        Result result = await _sender.Send(new DeleteSubscriberCommand
        {
            TelegramId = TestTelegramId
        });

        Result<List<SubscriberDto>> subscribers = await _sender.Send(new GetAllSubscribersQuery());

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(result.IsFailed, Is.True);
            Assert.That(subscribers.Value, Has.Count.EqualTo(0));
        });
    }
    
    [Test]
    public async Task DeleteSubscriber_WhenUserNotExist_ShouldReturnFailure()
    {
        // Arrange
        await CreateUserAndGroup();
        
        await _sender.Send(new CreateSubscriberCommand
        {
            TelegramId = TestTelegramId
        });

        // Act
        Result result = await _sender.Send(new DeleteSubscriberCommand
        {
            TelegramId = 987654321
        });

        Result<List<SubscriberDto>> subscribers = await _sender.Send(new GetAllSubscribersQuery());

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(result.IsFailed, Is.True);
            Assert.That(subscribers.Value, Has.Count.EqualTo(1));
        });
    }
    
    [Test]
    public async Task GetAllSubscribers_WhenSubscribersExist_ShouldReturnSuccess()
    {
        // Arrange
        await CreateUserAndGroup();
        
        await _sender.Send(new CreateSubscriberCommand
        {
            TelegramId = TestTelegramId
        });

        // Act
        Result<List<SubscriberDto>> result = await _sender.Send(new GetAllSubscribersQuery());

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(result.IsSuccess, Is.True);
            Assert.That(result.Value, Has.Count.EqualTo(1));
            Assert.That(result.Value.First().TelegramId, Is.EqualTo(TestTelegramId));
        });
    }
    
    [Test]
    public async Task GetAllSubscribers_WhenSubscribersIsEmpty_ShouldReturnSuccess()
    {
        // Arrange
        await CreateUserAndGroup();
        
        // Act
        Result<List<SubscriberDto>> result = await _sender.Send(new GetAllSubscribersQuery());

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(result.IsSuccess, Is.True);
            Assert.That(result.Value, Has.Count.EqualTo(0));
        });
    }
    
    private async Task CreateUserAndGroup()
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
    }
}*/
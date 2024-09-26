/*using DatabaseApp.Application.Group.Command.CreateGroup;
using DatabaseApp.Application.User;
using DatabaseApp.Application.User.Command.CreateUser;
using DatabaseApp.Application.User.Queries.GetUserInfo;
using DatabaseApp.Domain.Repositories;
using DatabaseApp.Tests.TestContext;
using FluentResults;
using MediatR;
using Microsoft.Extensions.DependencyInjection;

namespace DatabaseApp.Tests.DatabaseTests;

public class UserTests
{
    private readonly WebAppFactory _factory = new();
    
    private ISender _sender;
    private IUnitOfWork _unitOfWork;

    private const long TestTelegramId = 123456789;
    private const string TestFullName = "John Doe";
    private const string TestGroupName = "ОО-АА";
    
    [OneTimeSetUp]
    public async Task OneTimeSetup()
    {
        await _factory.InitializeAsync();

        IServiceScope scope = _factory.Services.CreateScope();

        _sender = scope.ServiceProvider.GetRequiredService<ISender>();
        _unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
    }

    [SetUp]
    public async Task Setup()
    {
        await _sender.Send(new CreateGroupCommand
        {
            GroupName = TestGroupName
        });
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
    public async Task CreateUser_WhenUserNotExist_ShouldReturnUser()
    {
        // Arrange
        
        // Act
        Result setResult = await _sender.Send(new CreateUserCommand
        {
            TelegramId = TestTelegramId,
            FullName = TestFullName,
            GroupName = TestGroupName
        });
        
        Result<UserDto> getResult = await _sender.Send(new GetUserInfoQuery
        {
            TelegramId = TestTelegramId
        });
        
        // Assert
        Assert.That(setResult.IsSuccess && getResult.IsSuccess, Is.True);
    }

    [Test]
    public async Task CreateUser_WhenUserExist_ShouldReturnFail()
    {
        // Arrange
        Result firstResult = await _sender.Send(new CreateUserCommand
        {
            TelegramId = TestTelegramId,
            FullName = TestFullName,
            GroupName = TestGroupName
        });
        
        // Act
        Result secondResult = await _sender.Send(new CreateUserCommand
        {
            TelegramId = TestTelegramId,
            FullName = TestFullName,
            GroupName = TestGroupName
        });
        
        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(firstResult.IsSuccess, Is.True);
            Assert.That(secondResult.IsFailed, Is.True);
        });
    }
    
    [Test]
    public async Task CreateUser_WhenGroupNotExist_ShouldReturnFail()
    {
        // Act
        Result createResult = await _sender.Send(new CreateUserCommand
        {
            TelegramId = TestTelegramId,
            FullName = TestFullName,
            GroupName = "Nonexistent group"
        });

        Result<UserDto> getResult = await _sender.Send(new GetUserInfoQuery
        {
            TelegramId = TestTelegramId
        });

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(createResult.IsFailed, Is.True);
            Assert.That(getResult.IsFailed, Is.True);
        });
    }

    [Test]
    public async Task GetUserInfo_WhenUserExist_ShouldReturnUser()
    {
        // Arrange
        Result createUserResult = await _sender.Send(new CreateUserCommand
        {
            TelegramId = TestTelegramId,
            FullName = TestFullName,
            GroupName = TestGroupName
        });
        
        // Act
        Result<UserDto> result = await _sender.Send(new GetUserInfoQuery
        {
            TelegramId = TestTelegramId
        });
        
        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(createUserResult.IsSuccess, Is.True);
            Assert.That(result.IsSuccess, Is.True);
            Assert.That(result.Value.FullName, Is.EqualTo(TestFullName));
        });
    }
    
    [Test]
    public async Task GetUserInfo_WhenUserNotExist_ShouldReturnFail()
    {
        // Act
        Result<UserDto> result = await _sender.Send(new GetUserInfoQuery
        {
            TelegramId = TestTelegramId
        });
        
        // Assert
        Assert.That(result.IsFailed, Is.True);
    }
}*/
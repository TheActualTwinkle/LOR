using DatabaseApp.Application.Group.Command.CreateGroup;
using DatabaseApp.Application.User.Command.CreateUser;
using DatabaseApp.Application.User.Queries.GetUserInfo;
using DatabaseApp.Domain.Repositories;
using DatabaseApp.Tests.TestContext;
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

        var scope = _factory.Services.CreateScope();

        _sender = scope.ServiceProvider.GetRequiredService<ISender>();
        _unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
    }

    [SetUp]
    public async Task Setup()
    {
        await _sender.Send(new CreateGroupsCommand
        {
            GroupNames = [TestGroupName]
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
    public async Task CreateUser_WhenUserNotExist_User()
    {
        // Arrange
        
        // Act
        var setResult = await _sender.Send(new CreateUserCommand
        {
            TelegramId = TestTelegramId,
            FullName = TestFullName,
            GroupName = TestGroupName
        });
        
        var getResult = await _sender.Send(new GetUserInfoQuery
        {
            TelegramId = TestTelegramId
        });
        
        // Assert
        Assert.That(setResult.IsSuccess && getResult.IsSuccess, Is.True);
    }

    [Test]
    public async Task CreateUser_WhenUserExist_Fail()
    {
        // Arrange
        var firstResult = await _sender.Send(new CreateUserCommand
        {
            TelegramId = TestTelegramId,
            FullName = TestFullName,
            GroupName = TestGroupName
        });
        
        // Act
        var secondResult = await _sender.Send(new CreateUserCommand
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
    public async Task CreateUser_WhenGroupNotExist_Fail()
    {
        // Act
        var createResult = await _sender.Send(new CreateUserCommand
        {
            TelegramId = TestTelegramId,
            FullName = TestFullName,
            GroupName = "Nonexistent group"
        });

        var getResult = await _sender.Send(new GetUserInfoQuery
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
    public async Task GetUserInfo_WhenUserExist_User()
    {
        // Arrange
        var createUserResult = await _sender.Send(new CreateUserCommand
        {
            TelegramId = TestTelegramId,
            FullName = TestFullName,
            GroupName = TestGroupName
        });
        
        // Act
        var result = await _sender.Send(new GetUserInfoQuery
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
    public async Task GetUserInfo_WhenUserNotExist_Fail()
    {
        // Act
        var result = await _sender.Send(new GetUserInfoQuery
        {
            TelegramId = TestTelegramId
        });
        
        // Assert
        Assert.That(result.IsFailed, Is.True);
    }
}
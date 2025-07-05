using DatabaseApp.Application.Groups.Command.CreateGroup;
using DatabaseApp.Application.Groups.Queries;
using DatabaseApp.Domain.Repositories;
using DatabaseApp.Tests.TestContext;
using MediatR;
using Microsoft.Extensions.DependencyInjection;

namespace DatabaseApp.Tests.DatabaseTests;

public class GroupTests
{
    private readonly WebAppFactory _factory = new();
    
    private ISender _sender;
    private IUnitOfWork _unitOfWork;
    
    private const string TestGroupName = "ОО-АА";
    
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
    public async Task CreateGroup_WhenGroupNotExist_Success()
    {
        // Act
        var createResult = await _sender.Send(new CreateGroupsCommand
        {
            GroupNames = [TestGroupName]
        });

        var getResult = await _sender.Send(new GetGroupQuery
        {
            GroupName = TestGroupName
        });

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(createResult.IsSuccess, Is.True);
            Assert.That(getResult.IsSuccess && getResult.Value.GroupName == TestGroupName, Is.True);
        });
    }
    
    [Test]
    public async Task CreateGroup_WhenGroupExist_Fail()
    {
        // Arrange
        var firstResult = await _sender.Send(new CreateGroupsCommand
        {
            GroupNames = [TestGroupName]
        });

        // Act
        var secondResult = await _sender.Send(new CreateGroupsCommand
        {
            GroupNames = [TestGroupName]
        });
        
        // Assert

        var getGroups = await _sender.Send(new GetGroupsQuery());

        Assert.Multiple(() =>
        {
            Assert.That(firstResult.IsSuccess, Is.True);
            Assert.That(secondResult.IsSuccess, Is.True);
            Assert.That(getGroups.IsSuccess, Is.True);
            Assert.That(getGroups.Value, Has.Count.EqualTo(1));
        });
    }
    
    [Test]
    public async Task GetGroup_WhenGroupExist_Group()
    {
        // Arrange
        await _sender.Send(new CreateGroupsCommand
        {
            GroupNames = [TestGroupName]
        });
        
        // Act
        var getResult = await _sender.Send(new GetGroupQuery
        {
            GroupName = TestGroupName
        });
        
        // Assert
        Assert.That(getResult.IsSuccess && getResult.Value.GroupName == TestGroupName, Is.True);
    }
    
    [Test]
    public async Task GetGroup_WhenGroupNotExist_Fail()
    {
        // Act
        var getResult = await _sender.Send(new GetGroupQuery
        {
            GroupName = TestGroupName
        });
        
        // Assert
        Assert.That(getResult.IsFailed, Is.True);
    }
    
    
    [Test]
    public async Task GetGroups_WhenGroupsExist_ListOfGroups()
    {
        // Arrange
        await _sender.Send(new CreateGroupsCommand
        {
            GroupNames = [TestGroupName, TestGroupName  + "_1"]
        });
        
        // Act
        var getResult = await _sender.Send(new GetGroupsQuery());
        
        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(getResult.IsSuccess, Is.True);
            Assert.That(getResult.Value, Has.Count.EqualTo(2));
        });
    }
    
    [Test]
    public async Task GetGroups_WhenGroupsNotExist_EmptyList()
    {
        // Act
        var getResult = await _sender.Send(new GetGroupsQuery());
        
        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(getResult.IsSuccess, Is.True);
            Assert.That(getResult.Value, Has.Count.EqualTo(0));
        });
    }
}
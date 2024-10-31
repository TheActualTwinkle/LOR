using DatabaseApp.Application.Class.Command.CreateClass;
using DatabaseApp.Application.Class.Command.DeleteClass;
using DatabaseApp.Application.Class.Queries.GetClass;
using DatabaseApp.Application.Class.Queries.GetClasses;
using DatabaseApp.Application.Class.Queries.GetOutdatedClasses;
using DatabaseApp.Application.Group.Command.CreateGroup;
using DatabaseApp.Application.Group.Queries.GetGroup;
using DatabaseApp.Domain.Repositories;
using DatabaseApp.Tests.TestContext;
using MediatR;
using Microsoft.Extensions.DependencyInjection;

namespace DatabaseApp.Tests.DatabaseTests;

public class ClassTests
{
    private readonly WebAppFactory _factory = new();
    
    private ISender _sender;
    private IUnitOfWork _unitOfWork;
    
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
    public async Task CreateClasses_WhenClassNotExist_ListOfClasses()
    {
        var groupDto = await _sender.Send(new GetGroupQuery
        {
            GroupName = TestGroupName
        });
        // Act
        var createResult = await _sender.Send(new CreateClassesCommand
        {
            Classes = new Dictionary<string, DateOnly> { { TestClassName, DateOnly.FromDateTime(DateTime.Now.AddDays(1)) } },
            GroupId = groupDto.Value.Id
        });

        var getGroupResult = await _sender.Send(new GetGroupQuery { GroupName = TestGroupName });

        var getResult = await _sender.Send(new GetClassesQuery {GroupId = getGroupResult.Value.Id});

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(createResult.IsSuccess, Is.True);
            Assert.That(getGroupResult.IsSuccess, Is.True);
            Assert.That(getResult.Value, Has.Count.EqualTo(1));
            Assert.That(getResult.Value.First().Name, Is.EqualTo(TestClassName));
        });
    }
    
    [Test]
    public async Task CreateClasses_WhenClassExist_DistinctClasses()
    {
        var groupDto = await _sender.Send(new GetGroupQuery
        {
            GroupName = TestGroupName
        });
        
        // Arrange
        var createResult1 = await _sender.Send(new CreateClassesCommand
        {
            Classes = new Dictionary<string, DateOnly> { { TestClassName, DateOnly.FromDateTime(DateTime.Now.AddDays(1)) } },
            GroupId = groupDto.Value.Id
        });

        // Act
        var createResult2 = await _sender.Send(new CreateClassesCommand
        {
            Classes = new Dictionary<string, DateOnly> { { TestClassName, DateOnly.FromDateTime(DateTime.Now.AddDays(1)) } },
            GroupId = groupDto.Value.Id
        });
        
        var getGroupResult = await _sender.Send(new GetGroupQuery { GroupName = TestGroupName });
        
        var getResult = await _sender.Send(new GetClassesQuery {GroupId = getGroupResult.Value.Id});

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(createResult1.IsSuccess, Is.True);
            Assert.That(createResult2.IsSuccess, Is.True);
            Assert.That(getGroupResult.IsSuccess, Is.True);
            Assert.That(getResult.Value, Has.Count.EqualTo(1));
            Assert.That(getResult.Value.First().Name, Is.EqualTo(TestClassName));
        });
    }
    
    [Test]
    public async Task DeleteClass_WhenClassExist_SuccessAndDatabaseContainEmptyClasses()
    {
        var groupDto = await _sender.Send(new GetGroupQuery
        {
            GroupName = TestGroupName
        });
        
        // Arrange
        var createResult = await _sender.Send(new CreateClassesCommand
        {
            Classes = new Dictionary<string, DateOnly> { { TestClassName, DateOnly.FromDateTime(DateTime.Now.AddDays(1)) } },
            GroupId = groupDto.Value.Id
        });
        
        var getGroupResult = await _sender.Send(new GetGroupQuery { GroupName = TestGroupName });
        
        var classes = await _sender.Send(new GetClassesQuery {GroupId = getGroupResult.Value.Id});
        
        // Act
        var deleteResult = await _sender.Send(new DeleteClassCommand
        {
            ClassesId = [classes.Value.First().Id]
        });
        
        var getResult = await _sender.Send(new GetClassesQuery {GroupId = getGroupResult.Value.Id});

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(createResult.IsSuccess, Is.True);
            Assert.That(deleteResult.IsSuccess, Is.True);
            Assert.That(getGroupResult.IsSuccess, Is.True);
            Assert.That(getResult.IsSuccess, Is.True);
            Assert.That(getResult.Value, Has.Count.EqualTo(0));
        });
    }
    
    [Test]
    public async Task DeleteClass_WhenClassNotExist_Fail()
    {
        // Act
        var deleteResult = await _sender.Send(new DeleteClassCommand
        {
            ClassesId = [99999985]
        });
        
        // Assert
        Assert.That(deleteResult.IsFailed, Is.True);
    }
    
    [Test]
    public async Task GetClass_WhenClassExist_Class()
    {
        var groupDto = await _sender.Send(new GetGroupQuery
        {
            GroupName = TestGroupName
        });
        
        // Arrange
        var createResult = await _sender.Send(new CreateClassesCommand
        {
            Classes = new Dictionary<string, DateOnly> { { TestClassName, DateOnly.FromDateTime(DateTime.Now.AddDays(1)) } },
            GroupId = groupDto.Value.Id
        });
        
        var getGroupResult = await _sender.Send(new GetGroupQuery { GroupName = TestGroupName });
        
        var classes = await _sender.Send(new GetClassesQuery {GroupId = getGroupResult.Value.Id});
        
        // Act
        var result = await _sender.Send(new GetClassQuery
        {
            ClassId = classes.Value.First().Id
        });
        
        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(createResult.IsSuccess, Is.True);
            Assert.That(getGroupResult.IsSuccess, Is.True);
            Assert.That(result.IsSuccess, Is.True);
            Assert.That(result.Value.Name, Is.EqualTo(TestClassName));
        });
    }
    
    [Test]
    public async Task GetClass_WhenClassNotExist_Fail()
    {
        // Act
        var result = await _sender.Send(new GetClassQuery
        {
            ClassId = 99999985
        });
        
        var getGroupResult = await _sender.Send(new GetGroupQuery { GroupName = TestGroupName });
        
        var classes = await _sender.Send(new GetClassesQuery {GroupId = getGroupResult.Value.Id});
        
        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(result.IsFailed, Is.True);
            Assert.That(getGroupResult.IsSuccess, Is.True);
            Assert.That(classes.Value, Has.Count.EqualTo(0));
        });
    }
    
    [Test]
    public async Task GetClasses_WhenClassesExist_ListOfClasses()
    {
        var groupDto = await _sender.Send(new GetGroupQuery
        {
            GroupName = TestGroupName
        });
        
        // Arrange
        await _sender.Send(new CreateClassesCommand
        {
            Classes = new Dictionary<string, DateOnly> { { TestClassName, DateOnly.FromDateTime(DateTime.Now.AddDays(1)) } },
            GroupId = groupDto.Value.Id
        });
        
        await _sender.Send(new CreateClassesCommand
        {
            Classes = new Dictionary<string, DateOnly> { { TestClassName + "_1", DateOnly.FromDateTime(DateTime.Now.AddDays(1)) } },
            GroupId = groupDto.Value.Id
        });
        
        var getGroupResult = await _sender.Send(new GetGroupQuery { GroupName = TestGroupName });
        
        // Act
        var getResult = await _sender.Send(new GetClassesQuery {GroupId = getGroupResult.Value.Id});
        
        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(getResult.IsSuccess, Is.True);
            Assert.That(getGroupResult.IsSuccess, Is.True);
            Assert.That(getResult.Value, Has.Count.EqualTo(2));
        });
    }
    
    [Test]
    public async Task GetClasses_WhenClassesNotExist_EmptyList()
    {
        var getGroupResult = await _sender.Send(new GetGroupQuery { GroupName = TestGroupName });
        
        // Act
        var getResult = await _sender.Send(new GetClassesQuery {GroupId = getGroupResult.Value.Id});
        
        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(getGroupResult.IsSuccess, Is.True);
            Assert.That(getResult.IsSuccess, Is.True);
            Assert.That(getResult.Value, Has.Count.EqualTo(0));
        });
    }
    
    [Test]
    public async Task GetOutdatedClasses_WhenOutdatedClassesExist_ListOfClasses()
    {
        var groupDto = await _sender.Send(new GetGroupQuery
        {
            GroupName = TestGroupName
        });
        
        // Arrange
        await _sender.Send(new CreateClassesCommand
        {
            Classes = new Dictionary<string, DateOnly> { { TestClassName, DateOnly.FromDateTime(DateTime.Now.AddDays(-2)) } },
            GroupId = groupDto.Value.Id
        });
        
        await _sender.Send(new CreateClassesCommand
        {
            Classes = new Dictionary<string, DateOnly> { { TestClassName + "_1", DateOnly.FromDateTime(DateTime.Now.AddDays(2)) } },
            GroupId = groupDto.Value.Id
        });
        
        // Act
        var getOutdatedClassesResult = await _sender.Send(new GetOutdatedClassesQuery());
        
        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(getOutdatedClassesResult.IsSuccess, Is.True);
            Assert.That(getOutdatedClassesResult.Value, Has.Count.EqualTo(1));
        });
    }
    
    [Test]
    public async Task GetOutdatedClasses_WhenOutdatedClassesNotExist_EmptyList()
    {
        // Act
        var getResult = await _sender.Send(new GetOutdatedClassesQuery());
        
        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(getResult.IsSuccess, Is.True);
            Assert.That(getResult.Value, Has.Count.EqualTo(0));
        });
    }
}
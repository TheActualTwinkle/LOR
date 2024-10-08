﻿using DatabaseApp.Application.Class;
using DatabaseApp.Application.Class.Command.CreateClass;
using DatabaseApp.Application.Class.Command.DeleteClass;
using DatabaseApp.Application.Class.Queries.GetClass;
using DatabaseApp.Application.Class.Queries.GetClasses;
using DatabaseApp.Application.Class.Queries.GetOutdatedClasses;
using DatabaseApp.Application.Group;
using DatabaseApp.Application.Group.Command.CreateGroup;
using DatabaseApp.Application.Group.Queries.GetGroup;
using DatabaseApp.Domain.Repositories;
using DatabaseApp.Tests.TestContext;
using FluentResults;
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

        IServiceScope scope = _factory.Services.CreateScope();

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
    public async Task CreateClasses_WhenClassNotExist_ShouldReturnListOfClasses()
    {
        Result<GroupDto> groupDto = await _sender.Send(new GetGroupQuery
        {
            GroupName = TestGroupName
        });
        // Act
        Result createResult = await _sender.Send(new CreateClassesCommand
        {
            Classes = new Dictionary<string, DateOnly> { { TestClassName, DateOnly.FromDateTime(DateTime.Now.AddDays(1)) } },
            GroupId = groupDto.Value.Id
        });

        Result<GroupDto> getGroupResult = await _sender.Send(new GetGroupQuery { GroupName = TestGroupName });

        Result<List<ClassDto>> getResult = await _sender.Send(new GetClassesQuery {GroupId = getGroupResult.Value.Id});

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
    public async Task CreateClasses_WhenClassExist_ShouldReturnDistinctClasses()
    {
        Result<GroupDto> groupDto = await _sender.Send(new GetGroupQuery
        {
            GroupName = TestGroupName
        });
        
        // Arrange
        Result createResult1 = await _sender.Send(new CreateClassesCommand
        {
            Classes = new Dictionary<string, DateOnly> { { TestClassName, DateOnly.FromDateTime(DateTime.Now.AddDays(1)) } },
            GroupId = groupDto.Value.Id
        });

        // Act
        Result createResult2 = await _sender.Send(new CreateClassesCommand
        {
            Classes = new Dictionary<string, DateOnly> { { TestClassName, DateOnly.FromDateTime(DateTime.Now.AddDays(1)) } },
            GroupId = groupDto.Value.Id
        });
        
        Result<GroupDto> getGroupResult = await _sender.Send(new GetGroupQuery { GroupName = TestGroupName });
        
        Result<List<ClassDto>> getResult = await _sender.Send(new GetClassesQuery {GroupId = getGroupResult.Value.Id});

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
    public async Task DeleteClass_WhenClassExist_ShouldReturnSuccessAndDatabaseContainEmptyClasses()
    {
        Result<GroupDto> groupDto = await _sender.Send(new GetGroupQuery
        {
            GroupName = TestGroupName
        });
        
        // Arrange
        Result createResult = await _sender.Send(new CreateClassesCommand
        {
            Classes = new Dictionary<string, DateOnly> { { TestClassName, DateOnly.FromDateTime(DateTime.Now.AddDays(1)) } },
            GroupId = groupDto.Value.Id
        });
        
        Result<GroupDto> getGroupResult = await _sender.Send(new GetGroupQuery { GroupName = TestGroupName });
        
        Result<List<ClassDto>> classes = await _sender.Send(new GetClassesQuery {GroupId = getGroupResult.Value.Id});
        
        // Act
        Result deleteResult = await _sender.Send(new DeleteClassCommand
        {
            ClassesId = [classes.Value.First().Id]
        });
        
        Result<List<ClassDto>> getResult = await _sender.Send(new GetClassesQuery {GroupId = getGroupResult.Value.Id});

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
    public async Task DeleteClass_WhenClassNotExist_ShouldReturnFail()
    {
        // Act
        Result deleteResult = await _sender.Send(new DeleteClassCommand
        {
            ClassesId = [99999985]
        });
        
        // Assert
        Assert.That(deleteResult.IsFailed, Is.True);
    }
    
    [Test]
    public async Task GetClass_WhenClassExist_ShouldReturnClass()
    {
        Result<GroupDto> groupDto = await _sender.Send(new GetGroupQuery
        {
            GroupName = TestGroupName
        });
        
        // Arrange
        Result createResult = await _sender.Send(new CreateClassesCommand
        {
            Classes = new Dictionary<string, DateOnly> { { TestClassName, DateOnly.FromDateTime(DateTime.Now.AddDays(1)) } },
            GroupId = groupDto.Value.Id
        });
        
        Result<GroupDto> getGroupResult = await _sender.Send(new GetGroupQuery { GroupName = TestGroupName });
        
        Result<List<ClassDto>> classes = await _sender.Send(new GetClassesQuery {GroupId = getGroupResult.Value.Id});
        
        // Act
        Result<ClassDto> result = await _sender.Send(new GetClassQuery
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
    public async Task GetClass_WhenClassNotExist_ShouldReturnFail()
    {
        // Act
        Result<ClassDto> result = await _sender.Send(new GetClassQuery
        {
            ClassId = 99999985
        });
        
        Result<GroupDto> getGroupResult = await _sender.Send(new GetGroupQuery { GroupName = TestGroupName });
        
        Result<List<ClassDto>> classes = await _sender.Send(new GetClassesQuery {GroupId = getGroupResult.Value.Id});
        
        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(result.IsFailed, Is.True);
            Assert.That(getGroupResult.IsSuccess, Is.True);
            Assert.That(classes.Value, Has.Count.EqualTo(0));
        });
    }
    
    [Test]
    public async Task GetClasses_WhenClassesExist_ShouldReturnListOfClasses()
    {
        Result<GroupDto> groupDto = await _sender.Send(new GetGroupQuery
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
        
        Result<GroupDto> getGroupResult = await _sender.Send(new GetGroupQuery { GroupName = TestGroupName });
        
        // Act
        Result<List<ClassDto>> getResult = await _sender.Send(new GetClassesQuery {GroupId = getGroupResult.Value.Id});
        
        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(getResult.IsSuccess, Is.True);
            Assert.That(getGroupResult.IsSuccess, Is.True);
            Assert.That(getResult.Value, Has.Count.EqualTo(2));
        });
    }
    
    [Test]
    public async Task GetClasses_WhenClassesNotExist_ShouldReturnEmptyList()
    {
        Result<GroupDto> getGroupResult = await _sender.Send(new GetGroupQuery { GroupName = TestGroupName });
        
        // Act
        Result<List<ClassDto>> getResult = await _sender.Send(new GetClassesQuery {GroupId = getGroupResult.Value.Id});
        
        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(getGroupResult.IsSuccess, Is.True);
            Assert.That(getResult.IsSuccess, Is.True);
            Assert.That(getResult.Value, Has.Count.EqualTo(0));
        });
    }
    
    [Test]
    public async Task GetOutdatedClasses_WhenOutdatedClassesExist_ShouldReturnListOfClasses()
    {
        Result<GroupDto> groupDto = await _sender.Send(new GetGroupQuery
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
        Result<List<int>> getOutdatedClassesResult = await _sender.Send(new GetOutdatedClassesQuery());
        
        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(getOutdatedClassesResult.IsSuccess, Is.True);
            Assert.That(getOutdatedClassesResult.Value, Has.Count.EqualTo(1));
        });
    }
    
    [Test]
    public async Task GetOutdatedClasses_WhenOutdatedClassesNotExist_ShouldReturnEmptyList()
    {
        // Act
        Result<List<int>> getResult = await _sender.Send(new GetOutdatedClassesQuery());
        
        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(getResult.IsSuccess, Is.True);
            Assert.That(getResult.Value, Has.Count.EqualTo(0));
        });
    }
}
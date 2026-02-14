using FluentAssertions;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using TaskManagement.Api.Data;
using TaskManagement.Api.Data.Repositories;
using TaskManagement.Api.Domain.Projects;
using TaskManagement.Api.Domain.Tasks;
using TaskManagement.Api.Domain.Users;
using TaskManagement.Api.Features.Tasks.CreateTask;
using TaskManagement.Api.Features.Tasks.GetTask;
using TaskManagement.Api.Features.Tasks.UpdateTaskStatus;
using TaskManagement.Api.Hubs;
using Xunit;
using DomainTask = TaskManagement.Api.Domain.Tasks.Task;
using DomainTaskStatus = TaskManagement.Api.Domain.Tasks.TaskStatus;

namespace TaskManagement.Api.Tests.Features.Tasks;

/// <summary>
/// Unit tests for task CRUD operations and authorization checks.
/// Tests cover: create, read, update status with proper authorization validation.
/// </summary>
public class TaskServiceTests
{
    private readonly TaskManagementDbContext _context;
    private readonly Mock<ITaskRepository> _taskRepositoryMock;
    private readonly Mock<IProjectRepository> _projectRepositoryMock;
    private readonly Mock<ILogger<CreateTaskService>> _createTaskLoggerMock;
    private readonly Mock<ILogger<UpdateTaskStatusService>> _updateStatusLoggerMock;
    private readonly Mock<IHubContext<TaskManagementHub>> _hubContextMock;

    public TaskServiceTests()
    {
        var options = new DbContextOptionsBuilder<TaskManagementDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        _context = new TaskManagementDbContext(options);
        _taskRepositoryMock = new Mock<ITaskRepository>();
        _projectRepositoryMock = new Mock<IProjectRepository>();
        _createTaskLoggerMock = new Mock<ILogger<CreateTaskService>>();
        _updateStatusLoggerMock = new Mock<ILogger<UpdateTaskStatusService>>();
        _hubContextMock = new Mock<IHubContext<TaskManagementHub>>();
    }

    #region CreateTaskService Tests

    [Fact]
    public async System.Threading.Tasks.Task CreateTask_WithValidRequest_ShouldSucceed()
    {
        // Arrange
        var projectId = 1;
        var userId = "user123";
        var request = new CreateTaskRequest
        {
            Title = "Test Task",
            Description = "A test task description",
            Priority = TaskPriority.High,
            DueDate = DateTime.UtcNow.AddDays(7)
        };

        var project = new Project { Id = projectId, Name = "Test Project", OwnerId = userId };

        _projectRepositoryMock.Setup(r => r.GetByIdAsync(projectId, default))
            .ReturnsAsync(project);

        _taskRepositoryMock
            .Setup(r => r.CreateAsync(It.IsAny<DomainTask>(), default))
            .ReturnsAsync((DomainTask task, CancellationToken ct) =>
            {
                task.Id = 1;
                return task;
            });

        var service = new CreateTaskService(
            _context,
            _projectRepositoryMock.Object,
            _taskRepositoryMock.Object,
            _hubContextMock.Object,
            _createTaskLoggerMock.Object);

        // Act
        var result = await service.CreateTaskAsync(projectId, userId, request);

        // Assert
        result.Should().NotBeNull();
        result.Title.Should().Be("Test Task");
        result.Status.Should().Be(DomainTaskStatus.ToDo);
        result.Priority.Should().Be(TaskPriority.High);
        _taskRepositoryMock.Verify(r => r.CreateAsync(It.IsAny<DomainTask>(), default), Times.Once);
    }

    [Fact]
    public async System.Threading.Tasks.Task CreateTask_WithNonexistentProject_ShouldThrowKeyNotFound()
    {
        // Arrange
        var projectId = 999;
        var userId = "user123";
        var request = new CreateTaskRequest { Title = "Test Task" };

        _projectRepositoryMock.Setup(r => r.GetByIdAsync(projectId, default))
            .ReturnsAsync((Project?)null);

        var service = new CreateTaskService(
            _context,
            _projectRepositoryMock.Object,
            _taskRepositoryMock.Object,
            _hubContextMock.Object,
            _createTaskLoggerMock.Object);

        // Act & Assert
        await Assert.ThrowsAsync<KeyNotFoundException>(() =>
            service.CreateTaskAsync(projectId, userId, request));
    }

    #endregion

    #region GetTaskService Tests

    [Fact]
    public async System.Threading.Tasks.Task GetTask_WithValidTaskAndMembership_ShouldReturnTask()
    {
        // Arrange
        var taskId = 1;
        var projectId = 1;
        var userId = "user123";

        var creator = new ApplicationUser 
        { 
            Id = "creator123", 
            Email = "creator@example.com", 
            FirstName = "John", 
            LastName = "Doe" 
        };
        
        var task = new DomainTask
        {
            Id = taskId,
            ProjectId = projectId,
            Title = "Test Task",
            Description = "Task description",
            Status = DomainTaskStatus.InProgress,
            Priority = TaskPriority.Medium,
            CreatedBy = "creator123",
            CreatedTimestamp = DateTime.UtcNow,
            UpdatedTimestamp = DateTime.UtcNow,
            Creator = creator,
            Comments = new List<Comment>(),
            History = new List<TaskHistory>()
        };

        var project = new Project { Id = projectId, Name = "Test Project", OwnerId = userId };

        _taskRepositoryMock.Setup(r => r.GetByIdAsync(taskId, default))
            .ReturnsAsync(task);
        _projectRepositoryMock.Setup(r => r.GetByIdAsync(projectId, default))
            .ReturnsAsync(project);

        var service = new GetTaskService(_taskRepositoryMock.Object, _projectRepositoryMock.Object);

        // Act
        var result = await service.GetTaskAsync(taskId, userId);

        // Assert
        result.Should().NotBeNull();
        result.Title.Should().Be("Test Task");
        result.Status.Should().Be(DomainTaskStatus.InProgress);
        result.CreatedBy.Should().NotBeNull();
    }

    #endregion

    #region UpdateTaskStatusService Tests

    [Fact]
    public async System.Threading.Tasks.Task UpdateTaskStatus_WithValidStatusChange_ShouldSucceed()
    {
        // Arrange
        var taskId = 1;
        var projectId = 1;
        var userId = "user123";
        var request = new UpdateTaskStatusRequest { NewStatus = DomainTaskStatus.InProgress };

        var assignee = new ApplicationUser { Id = "assignee123", FirstName = "Jane", LastName = "Smith" };
        var task = new DomainTask
        {
            Id = taskId,
            ProjectId = projectId,
            Title = "Test Task",
            Status = DomainTaskStatus.ToDo,
            Priority = TaskPriority.Medium,
            CreatedBy = "creator123",
            AssigneeId = "assignee123",
            Assignee = assignee,
            CreatedTimestamp = DateTime.UtcNow,
            UpdatedTimestamp = DateTime.UtcNow
        };

        var project = new Project { Id = projectId, Name = "Test Project", OwnerId = userId };

        _taskRepositoryMock.Setup(r => r.GetByIdAsync(taskId, default))
            .ReturnsAsync(task);
        _projectRepositoryMock.Setup(r => r.GetByIdAsync(projectId, default))
            .ReturnsAsync(project);

        _context.ProjectMembers.Add(new ProjectMember
        {
            ProjectId = projectId,
            UserId = userId,
            Role = ProjectRole.Member,
            JoinedTimestamp = DateTime.UtcNow
        });
        _context.Tasks.Add(task);
        await _context.SaveChangesAsync();

        _taskRepositoryMock
            .Setup(r => r.UpdateAsync(task, default))
            .Returns(async () =>
            {
                _context.Tasks.Update(task);
                await _context.SaveChangesAsync();
            });

        var service = new UpdateTaskStatusService(
            _context,
            _taskRepositoryMock.Object,
            _projectRepositoryMock.Object,
            _hubContextMock.Object,
            _updateStatusLoggerMock.Object);

        // Act
        var result = await service.UpdateTaskStatusAsync(taskId, userId, request);

        // Assert
        result.Should().NotBeNull();
        result.Status.Should().Be(DomainTaskStatus.InProgress);
        task.Status.Should().Be(DomainTaskStatus.InProgress);
        _taskRepositoryMock.Verify(r => r.UpdateAsync(task, default), Times.Once);
        _context.TaskHistory.Count().Should().Be(1);
    }

    [Fact]
    public async System.Threading.Tasks.Task UpdateTaskStatus_WithSameStatus_ShouldSkipHistoryEntry()
    {
        // Arrange
        var taskId = 1;
        var projectId = 1;
        var userId = "user123";
        var request = new UpdateTaskStatusRequest { NewStatus = DomainTaskStatus.InProgress };

        var task = new DomainTask
        {
            Id = taskId,
            ProjectId = projectId,
            Title = "Test Task",
            Status = DomainTaskStatus.InProgress, // Same as request
            Priority = TaskPriority.Medium,
            CreatedBy = "creator123",
            CreatedTimestamp = DateTime.UtcNow,
            UpdatedTimestamp = DateTime.UtcNow
        };

        var project = new Project { Id = projectId, Name = "Test Project", OwnerId = userId };

        _taskRepositoryMock.Setup(r => r.GetByIdAsync(taskId, default))
            .ReturnsAsync(task);
        _projectRepositoryMock.Setup(r => r.GetByIdAsync(projectId, default))
            .ReturnsAsync(project);

        _context.ProjectMembers.Add(new ProjectMember
        {
            ProjectId = projectId,
            UserId = userId,
            Role = ProjectRole.Member,
            JoinedTimestamp = DateTime.UtcNow
        });
        await _context.SaveChangesAsync();

        var service = new UpdateTaskStatusService(
            _context,
            _taskRepositoryMock.Object,
            _projectRepositoryMock.Object,
            _hubContextMock.Object,
            _updateStatusLoggerMock.Object);

        // Act
        var result = await service.UpdateTaskStatusAsync(taskId, userId, request);

        // Assert
        result.Should().NotBeNull();
        result.Status.Should().Be(DomainTaskStatus.InProgress);
        _taskRepositoryMock.Verify(r => r.UpdateAsync(It.IsAny<DomainTask>(), default), Times.Never);
    }

    [Fact]
    public async System.Threading.Tasks.Task UpdateTaskStatus_WithoutProjectMembership_ShouldThrowUnauthorizedAccess()
    {
        // Arrange
        var taskId = 1;
        var projectId = 1;
        var userId = "user123";
        var request = new UpdateTaskStatusRequest { NewStatus = DomainTaskStatus.InProgress };

        var task = new DomainTask 
        { 
            Id = taskId, 
            ProjectId = projectId, 
            Title = "Test Task", 
            Status = DomainTaskStatus.ToDo 
        };
        
        var project = new Project { Id = projectId, Name = "Test Project", OwnerId = "owner123" };

        _taskRepositoryMock.Setup(r => r.GetByIdAsync(taskId, default))
            .ReturnsAsync(task);
        _projectRepositoryMock.Setup(r => r.GetByIdAsync(projectId, default))
            .ReturnsAsync(project);

        var service = new UpdateTaskStatusService(
            _context,
            _taskRepositoryMock.Object,
            _projectRepositoryMock.Object,
            _hubContextMock.Object,
            _updateStatusLoggerMock.Object);

        // Act & Assert
        await Assert.ThrowsAsync<UnauthorizedAccessException>(() =>
            service.UpdateTaskStatusAsync(taskId, userId, request));
    }

    #endregion
}

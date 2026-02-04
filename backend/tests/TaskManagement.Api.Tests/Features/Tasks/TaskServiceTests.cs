using FluentAssertions;
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
    private readonly Mock<TaskManagementDbContext> _contextMock;
    private readonly Mock<ITaskRepository> _taskRepositoryMock;
    private readonly Mock<IProjectRepository> _projectRepositoryMock;
    private readonly Mock<ILogger<CreateTaskService>> _createTaskLoggerMock;
    private readonly Mock<ILogger<UpdateTaskStatusService>> _updateStatusLoggerMock;

    public TaskServiceTests()
    {
        _contextMock = new Mock<TaskManagementDbContext>();
        _taskRepositoryMock = new Mock<ITaskRepository>();
        _projectRepositoryMock = new Mock<IProjectRepository>();
        _createTaskLoggerMock = new Mock<ILogger<CreateTaskService>>();
        _updateStatusLoggerMock = new Mock<ILogger<UpdateTaskStatusService>>();
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

        var user = new ApplicationUser { Id = userId, Email = "user@example.com" };
        var project = new Project { Id = projectId, Name = "Test Project", OwnerId = userId };

        _projectRepositoryMock.Setup(r => r.GetByIdAsync(projectId, default))
            .ReturnsAsync(project);

        var mockDbSet = new Mock<DbSet<ProjectMember>>();
        _contextMock.Setup(c => c.ProjectMembers).Returns(mockDbSet.Object);

        var service = new CreateTaskService(
            _contextMock.Object,
            _projectRepositoryMock.Object,
            _taskRepositoryMock.Object,
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
            _contextMock.Object,
            _projectRepositoryMock.Object,
            _taskRepositoryMock.Object,
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

        var mockProjectMembersDbSet = new Mock<DbSet<ProjectMember>>();
        mockProjectMembersDbSet.Setup(d => d.AnyAsync(It.IsAny<System.Linq.Expressions.Expression<System.Func<ProjectMember, bool>>>(), default))
            .ReturnsAsync(true);
        _contextMock.Setup(c => c.ProjectMembers).Returns(mockProjectMembersDbSet.Object);

        var mockTaskHistoryDbSet = new Mock<DbSet<TaskHistory>>();
        _contextMock.Setup(c => c.TaskHistory).Returns(mockTaskHistoryDbSet.Object);

        var service = new UpdateTaskStatusService(
            _contextMock.Object,
            _taskRepositoryMock.Object,
            _projectRepositoryMock.Object,
            _updateStatusLoggerMock.Object);

        // Act
        var result = await service.UpdateTaskStatusAsync(taskId, userId, request);

        // Assert
        result.Should().NotBeNull();
        result.Status.Should().Be(DomainTaskStatus.InProgress);
        task.Status.Should().Be(DomainTaskStatus.InProgress);
        _taskRepositoryMock.Verify(r => r.UpdateAsync(task, default), Times.Once);
        mockTaskHistoryDbSet.Verify(d => d.Add(It.IsAny<TaskHistory>()), Times.Once);
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

        var mockProjectMembersDbSet = new Mock<DbSet<ProjectMember>>();
        mockProjectMembersDbSet.Setup(d => d.AnyAsync(It.IsAny<System.Linq.Expressions.Expression<System.Func<ProjectMember, bool>>>(), default))
            .ReturnsAsync(true);
        _contextMock.Setup(c => c.ProjectMembers).Returns(mockProjectMembersDbSet.Object);

        var service = new UpdateTaskStatusService(
            _contextMock.Object,
            _taskRepositoryMock.Object,
            _projectRepositoryMock.Object,
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

        var mockProjectMembersDbSet = new Mock<DbSet<ProjectMember>>();
        mockProjectMembersDbSet.Setup(d => d.AnyAsync(It.IsAny<System.Linq.Expressions.Expression<System.Func<ProjectMember, bool>>>(), default))
            .ReturnsAsync(false);
        _contextMock.Setup(c => c.ProjectMembers).Returns(mockProjectMembersDbSet.Object);

        var service = new UpdateTaskStatusService(
            _contextMock.Object,
            _taskRepositoryMock.Object,
            _projectRepositoryMock.Object,
            _updateStatusLoggerMock.Object);

        // Act & Assert
        await Assert.ThrowsAsync<UnauthorizedAccessException>(() =>
            service.UpdateTaskStatusAsync(taskId, userId, request));
    }

    #endregion
}

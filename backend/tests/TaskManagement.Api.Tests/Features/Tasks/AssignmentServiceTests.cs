using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using TaskManagement.Api.Data;
using TaskManagement.Api.Data.Repositories;
using TaskManagement.Api.Domain.Notifications;
using TaskManagement.Api.Domain.Projects;
using TaskManagement.Api.Domain.Tasks;
using TaskManagement.Api.Domain.Users;
using TaskManagement.Api.Features.Tasks.AssignTask;
using Xunit;
using DomainTask = TaskManagement.Api.Domain.Tasks.Task;

namespace TaskManagement.Api.Tests.Features.Tasks;

/// <summary>
/// Unit tests for task assignment behavior, history recording, and authorization checks.
/// </summary>
public class AssignmentServiceTests
{
    private readonly TaskManagementDbContext _context;
    private readonly TaskRepository _taskRepository;
    private readonly ProjectRepository _projectRepository;
    private readonly Mock<ILogger<AssignTaskService>> _loggerMock;

    public AssignmentServiceTests()
    {
        var options = new DbContextOptionsBuilder<TaskManagementDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new TaskManagementDbContext(options);
        _taskRepository = new TaskRepository(_context);
        _projectRepository = new ProjectRepository(_context);
        _loggerMock = new Mock<ILogger<AssignTaskService>>();
    }

    [Fact]
    public async System.Threading.Tasks.Task AssignTask_WithManager_ShouldUpdateAssigneeAndRecordHistory()
    {
        // Arrange
        var project = SeedProject(out var owner, out var manager, out var assignee);
        var task = SeedTask(project, owner);

        var service = new AssignTaskService(
            _context,
            _taskRepository,
            _projectRepository,
            _loggerMock.Object);

        var request = new AssignTaskRequest { AssigneeId = assignee.Id };

        // Act
        var response = await service.AssignTaskAsync(task.Id, manager.Id, request);

        // Assert
        response.Should().NotBeNull();
        response.AssigneeId.Should().Be(assignee.Id);

        var updatedTask = await _taskRepository.GetByIdAsync(task.Id);
        updatedTask.Should().NotBeNull();
        updatedTask!.AssigneeId.Should().Be(assignee.Id);

        var historyEntry = await _context.TaskHistory
            .AsNoTracking()
            .FirstOrDefaultAsync(h => h.TaskId == task.Id && h.ChangeType == TaskHistoryChangeType.AssigneeChanged);
        historyEntry.Should().NotBeNull();
        historyEntry!.OldValue.Should().BeNull();
        historyEntry.NewValue.Should().Be(assignee.Id);

        var notification = await _context.Notifications
            .AsNoTracking()
            .FirstOrDefaultAsync(n => n.TaskId == task.Id && n.Type == NotificationType.TaskAssigned);
        notification.Should().NotBeNull();
        notification!.RecipientId.Should().Be(assignee.Id);
    }

    [Fact]
    public async System.Threading.Tasks.Task AssignTask_WithNonMember_ShouldThrowUnauthorizedAccess()
    {
        // Arrange
        var project = SeedProject(out var owner, out _, out var assignee);
        var task = SeedTask(project, owner);

        var nonMember = new ApplicationUser { Id = "user-outsider", Email = "outsider@example.com" };
        _context.Users.Add(nonMember);
        await _context.SaveChangesAsync();

        var service = new AssignTaskService(
            _context,
            _taskRepository,
            _projectRepository,
            _loggerMock.Object);

        var request = new AssignTaskRequest { AssigneeId = assignee.Id };

        // Act
        Func<System.Threading.Tasks.Task> action = () => service.AssignTaskAsync(task.Id, nonMember.Id, request);

        // Assert
        await action.Should().ThrowAsync<UnauthorizedAccessException>();
    }

    [Fact]
    public async System.Threading.Tasks.Task AssignTask_WithAssigneeNotInProject_ShouldThrowInvalidOperation()
    {
        // Arrange
        var project = SeedProject(out var owner, out var manager, out _);
        var task = SeedTask(project, owner);

        var outsiderAssignee = new ApplicationUser { Id = "user-remote", Email = "remote@example.com" };
        _context.Users.Add(outsiderAssignee);
        await _context.SaveChangesAsync();

        var service = new AssignTaskService(
            _context,
            _taskRepository,
            _projectRepository,
            _loggerMock.Object);

        var request = new AssignTaskRequest { AssigneeId = outsiderAssignee.Id };

        // Act
        Func<System.Threading.Tasks.Task> action = () => service.AssignTaskAsync(task.Id, manager.Id, request);

        // Assert
        await action.Should().ThrowAsync<InvalidOperationException>();
    }

    private Project SeedProject(out ApplicationUser owner, out ApplicationUser manager, out ApplicationUser member)
    {
        owner = new ApplicationUser { Id = "user-owner", Email = "owner@example.com" };
        manager = new ApplicationUser { Id = "user-manager", Email = "manager@example.com" };
        member = new ApplicationUser { Id = "user-member", Email = "member@example.com" };

        _context.Users.AddRange(owner, manager, member);

        var project = new Project
        {
            Id = 1,
            Name = "Assignment Project",
            OwnerId = owner.Id,
            Owner = owner,
            CreatedTimestamp = DateTime.UtcNow,
            UpdatedTimestamp = DateTime.UtcNow
        };

        _context.Projects.Add(project);
        _context.ProjectMembers.AddRange(
            new ProjectMember
            {
                ProjectId = project.Id,
                UserId = owner.Id,
                Role = ProjectRole.Owner,
                JoinedTimestamp = DateTime.UtcNow,
                User = owner,
                Project = project
            },
            new ProjectMember
            {
                ProjectId = project.Id,
                UserId = manager.Id,
                Role = ProjectRole.Manager,
                JoinedTimestamp = DateTime.UtcNow,
                User = manager,
                Project = project
            },
            new ProjectMember
            {
                ProjectId = project.Id,
                UserId = member.Id,
                Role = ProjectRole.Member,
                JoinedTimestamp = DateTime.UtcNow,
                User = member,
                Project = project
            });

        _context.SaveChanges();

        return project;
    }

    private DomainTask SeedTask(Project project, ApplicationUser owner)
    {
        var task = new DomainTask
        {
            Id = 1,
            ProjectId = project.Id,
            Project = project,
            Title = "Assignment Task",
            CreatedBy = owner.Id,
            Creator = owner,
            Status = TaskStatus.ToDo,
            Priority = TaskPriority.Medium,
            CreatedTimestamp = DateTime.UtcNow,
            UpdatedTimestamp = DateTime.UtcNow
        };

        _context.Tasks.Add(task);
        _context.SaveChanges();

        return task;
    }
}

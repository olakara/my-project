using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Moq;
using TaskManagement.Api.Data;
using TaskManagement.Api.Data.Repositories;
using TaskManagement.Api.Domain.Projects;
using TaskManagement.Api.Domain.Tasks;
using TaskManagement.Api.Domain.Users;
using TaskManagement.Api.Features.Dashboard.GetProjectMetrics;
using DomainTask = TaskManagement.Api.Domain.Tasks.Task;
using DomainTaskStatus = TaskManagement.Api.Domain.Tasks.TaskStatus;

namespace TaskManagement.Api.Tests.Features.Dashboard;

/// <summary>
/// Unit tests for project metrics calculations.
/// </summary>
public class MetricsServiceTests
{
    private readonly TaskManagementDbContext _context;
    private readonly Mock<IProjectRepository> _projectRepositoryMock;

    public MetricsServiceTests()
    {
        var options = new DbContextOptionsBuilder<TaskManagementDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        _context = new TaskManagementDbContext(options);
        _projectRepositoryMock = new Mock<IProjectRepository>();
    }

    [Fact]
    public async System.Threading.Tasks.Task GetProjectMetrics_WithValidMember_ReturnsStatusAndTeamStats()
    {
        // Arrange
        var owner = new ApplicationUser
        {
            Id = "owner-1",
            Email = "owner@example.com",
            FirstName = "Owner",
            LastName = "User"
        };

        var member = new ApplicationUser
        {
            Id = "member-1",
            Email = "member@example.com",
            FirstName = "Team",
            LastName = "Member"
        };

        var project = new Project
        {
            Id = 10,
            Name = "Metrics Project",
            OwnerId = owner.Id,
            Owner = owner
        };

        var ownerMembership = new ProjectMember
        {
            ProjectId = project.Id,
            Project = project,
            UserId = owner.Id,
            User = owner,
            Role = ProjectRole.Owner,
            JoinedTimestamp = DateTime.UtcNow.AddDays(-10)
        };

        var memberMembership = new ProjectMember
        {
            ProjectId = project.Id,
            Project = project,
            UserId = member.Id,
            User = member,
            Role = ProjectRole.Member,
            JoinedTimestamp = DateTime.UtcNow.AddDays(-5)
        };

        project.Members = new List<ProjectMember> { ownerMembership, memberMembership };

        _projectRepositoryMock
            .Setup(repo => repo.GetByIdAsync(project.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(project);

        _context.Users.AddRange(owner, member);
        _context.Projects.Add(project);
        _context.ProjectMembers.AddRange(ownerMembership, memberMembership);

        _context.Tasks.AddRange(
            new DomainTask
            {
                ProjectId = project.Id,
                Title = "Done task 1",
                Status = DomainTaskStatus.Done,
                Priority = TaskPriority.Low,
                AssigneeId = member.Id,
                Assignee = member,
                CreatedBy = owner.Id,
                Creator = owner,
                CreatedTimestamp = DateTime.UtcNow.AddDays(-3),
                UpdatedTimestamp = DateTime.UtcNow.AddDays(-1)
            },
            new DomainTask
            {
                ProjectId = project.Id,
                Title = "In progress task",
                Status = DomainTaskStatus.InProgress,
                Priority = TaskPriority.Medium,
                AssigneeId = member.Id,
                Assignee = member,
                CreatedBy = owner.Id,
                Creator = owner,
                CreatedTimestamp = DateTime.UtcNow.AddDays(-2),
                UpdatedTimestamp = DateTime.UtcNow.AddDays(-1)
            },
            new DomainTask
            {
                ProjectId = project.Id,
                Title = "Owner done task",
                Status = DomainTaskStatus.Done,
                Priority = TaskPriority.High,
                AssigneeId = owner.Id,
                Assignee = owner,
                CreatedBy = owner.Id,
                Creator = owner,
                CreatedTimestamp = DateTime.UtcNow.AddDays(-4),
                UpdatedTimestamp = DateTime.UtcNow.AddDays(-2)
            },
            new DomainTask
            {
                ProjectId = project.Id,
                Title = "Unassigned task",
                Status = DomainTaskStatus.ToDo,
                Priority = TaskPriority.Medium,
                AssigneeId = null,
                CreatedBy = owner.Id,
                Creator = owner,
                CreatedTimestamp = DateTime.UtcNow.AddDays(-1),
                UpdatedTimestamp = DateTime.UtcNow.AddDays(-1)
            });

        await _context.SaveChangesAsync();

        var service = new GetProjectMetricsService(_context, _projectRepositoryMock.Object);

        // Act
        var result = await service.GetProjectMetricsAsync(project.Id, owner.Id);

        // Assert
        result.ProjectId.Should().Be(project.Id);
        result.ProjectName.Should().Be(project.Name);
        result.TotalTasks.Should().Be(4);
        result.CompletedTasks.Should().Be(2);
        result.CompletionPercentage.Should().Be(50m);

        var statusLookup = result.StatusCounts.ToDictionary(entry => entry.Status, entry => entry.Count);
        statusLookup[DomainTaskStatus.ToDo].Should().Be(1);
        statusLookup[DomainTaskStatus.InProgress].Should().Be(1);
        statusLookup[DomainTaskStatus.InReview].Should().Be(0);
        statusLookup[DomainTaskStatus.Done].Should().Be(2);

        var memberStats = result.TeamMembers.Single(entry => entry.UserId == member.Id);
        memberStats.AssignedTasks.Should().Be(2);
        memberStats.CompletedTasks.Should().Be(1);

        var ownerStats = result.TeamMembers.Single(entry => entry.UserId == owner.Id);
        ownerStats.AssignedTasks.Should().Be(1);
        ownerStats.CompletedTasks.Should().Be(1);
    }

    [Fact]
    public async System.Threading.Tasks.Task GetProjectMetrics_WithMissingProject_ShouldThrowKeyNotFound()
    {
        // Arrange
        _projectRepositoryMock
            .Setup(repo => repo.GetByIdAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Project?)null);

        var service = new GetProjectMetricsService(_context, _projectRepositoryMock.Object);

        // Act
        var act = () => service.GetProjectMetricsAsync(999, "user-1");

        // Assert
        await act.Should().ThrowAsync<KeyNotFoundException>();
    }

    [Fact]
    public async System.Threading.Tasks.Task GetProjectMetrics_WithNonMember_ShouldThrowUnauthorized()
    {
        // Arrange
        var project = new Project
        {
            Id = 12,
            Name = "Restricted Project",
            OwnerId = "owner-2",
            Owner = new ApplicationUser { Id = "owner-2", Email = "owner2@example.com" },
            Members = new List<ProjectMember>()
        };

        _projectRepositoryMock
            .Setup(repo => repo.GetByIdAsync(project.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(project);

        var service = new GetProjectMetricsService(_context, _projectRepositoryMock.Object);

        // Act
        var act = () => service.GetProjectMetricsAsync(project.Id, "outsider");

        // Assert
        await act.Should().ThrowAsync<UnauthorizedAccessException>();
    }
}
